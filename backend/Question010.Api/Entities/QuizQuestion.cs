namespace Example.Com.InterviewQuestion010.Entities;

public sealed class QuizQuestion
{
    public int Id { get; set; }
    public int Position { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectOption { get; set; } = string.Empty;
}
