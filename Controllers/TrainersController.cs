using FitnessClubApi.Data;
using FitnessClubApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessClubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TrainersController : ControllerBase
{
    private readonly FitnessClubContext _context;

    public TrainersController(FitnessClubContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrainers()
    {
        var trainers = await _context.Trainers
            .Select(t => new
            {
                t.Id,
                t.FullName,
                t.ExperienceYears,
                t.Specialization
            })
            .ToListAsync();

        return Ok(trainers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTrainer(int id)
    {
        var trainer = await _context.Trainers
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.FullName,
                t.ExperienceYears,
                t.Specialization
            })
            .FirstOrDefaultAsync();

        if (trainer == null)
        {
            return NotFound("Тренера не знайдено.");
        }

        return Ok(trainer);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTrainer(CreateTrainerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest("ПІБ тренера не може бути порожнім.");
        }

        if (request.ExperienceYears < 0)
        {
            return BadRequest("Досвід роботи не може бути від'ємним.");
        }

        var trainer = new Trainer
        {
            FullName = request.FullName,
            ExperienceYears = request.ExperienceYears,
            Specialization = request.Specialization
        };

        _context.Trainers.Add(trainer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTrainer), new { id = trainer.Id }, new
        {
            trainer.Id,
            trainer.FullName,
            trainer.ExperienceYears,
            trainer.Specialization
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTrainer(int id, UpdateTrainerRequest request)
    {
        var trainer = await _context.Trainers.FindAsync(id);

        if (trainer == null)
        {
            return NotFound("Тренера не знайдено.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest("ПІБ тренера не може бути порожнім.");
        }

        if (request.ExperienceYears < 0)
        {
            return BadRequest("Досвід роботи не може бути від'ємним.");
        }

        trainer.FullName = request.FullName;
        trainer.ExperienceYears = request.ExperienceYears;
        trainer.Specialization = request.Specialization;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrainer(int id)
    {
        var trainer = await _context.Trainers.FindAsync(id);

        if (trainer == null)
        {
            return NotFound("Тренера не знайдено.");
        }

        var hasWorkouts = await _context.Workouts
            .AnyAsync(w => w.TrainerId == id);

        if (hasWorkouts)
        {
            return BadRequest("Неможливо видалити тренера, бо з ним пов'язані тренування.");
        }

        _context.Trainers.Remove(trainer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateTrainerRequest
{
    public string FullName { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }

    public string? Specialization { get; set; }
}

public class UpdateTrainerRequest
{
    public string FullName { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }

    public string? Specialization { get; set; }
}