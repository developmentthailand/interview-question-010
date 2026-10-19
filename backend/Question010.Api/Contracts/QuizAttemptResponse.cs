namespace Example.Com.InterviewQuestion010.Contracts;

public sealed record QuizAttemptResponse(
    int Id,
    string ParticipantName,
    int Score,
    int TotalQuestions,
    DateTimeOffset SubmittedAtUtc,
    IReadOnlyList<QuizAnswerResponse> Answers);
