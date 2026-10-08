using Microsoft.EntityFrameworkCore;
using AutomatedInterviewSchedulingSystem.Models;
using System;

namespace AutomatedInterviewSchedulingSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Interviewer> Interviewers { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<ScheduledInterview> ScheduledInterviews { get; set; }
        public DbSet<Conflict> Conflicts { get; set; }
        public DbSet<Log> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<string>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Candidate Configuration
            modelBuilder.Entity<Candidate>(entity =>
            {
                entity.HasKey(e => e.CandidateId);
                entity.Property(e => e.Status).HasConversion<string>();
                
                entity.HasOne(e => e.User)
                    .WithOne(u => u.Candidate)
                    .HasForeignKey<Candidate>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Interviewer Configuration
            modelBuilder.Entity<Interviewer>(entity =>
            {
                entity.HasKey(e => e.InterviewerId);
                
                entity.HasOne(e => e.User)
                    .WithOne(u => u.Interviewer)
                    .HasForeignKey<Interviewer>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Availability Configuration
            modelBuilder.Entity<Availability>(entity =>
            {
                entity.HasKey(e => e.AvailabilityId);
                
                entity.HasOne(e => e.Interviewer)
                    .WithMany(i => i.Availabilities)
                    .HasForeignKey(e => e.InterviewerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Create index for faster queries
                entity.HasIndex(e => new { e.InterviewerId, e.AvailableDate });
            });

            // ScheduledInterview Configuration
            modelBuilder.Entity<ScheduledInterview>(entity =>
            {
                entity.HasKey(e => e.InterviewId);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Candidate)
                    .WithMany(c => c.ScheduledInterviews)
                    .HasForeignKey(e => e.CandidateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Interviewer)
                    .WithMany(i => i.ScheduledInterviews)
                    .HasForeignKey(e => e.InterviewerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Create composite index for conflict detection
                entity.HasIndex(e => new { e.InterviewerId, e.InterviewDate, e.StartTime, e.EndTime });
                entity.HasIndex(e => new { e.CandidateId, e.InterviewDate, e.StartTime, e.EndTime });
            });

            // Conflict Configuration
            modelBuilder.Entity<Conflict>(entity =>
            {
                entity.HasKey(e => e.ConflictId);
                entity.Property(e => e.DetectedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.ScheduledInterview)
                    .WithMany(s => s.Conflicts)
                    .HasForeignKey(e => e.InterviewId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Log Configuration
            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasKey(e => e.LogId);
                entity.Property(e => e.Level).HasConversion<string>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Create index for log queries
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Level);
            });

            // Seed initial admin user (optional)
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed an admin user for initial access
            // Password: Admin@123  — valid BCrypt $2a$11$ hash
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Name = "System Admin",
                    Email = "admin@interviewscheduler.com",
                    PasswordHash = "$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi",
                    Role = UserRole.Admin,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
