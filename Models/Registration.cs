namespace FitnessClubApi.Models;

public class Registration
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int WorkoutId { get; set; }

    public int SubscriptionId { get; set; }

    public int WorkoutTypeId { get; set; }

    public DateTime RegistrationDateTime { get; set; } = DateTime.Now;

    public RegistrationStatus Status { get; set; }

    public string? Note { get; set; }

    public Client? Client { get; set; }

    public Workout? Workout { get; set; }

    public Subscription? Subscription { get; set; }

    public WorkoutType? WorkoutType { get; set; }
}