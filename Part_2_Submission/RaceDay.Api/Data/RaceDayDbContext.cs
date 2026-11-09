using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using RaceDayRoute = RaceDay.Api.Models.Route;

namespace RaceDay.Api.Data;

public sealed class RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<RaceDayRoute> Routes => Set<RaceDayRoute>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.UserId);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.FirstName).HasMaxLength(60).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(60).IsRequired();
            e.Property(x => x.Email).HasMaxLength(255).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.ProfileImageUrl).HasMaxLength(1000);
            e.ToTable(t => t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Organiser','Participant')"));
        });
        modelBuilder.Entity<Event>(e =>
        {
            e.HasKey(x => x.EventId);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Venue).HasMaxLength(150).IsRequired();
            e.Property(x => x.City).HasMaxLength(100).IsRequired();
            e.Property(x => x.Province).HasMaxLength(100).IsRequired();
            e.Property(x => x.DistanceKm).HasPrecision(6, 2);
            e.Property(x => x.BannerImageUrl).HasMaxLength(1000);
            e.Property(x => x.EventType).HasMaxLength(20);
            e.Property(x => x.Status).HasMaxLength(20);
            e.HasOne(x => x.Organiser).WithMany(x => x.OrganisedEvents).HasForeignKey(x => x.OrganiserId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Events_Distance", "[DistanceKm] > 0");
                t.HasCheckConstraint("CK_Events_Type", "[EventType] IN ('Running','Walking','Cycling')");
                t.HasCheckConstraint("CK_Events_RegistrationDates", "[RegistrationClose] >= [RegistrationOpen]");
                t.HasCheckConstraint("CK_Events_EventAfterRegistration", "[EventDate] >= [RegistrationOpen]");
                t.HasCheckConstraint("CK_Events_Status", "[Status] IN ('Draft','Open','Closed','Completed','Cancelled')");
            });
        });
        modelBuilder.Entity<RaceDayRoute>(e =>
        {
            e.HasKey(x => x.RouteId);
            e.HasIndex(x => x.EventId).IsUnique();
            e.Property(x => x.StartLocation).HasMaxLength(200).IsRequired();
            e.Property(x => x.FinishLocation).HasMaxLength(200).IsRequired();
            e.Property(x => x.RouteUrl).HasMaxLength(500);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasOne(x => x.Event).WithMany(x => x.Routes).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
            e.ToTable(t => t.HasCheckConstraint("CK_Routes_Elevation", "[ElevationGainM] IS NULL OR [ElevationGainM] >= 0"));
        });
        modelBuilder.Entity<Category>(e =>
        {
            e.HasKey(x => x.CategoryId);
            e.HasIndex(x => new { x.EventId, x.Name }).IsUnique();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.DistanceKm).HasPrecision(6, 2);
            e.Property(x => x.EntryFee).HasPrecision(10, 2);
            e.HasOne(x => x.Event).WithMany(x => x.Categories).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Categories_Distance", "[DistanceKm] > 0");
                t.HasCheckConstraint("CK_Categories_EntryFee", "[EntryFee] >= 0");
                t.HasCheckConstraint("CK_Categories_MaxEntries", "[MaxEntries] > 0");
                t.HasCheckConstraint("CK_Categories_Ages", "([MinAge] IS NULL OR [MinAge] >= 0) AND ([MaxAge] IS NULL OR ([MaxAge] >= 0 AND ([MinAge] IS NULL OR [MaxAge] >= [MinAge])))");
            });
        });
        modelBuilder.Entity<Enrolment>(e =>
        {
            e.HasKey(x => x.EnrolmentId);
            e.HasIndex(x => new { x.ParticipantId, x.EventId }).IsUnique();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Participant).WithMany(x => x.Enrolments).HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Event).WithMany(x => x.Enrolments).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Category).WithMany(x => x.Enrolments).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_Enrolments_Status", "[Status] IN ('Confirmed','Cancelled')"));
        });
        modelBuilder.Entity<Result>(e =>
        {
            e.HasKey(x => x.ResultId);
            e.HasIndex(x => x.EnrolmentId).IsUnique();
            e.Property(x => x.AveragePace).HasPrecision(6, 2);
            e.Property(x => x.FinishStatus).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Enrolment).WithOne(x => x.Result).HasForeignKey<Result>(x => x.EnrolmentId).OnDelete(DeleteBehavior.Cascade);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Results_OverallPosition", "[OverallPosition] IS NULL OR [OverallPosition] > 0");
                t.HasCheckConstraint("CK_Results_CategoryPosition", "[CategoryPosition] IS NULL OR [CategoryPosition] > 0");
                t.HasCheckConstraint("CK_Results_Pace", "[AveragePace] IS NULL OR [AveragePace] > 0");
                t.HasCheckConstraint("CK_Results_FinishStatus", "[FinishStatus] IN ('Finished','DNF','DNS')");
            });
        });
    }
}