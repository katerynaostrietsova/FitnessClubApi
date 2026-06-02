using FitnessClubApi.Data;
using FitnessClubApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessClubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubscriptionsController : ControllerBase
{
    private readonly FitnessClubContext _context;

    public SubscriptionsController(FitnessClubContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSubscriptions()
    {
        var subscriptions = await _context.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.WorkoutType)
            .Select(s => new
            {
                s.Id,
                s.ClientId,
                ClientName = s.Client != null ? s.Client.FullName : null,
                s.WorkoutTypeId,
                WorkoutTypeName = s.WorkoutType != null ? s.WorkoutType.Name : null,
                s.TotalSessions,
                s.RemainingSessions,
                s.StartDate,
                s.EndDate,
                s.Status,
                s.Price,
                s.CreatedAt
            })
            .ToListAsync();

        return Ok(subscriptions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubscription(int id)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.WorkoutType)
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.ClientId,
                ClientName = s.Client != null ? s.Client.FullName : null,
                s.WorkoutTypeId,
                WorkoutTypeName = s.WorkoutType != null ? s.WorkoutType.Name : null,
                s.TotalSessions,
                s.RemainingSessions,
                s.StartDate,
                s.EndDate,
                s.Status,
                s.Price,
                s.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (subscription == null)
        {
            return NotFound("Абонемент не знайдено.");
        }

        return Ok(subscription);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubscription(CreateSubscriptionRequest request)
    {
        var clientExists = await _context.Clients
            .AnyAsync(c => c.Id == request.ClientId);

        if (!clientExists)
        {
            return BadRequest("Обраного клієнта не існує.");
        }

        var workoutTypeExists = await _context.WorkoutTypes
            .AnyAsync(wt => wt.Id == request.WorkoutTypeId);

        if (!workoutTypeExists)
        {
            return BadRequest("Обраного виду тренування не існує.");
        }

        if (request.TotalSessions <= 0)
        {
            return BadRequest("Кількість тренувань в абонементі має бути більшою за 0.");
        }

        if (request.EndDate < request.StartDate)
        {
            return BadRequest("Дата завершення не може бути раніше дати початку.");
        }

        if (request.Price < 0)
        {
            return BadRequest("Ціна не може бути від'ємною.");
        }

        var subscription = new Subscription
        {
            ClientId = request.ClientId,
            WorkoutTypeId = request.WorkoutTypeId,
            TotalSessions = request.TotalSessions,
            RemainingSessions = request.TotalSessions,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = SubscriptionStatus.Active,
            Price = request.Price,
            CreatedAt = DateTime.Now
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, new
        {
            subscription.Id,
            subscription.ClientId,
            subscription.WorkoutTypeId,
            subscription.TotalSessions,
            subscription.RemainingSessions,
            subscription.StartDate,
            subscription.EndDate,
            subscription.Status,
            subscription.Price,
            subscription.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSubscription(int id, UpdateSubscriptionRequest request)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);

        if (subscription == null)
        {
            return NotFound("Абонемент не знайдено.");
        }

        if (request.TotalSessions <= 0)
        {
            return BadRequest("Загальна кількість тренувань має бути більшою за 0.");
        }

        if (request.RemainingSessions < 0 || request.RemainingSessions > request.TotalSessions)
        {
            return BadRequest("Залишок тренувань має бути від 0 до загальної кількості тренувань.");
        }

        if (request.EndDate < request.StartDate)
        {
            return BadRequest("Дата завершення не може бути раніше дати початку.");
        }

        if (request.Price < 0)
        {
            return BadRequest("Ціна не може бути від'ємною.");
        }

        subscription.TotalSessions = request.TotalSessions;
        subscription.RemainingSessions = request.RemainingSessions;
        subscription.StartDate = request.StartDate;
        subscription.EndDate = request.EndDate;
        subscription.Status = request.Status;
        subscription.Price = request.Price;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSubscription(int id)
    {
        var subscription = await _context.Subscriptions.FindAsync(id);

        if (subscription == null)
        {
            return NotFound("Абонемент не знайдено.");
        }

        var hasRegistrations = await _context.Registrations
            .AnyAsync(r => r.SubscriptionId == id);

        if (hasRegistrations)
        {
            return BadRequest("Неможливо видалити абонемент, бо він використовується у записах на тренування.");
        }

        _context.Subscriptions.Remove(subscription);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateSubscriptionRequest
{
    public int ClientId { get; set; }

    public int WorkoutTypeId { get; set; }

    public int TotalSessions { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal Price { get; set; }
}

public class UpdateSubscriptionRequest
{
    public int TotalSessions { get; set; }

    public int RemainingSessions { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public SubscriptionStatus Status { get; set; }

    public decimal Price { get; set; }
}