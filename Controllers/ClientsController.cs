using FitnessClubApi.Data;
using FitnessClubApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessClubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly FitnessClubContext _context;

    public ClientsController(FitnessClubContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetClients()
    {
        var clients = await _context.Clients
            .Select(c => new
            {
                c.Id,
                c.FullName,
                c.MembershipCardNumber,
                c.Phone,
                c.CreatedAt
            })
            .ToListAsync();

        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClient(int id)
    {
        var client = await _context.Clients
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.FullName,
                c.MembershipCardNumber,
                c.Phone,
                c.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (client == null)
        {
            return NotFound("Клієнта не знайдено.");
        }

        return Ok(client);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient(CreateClientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest("ПІБ клієнта не може бути порожнім.");
        }

        if (string.IsNullOrWhiteSpace(request.MembershipCardNumber))
        {
            return BadRequest("Номер членської карти не може бути порожнім.");
        }

        var exists = await _context.Clients
            .AnyAsync(c => c.MembershipCardNumber == request.MembershipCardNumber);

        if (exists)
        {
            return BadRequest("Клієнт з таким номером членської карти вже існує.");
        }

        var client = new Client
        {
            FullName = request.FullName,
            MembershipCardNumber = request.MembershipCardNumber,
            Phone = request.Phone,
            CreatedAt = DateTime.Now
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClient), new { id = client.Id }, new
        {
            client.Id,
            client.FullName,
            client.MembershipCardNumber,
            client.Phone,
            client.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClient(int id, UpdateClientRequest request)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client == null)
        {
            return NotFound("Клієнта не знайдено.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest("ПІБ клієнта не може бути порожнім.");
        }

        if (string.IsNullOrWhiteSpace(request.MembershipCardNumber))
        {
            return BadRequest("Номер членської карти не може бути порожнім.");
        }

        var cardExists = await _context.Clients
            .AnyAsync(c => c.Id != id && c.MembershipCardNumber == request.MembershipCardNumber);

        if (cardExists)
        {
            return BadRequest("Інший клієнт вже має такий номер членської карти.");
        }

        client.FullName = request.FullName;
        client.MembershipCardNumber = request.MembershipCardNumber;
        client.Phone = request.Phone;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);

        if (client == null)
        {
            return NotFound("Клієнта не знайдено.");
        }

        var hasSubscriptions = await _context.Subscriptions
            .AnyAsync(s => s.ClientId == id);

        var hasRegistrations = await _context.Registrations
            .AnyAsync(r => r.ClientId == id);

        if (hasSubscriptions || hasRegistrations)
        {
            return BadRequest("Неможливо видалити клієнта, бо з ним пов'язані абонементи або записи на тренування.");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateClientRequest
{
    public string FullName { get; set; } = string.Empty;

    public string MembershipCardNumber { get; set; } = string.Empty;

    public string? Phone { get; set; }
}

public class UpdateClientRequest
{
    public string FullName { get; set; } = string.Empty;

    public string MembershipCardNumber { get; set; } = string.Empty;

    public string? Phone { get; set; }
}