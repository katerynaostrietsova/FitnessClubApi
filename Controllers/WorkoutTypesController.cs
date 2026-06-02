using FitnessClubApi.Data;
using FitnessClubApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessClubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkoutTypesController : ControllerBase
{
    private readonly FitnessClubContext _context;

    public WorkoutTypesController(FitnessClubContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkoutTypes()
    {
        var workoutTypes = await _context.WorkoutTypes
            .Select(wt => new
            {
                wt.Id,
                wt.Name,
                wt.DurationMinutes,
                wt.IsGroup,
                wt.RequiresTrainer,
                wt.Description
            })
            .ToListAsync();

        return Ok(workoutTypes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkoutType(int id)
    {
        var workoutType = await _context.WorkoutTypes
            .Where(wt => wt.Id == id)
            .Select(wt => new
            {
                wt.Id,
                wt.Name,
                wt.DurationMinutes,
                wt.IsGroup,
                wt.RequiresTrainer,
                wt.Description
            })
            .FirstOrDefaultAsync();

        if (workoutType == null)
        {
            return NotFound("Вид тренування не знайдено.");
        }

        return Ok(workoutType);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkoutType(CreateWorkoutTypeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Назва виду тренування не може бути порожньою.");
        }

        if (request.DurationMinutes <= 0)
        {
            return BadRequest("Тривалість тренування має бути більшою за 0.");
        }

        var workoutType = new WorkoutType
        {
            Name = request.Name,
            DurationMinutes = request.DurationMinutes,
            IsGroup = request.IsGroup,
            RequiresTrainer = request.RequiresTrainer,
            Description = request.Description
        };

        _context.WorkoutTypes.Add(workoutType);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkoutType), new { id = workoutType.Id }, new
        {
            workoutType.Id,
            workoutType.Name,
            workoutType.DurationMinutes,
            workoutType.IsGroup,
            workoutType.RequiresTrainer,
            workoutType.Description
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkoutType(int id, UpdateWorkoutTypeRequest request)
    {
        var workoutType = await _context.WorkoutTypes.FindAsync(id);

        if (workoutType == null)
        {
            return NotFound("Вид тренування не знайдено.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Назва виду тренування не може бути порожньою.");
        }

        if (request.DurationMinutes <= 0)
        {
            return BadRequest("Тривалість тренування має бути більшою за 0.");
        }

        workoutType.Name = request.Name;
        workoutType.DurationMinutes = request.DurationMinutes;
        workoutType.IsGroup = request.IsGroup;
        workoutType.RequiresTrainer = request.RequiresTrainer;
        workoutType.Description = request.Description;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkoutType(int id)
    {
        var workoutType = await _context.WorkoutTypes.FindAsync(id);

        if (workoutType == null)
        {
            return NotFound("Вид тренування не знайдено.");
        }

        var hasWorkouts = await _context.Workouts
            .AnyAsync(w => w.WorkoutTypeId == id);

        var hasSubscriptions = await _context.Subscriptions
            .AnyAsync(s => s.WorkoutTypeId == id);

        if (hasWorkouts || hasSubscriptions)
        {
            return BadRequest("Неможливо видалити вид тренування, бо з ним пов'язані тренування або абонементи.");
        }

        _context.WorkoutTypes.Remove(workoutType);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateWorkoutTypeRequest
{
    public string Name { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public bool IsGroup { get; set; }

    public bool RequiresTrainer { get; set; }

    public string? Description { get; set; }
}

public class UpdateWorkoutTypeRequest
{
    public string Name { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public bool IsGroup { get; set; }

    public bool RequiresTrainer { get; set; }

    public string? Description { get; set; }
}