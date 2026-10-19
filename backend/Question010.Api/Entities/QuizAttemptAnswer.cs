namespace Example.Com.InterviewQuestion010.Entities;

public sealed class QuizAttemptAnswer
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public QuizAttempt Attempt { get; set; } = null!;
    public int QuestionId { get; set; }
    public int Position { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string SelectedOption { get; set; } = string.Empty;
    public string SelectedText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
