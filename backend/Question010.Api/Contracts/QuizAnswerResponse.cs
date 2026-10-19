namespace Example.Com.InterviewQuestion010.Contracts;

public sealed record QuizAnswerResponse(int QuestionId, int Position, string Prompt, string SelectedOption, string SelectedText, bool IsCorrect);
