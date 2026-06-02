namespace FitnessClubApi.Models;

public class Client
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string MembershipCardNumber { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}