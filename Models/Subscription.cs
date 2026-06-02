namespace FitnessClubApi.Models;

public class Subscription
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int WorkoutTypeId { get; set; }

    public int TotalSessions { get; set; }

    public int RemainingSessions { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public SubscriptionStatus Status { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Client? Client { get; set; }

    public WorkoutType? WorkoutType { get; set; }

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}