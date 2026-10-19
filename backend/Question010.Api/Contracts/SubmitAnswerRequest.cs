namespace Example.Com.InterviewQuestion010.Contracts;

public sealed record SubmitAnswerRequest(int QuestionId, string? SelectedOption);
