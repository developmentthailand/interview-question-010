namespace Example.Com.InterviewQuestion010.Contracts;

public sealed record SubmitAttemptRequest(string? ParticipantName, IReadOnlyList<SubmitAnswerRequest>? Answers);
