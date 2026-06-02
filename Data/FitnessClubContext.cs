using FitnessClubApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessClubApi.Data;

public class FitnessClubContext : DbContext
{
    public FitnessClubContext(DbContextOptions<FitnessClubContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<WorkoutType> WorkoutTypes => Set<WorkoutType>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.MembershipCardNumber)
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(e => e.MembershipCardNumber)
                .IsUnique();

            entity.Property(e => e.Phone)
                .HasMaxLength(20);
        });

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Specialization)
                .HasMaxLength(100);

            entity.ToTable(t =>
                t.HasCheckConstraint("CK_Trainers_ExperienceYears", "[ExperienceYears] >= 0"));
        });

        modelBuilder.Entity<WorkoutType>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(255);

            entity.ToTable(t =>
                t.HasCheckConstraint("CK_WorkoutTypes_DurationMinutes", "[DurationMinutes] > 0"));
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.Price)
                .HasColumnType("decimal(10,2)");

            entity.HasOne(e => e.Client)
                .WithMany(e => e.Subscriptions)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WorkoutType)
                .WithMany(e => e.Subscriptions)
                .HasForeignKey(e => e.WorkoutTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasAlternateKey(e => new
            {
                e.Id,
                e.ClientId,
                e.WorkoutTypeId
            });

            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Subscriptions_TotalSessions", "[TotalSessions] > 0");
                t.HasCheckConstraint("CK_Subscriptions_RemainingSessions_Min", "[RemainingSessions] >= 0");
                t.HasCheckConstraint("CK_Subscriptions_RemainingSessions_Max", "[RemainingSessions] <= [TotalSessions]");
                t.HasCheckConstraint("CK_Subscriptions_Dates", "[EndDate] >= [StartDate]");
                t.HasCheckConstraint("CK_Subscriptions_Price", "[Price] >= 0");
            });
        });

        modelBuilder.Entity<Workout>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(e => e.WorkoutType)
                .WithMany(e => e.Workouts)
                .HasForeignKey(e => e.WorkoutTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Trainer)
                .WithMany(e => e.Workouts)
                .HasForeignKey(e => e.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasAlternateKey(e => new
            {
                e.Id,
                e.WorkoutTypeId
            });

            entity.HasIndex(e => new
            {
                e.TrainerId,
                e.WorkoutDateTime
            })
            .IsUnique()
            .HasFilter("[TrainerId] IS NOT NULL");

            entity.ToTable(t =>
                t.HasCheckConstraint("CK_Workouts_MaxParticipants", "[MaxParticipants] > 0"));
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.Note)
                .HasMaxLength(255);

            entity.HasIndex(e => new
            {
                e.ClientId,
                e.WorkoutId
            })
            .IsUnique();

            entity.HasOne(e => e.Client)
                .WithMany(e => e.Registrations)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WorkoutType)
                .WithMany(e => e.Registrations)
                .HasForeignKey(e => e.WorkoutTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Subscription)
                .WithMany(e => e.Registrations)
                .HasForeignKey(e => new
                {
                    e.SubscriptionId,
                    e.ClientId,
                    e.WorkoutTypeId
                })
                .HasPrincipalKey(e => new
                {
                    e.Id,
                    e.ClientId,
                    e.WorkoutTypeId
                })
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Workout)
                .WithMany(e => e.Registrations)
                .HasForeignKey(e => new
                {
                    e.WorkoutId,
                    e.WorkoutTypeId
                })
                .HasPrincipalKey(e => new
                {
                    e.Id,
                    e.WorkoutTypeId
                })
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}