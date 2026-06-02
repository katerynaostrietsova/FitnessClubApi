namespace FitnessClubApi.Models;

public class WorkoutType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public bool IsGroup { get; set; }

    public bool RequiresTrainer { get; set; }

    public string? Description { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}