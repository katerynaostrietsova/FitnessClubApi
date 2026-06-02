using FitnessClubApi.Data;
using FitnessClubApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessClubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkoutsController : ControllerBase
{
    private readonly FitnessClubContext _context;

    public WorkoutsController(FitnessClubContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkouts()
    {
        var workouts = await _context.Workouts
            .Include(w => w.WorkoutType)
            .Include(w => w.Trainer)
            .Select(w => new
            {
                w.Id,
                w.WorkoutTypeId,
                WorkoutTypeName = w.WorkoutType != null ? w.WorkoutType.Name : null,
                w.TrainerId,
                TrainerName = w.Trainer != null ? w.Trainer.FullName : null,
                w.WorkoutDateTime,
                w.MaxParticipants,
                w.Status
            })
            .ToListAsync();

        return Ok(workouts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkout(int id)
    {
        var workout = await _context.Workouts
            .Include(w => w.WorkoutType)
            .Include(w => w.Trainer)
            .Where(w => w.Id == id)
            .Select(w => new
            {
                w.Id,
                w.WorkoutTypeId,
                WorkoutTypeName = w.WorkoutType != null ? w.WorkoutType.Name : null,
                w.TrainerId,
                TrainerName = w.Trainer != null ? w.Trainer.FullName : null,
                w.WorkoutDateTime,
                w.MaxParticipants,
                w.Status
            })
            .FirstOrDefaultAsync();

        if (workout == null)
        {
            return NotFound("Тренування не знайдено.");
        }

        return Ok(workout);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkout(CreateWorkoutRequest request)
    {
        var workoutType = await _context.WorkoutTypes.FindAsync(request.WorkoutTypeId);

        if (workoutType == null)
        {
            return BadRequest("Обраного виду тренування не існує.");
        }

        if (request.MaxParticipants <= 0)
        {
            return BadRequest("Кількість місць має бути більшою за 0.");
        }

        if (workoutType.RequiresTrainer && request.TrainerId == null)
        {
            return BadRequest("Для цього виду тренування потрібно вказати тренера.");
        }

        if (!workoutType.RequiresTrainer && request.TrainerId != null)
        {
            return BadRequest("Для цього виду тренування тренер не потрібен.");
        }

        if (request.TrainerId != null)
        {
            var trainerExists = await _context.Trainers
                .AnyAsync(t => t.Id == request.TrainerId);

            if (!trainerExists)
            {
                return BadRequest("Обраного тренера не існує.");
            }

            var trainerBusy = await _context.Workouts
                .AnyAsync(w => w.TrainerId == request.TrainerId
                               && w.WorkoutDateTime == request.WorkoutDateTime);

            if (trainerBusy)
            {
                return BadRequest("Тренер уже має тренування на цей час.");
            }
        }

        var workout = new Workout
        {
            WorkoutTypeId = request.WorkoutTypeId,
            TrainerId = request.TrainerId,
            WorkoutDateTime = request.WorkoutDateTime,
            MaxParticipants = request.MaxParticipants,
            Status = WorkoutStatus.Scheduled
        };

        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkout), new { id = workout.Id }, new
        {
            workout.Id,
            workout.WorkoutTypeId,
            workout.TrainerId,
            workout.WorkoutDateTime,
            workout.MaxParticipants,
            workout.Status
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkout(int id, UpdateWorkoutRequest request)
    {
        var workout = await _context.Workouts.FindAsync(id);

        if (workout == null)
        {
            return NotFound("Тренування не знайдено.");
        }

        var workoutType = await _context.WorkoutTypes.FindAsync(request.WorkoutTypeId);

        if (workoutType == null)
        {
            return BadRequest("Обраного виду тренування не існує.");
        }

        if (request.MaxParticipants <= 0)
        {
            return BadRequest("Кількість місць має бути більшою за 0.");
        }

        if (workoutType.RequiresTrainer && request.TrainerId == null)
        {
            return BadRequest("Для цього виду тренування потрібно вказати тренера.");
        }

        if (!workoutType.RequiresTrainer && request.TrainerId != null)
        {
            return BadRequest("Для цього виду тренування тренер не потрібен.");
        }

        if (request.TrainerId != null)
        {
            var trainerExists = await _context.Trainers
                .AnyAsync(t => t.Id == request.TrainerId);

            if (!trainerExists)
            {
                return BadRequest("Обраного тренера не існує.");
            }

            var trainerBusy = await _context.Workouts
                .AnyAsync(w => w.Id != id
                               && w.TrainerId == request.TrainerId
                               && w.WorkoutDateTime == request.WorkoutDateTime);

            if (trainerBusy)
            {
                return BadRequest("Тренер уже має тренування на цей час.");
            }
        }

        var hasRegistrations = await _context.Registrations
            .AnyAsync(r => r.WorkoutId == id);

        if (hasRegistrations && request.WorkoutTypeId != workout.WorkoutTypeId)
        {
            return BadRequest("Неможливо змінити вид тренування, бо на нього вже є записи клієнтів.");
        }

        var activeRegistrationsCount = await _context.Registrations
            .CountAsync(r => r.WorkoutId == id && r.Status != RegistrationStatus.Cancelled);

        if (request.MaxParticipants < activeRegistrationsCount)
        {
            return BadRequest("Кількість місць не може бути меншою за кількість записаних клієнтів.");
        }

        workout.WorkoutTypeId = request.WorkoutTypeId;
        workout.TrainerId = request.TrainerId;
        workout.WorkoutDateTime = request.WorkoutDateTime;
        workout.MaxParticipants = request.MaxParticipants;
        workout.Status = request.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkout(int id)
    {
        var workout = await _context.Workouts.FindAsync(id);

        if (workout == null)
        {
            return NotFound("Тренування не знайдено.");
        }

        var hasRegistrations = await _context.Registrations
            .AnyAsync(r => r.WorkoutId == id);

        if (hasRegistrations)
        {
            return BadRequest("Неможливо видалити тренування, бо на нього є записи клієнтів.");
        }

        _context.Workouts.Remove(workout);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateWorkoutRequest
{
    public int WorkoutTypeId { get; set; }

    public int? TrainerId { get; set; }

    public DateTime WorkoutDateTime { get; set; }

    public int MaxParticipants { get; set; }
}

public class UpdateWorkoutRequest
{
    public int WorkoutTypeId { get; set; }

    public int? TrainerId { get; set; }

    public DateTime WorkoutDateTime { get; set; }

    public int MaxParticipants { get; set; }

    public WorkoutStatus Status { get; set; }
}