namespace FitnessClubApi.Models;

public class Workout
{
    public int Id { get; set; }

    public int WorkoutTypeId { get; set; }

    public int? TrainerId { get; set; }

    public DateTime WorkoutDateTime { get; set; }

    public int MaxParticipants { get; set; }

    public WorkoutStatus Status { get; set; }

    public WorkoutType? WorkoutType { get; set; }

    public Trainer? Trainer { get; set; }

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}