namespace Example.Com.InterviewQuestion010.Contracts;

public sealed record QuizQuestionResponse(int Id, int Position, string Prompt, IReadOnlyList<QuizOptionResponse> Options);
