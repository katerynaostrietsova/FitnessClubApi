namespace FitnessClubApi.Models;

public class Trainer
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }

    public string? Specialization { get; set; }

    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
}