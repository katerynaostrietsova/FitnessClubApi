using FitnessClubApi.Data;
using FitnessClubApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessClubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RegistrationsController : ControllerBase
{
    private readonly FitnessClubContext _context;

    public RegistrationsController(FitnessClubContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetRegistrations()
    {
        var registrations = await _context.Registrations
            .Include(r => r.Client)
            .Include(r => r.Workout)
            .Include(r => r.WorkoutType)
            .Include(r => r.Subscription)
            .Select(r => new
            {
                r.Id,
                r.ClientId,
                ClientName = r.Client != null ? r.Client.FullName : null,
                r.WorkoutId,
                WorkoutDateTime = r.Workout != null ? r.Workout.WorkoutDateTime : (DateTime?)null,
                r.SubscriptionId,
                r.WorkoutTypeId,
                WorkoutTypeName = r.WorkoutType != null ? r.WorkoutType.Name : null,
                r.RegistrationDateTime,
                r.Status,
                r.Note
            })
            .ToListAsync();

        return Ok(registrations);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRegistration(int id)
    {
        var registration = await _context.Registrations
            .Include(r => r.Client)
            .Include(r => r.Workout)
            .Include(r => r.WorkoutType)
            .Include(r => r.Subscription)
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Id,
                r.ClientId,
                ClientName = r.Client != null ? r.Client.FullName : null,
                r.WorkoutId,
                WorkoutDateTime = r.Workout != null ? r.Workout.WorkoutDateTime : (DateTime?)null,
                r.SubscriptionId,
                r.WorkoutTypeId,
                WorkoutTypeName = r.WorkoutType != null ? r.WorkoutType.Name : null,
                r.RegistrationDateTime,
                r.Status,
                r.Note
            })
            .FirstOrDefaultAsync();

        if (registration == null)
        {
            return NotFound("Запис на тренування не знайдено.");
        }

        return Ok(registration);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRegistration(CreateRegistrationRequest request)
    {
        var clientExists = await _context.Clients
            .AnyAsync(c => c.Id == request.ClientId);

        if (!clientExists)
        {
            return BadRequest("Обраного клієнта не існує.");
        }

        var workout = await _context.Workouts
            .Include(w => w.WorkoutType)
            .FirstOrDefaultAsync(w => w.Id == request.WorkoutId);

        if (workout == null)
        {
            return BadRequest("Обраного тренування не існує.");
        }

        if (workout.Status != WorkoutStatus.Scheduled)
        {
            return BadRequest("Запис можливий тільки на заплановане тренування.");
        }

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId);

        if (subscription == null)
        {
            return BadRequest("Обраного абонемента не існує.");
        }

        if (subscription.ClientId != request.ClientId)
        {
            return BadRequest("Абонемент не належить обраному клієнту.");
        }

        if (subscription.WorkoutTypeId != workout.WorkoutTypeId)
        {
            return BadRequest("Абонемент не відповідає виду обраного тренування.");
        }

        if (subscription.Status != SubscriptionStatus.Active)
        {
            return BadRequest("Абонемент не є активним.");
        }

        if (subscription.RemainingSessions <= 0)
        {
            return BadRequest("На абонементі не залишилося тренувань.");
        }

        var today = DateTime.Today;

        if (subscription.StartDate.Date > today || subscription.EndDate.Date < today)
        {
            return BadRequest("Абонемент не діє на поточну дату.");
        }

        var duplicateRegistration = await _context.Registrations
            .AnyAsync(r => r.ClientId == request.ClientId &&
                           r.WorkoutId == request.WorkoutId);

        if (duplicateRegistration)
        {
            return BadRequest("Клієнт уже має запис на це тренування.");
        }

        if (workout.WorkoutType != null && workout.WorkoutType.IsGroup)
        {
            var registeredCount = await _context.Registrations
                .CountAsync(r => r.WorkoutId == request.WorkoutId &&
                                 r.Status != RegistrationStatus.Cancelled);

            if (registeredCount >= workout.MaxParticipants)
            {
                return BadRequest("На це тренування немає вільних місць.");
            }
        }

        var registration = new Registration
        {
            ClientId = request.ClientId,
            WorkoutId = request.WorkoutId,
            SubscriptionId = request.SubscriptionId,
            WorkoutTypeId = workout.WorkoutTypeId,
            RegistrationDateTime = DateTime.Now,
            Status = RegistrationStatus.Booked,
            Note = request.Note
        };

        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRegistration), new { id = registration.Id }, new
        {
            registration.Id,
            registration.ClientId,
            registration.WorkoutId,
            registration.SubscriptionId,
            registration.WorkoutTypeId,
            registration.RegistrationDateTime,
            registration.Status,
            registration.Note
        });
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateRegistrationStatus(
        int id,
        UpdateRegistrationStatusRequest request)
    {
        var registration = await _context.Registrations
            .Include(r => r.Subscription)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (registration == null)
        {
            return NotFound("Запис на тренування не знайдено.");
        }

        if (registration.Subscription == null)
        {
            return BadRequest("Для запису не знайдено абонемент.");
        }

        var oldStatus = registration.Status;
        var newStatus = request.Status;

        if (oldStatus == RegistrationStatus.Attended &&
            newStatus != RegistrationStatus.Attended)
        {
            registration.Subscription.RemainingSessions++;
        }

        if (oldStatus != RegistrationStatus.Attended &&
            newStatus == RegistrationStatus.Attended)
        {
            if (registration.Subscription.RemainingSessions <= 0)
            {
                return BadRequest("На абонементі не залишилося тренувань.");
            }

            registration.Subscription.RemainingSessions--;
        }

        registration.Status = newStatus;
        registration.Note = request.Note;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRegistration(int id)
    {
        var registration = await _context.Registrations
            .Include(r => r.Subscription)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (registration == null)
        {
            return NotFound("Запис на тренування не знайдено.");
        }

        if (registration.Status == RegistrationStatus.Attended &&
            registration.Subscription != null)
        {
            registration.Subscription.RemainingSessions++;
        }

        _context.Registrations.Remove(registration);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateRegistrationRequest
{
    public int ClientId { get; set; }

    public int WorkoutId { get; set; }

    public int SubscriptionId { get; set; }

    public string? Note { get; set; }
}

public class UpdateRegistrationStatusRequest
{
    public RegistrationStatus Status { get; set; }

    public string? Note { get; set; }
}