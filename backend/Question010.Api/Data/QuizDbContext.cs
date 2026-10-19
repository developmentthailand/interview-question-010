using Example.Com.InterviewQuestion010.Entities;
using Microsoft.EntityFrameworkCore;

namespace Example.Com.InterviewQuestion010.Data;

public sealed class QuizDbContext(DbContextOptions<QuizDbContext> options) : DbContext(options)
{
    public DbSet<QuizQuestion> Questions => Set<QuizQuestion>();
    public DbSet<QuizAttempt> Attempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswer> AttemptAnswers => Set<QuizAttemptAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuizQuestion>().ToTable("QuizQuestions").HasKey(question => question.Id);
        modelBuilder.Entity<QuizQuestion>().HasIndex(question => question.Position).IsUnique();
        modelBuilder.Entity<QuizAttempt>().ToTable("QuizAttempts").HasKey(attempt => attempt.Id);
        modelBuilder.Entity<QuizAttempt>().Property(attempt => attempt.ParticipantName).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<QuizAttempt>().HasIndex(attempt => attempt.SubmittedAtUtc);
        modelBuilder.Entity<QuizAttemptAnswer>().ToTable("QuizAttemptAnswers").HasKey(answer => answer.Id);
        modelBuilder.Entity<QuizAttemptAnswer>().Property(answer => answer.Prompt).HasMaxLength(500).IsRequired();
        modelBuilder.Entity<QuizAttemptAnswer>().Property(answer => answer.SelectedText).HasMaxLength(300).IsRequired();
        modelBuilder.Entity<QuizAttemptAnswer>().HasOne(answer => answer.Attempt).WithMany(attempt => attempt.Answers)
            .HasForeignKey(answer => answer.AttemptId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<QuizAttemptAnswer>().HasIndex(answer => new { answer.AttemptId, answer.QuestionId }).IsUnique();
    }
}
