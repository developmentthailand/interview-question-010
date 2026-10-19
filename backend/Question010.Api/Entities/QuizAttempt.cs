namespace Example.Com.InterviewQuestion010.Entities;

public sealed class QuizAttempt
{
    public int Id { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public DateTimeOffset SubmittedAtUtc { get; set; }
    public List<QuizAttemptAnswer> Answers { get; set; } = [];
}
