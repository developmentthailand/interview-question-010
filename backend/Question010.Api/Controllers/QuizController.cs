using Example.Com.InterviewQuestion010.Contracts;
using Example.Com.InterviewQuestion010.Data;
using Example.Com.InterviewQuestion010.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Example.Com.InterviewQuestion010.Controllers;

[ApiController]
[Route("api/quiz")]
public sealed class QuizController(QuizDbContext database) : ControllerBase
{
    private static readonly string[] OptionKeys = ["A", "B", "C", "D"];

    [HttpGet("questions")]
    public async Task<ActionResult<IReadOnlyList<QuizQuestionResponse>>> GetQuestions(CancellationToken cancellationToken)
    {
        var questions = await database.Questions.AsNoTracking().OrderBy(question => question.Position).ToListAsync(cancellationToken);
        return Ok(questions.Select(ToQuestionResponse).ToArray());
    }

    [HttpPost("attempts")]
    [ProducesResponseType<QuizAttemptResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Submit(SubmitAttemptRequest request, CancellationToken cancellationToken)
    {
        var name = request.ParticipantName?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            return BadRequest(new ProblemDetails { Title = "Enter a name up to 100 characters." });

        var questions = await database.Questions.AsNoTracking().OrderBy(question => question.Position).ToListAsync(cancellationToken);
        if (questions.Count == 0 || request.Answers is null || request.Answers.Count != questions.Count)
            return BadRequest(new ProblemDetails { Title = "Choose one answer for every question." });

        var answerByQuestion = new Dictionary<int, SubmitAnswerRequest>(request.Answers.Count);
        foreach (var answer in request.Answers)
        {
            if (!answerByQuestion.TryAdd(answer.QuestionId, answer) || answer.SelectedOption is null || !OptionKeys.Contains(answer.SelectedOption, StringComparer.Ordinal))
                return BadRequest(new ProblemDetails { Title = "Each question needs one valid answer." });
        }
        if (questions.Any(question => !answerByQuestion.ContainsKey(question.Id)))
            return BadRequest(new ProblemDetails { Title = "Choose one answer for every question." });

        var attempt = new QuizAttempt
        {
            ParticipantName = name,
            TotalQuestions = questions.Count,
            SubmittedAtUtc = DateTimeOffset.UtcNow
        };
        foreach (var question in questions)
        {
            var selected = answerByQuestion[question.Id].SelectedOption!;
            var isCorrect = string.Equals(selected, question.CorrectOption, StringComparison.Ordinal);
            attempt.Answers.Add(new QuizAttemptAnswer
            {
                QuestionId = question.Id,
                Position = question.Position,
                Prompt = question.Prompt,
                SelectedOption = selected,
                SelectedText = OptionText(question, selected),
                IsCorrect = isCorrect
            });
            if (isCorrect) attempt.Score++;
        }

        database.Attempts.Add(attempt);
        await database.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetAttempt), new { id = attempt.Id }, ToAttemptResponse(attempt));
    }

    [HttpGet("attempts/{id:int}")]
    public async Task<ActionResult<QuizAttemptResponse>> GetAttempt(int id, CancellationToken cancellationToken)
    {
        var attempt = await database.Attempts.AsNoTracking().Include(item => item.Answers)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return attempt is null ? NotFound() : Ok(ToAttemptResponse(attempt));
    }

    private static QuizQuestionResponse ToQuestionResponse(QuizQuestion question) => new(
        question.Id,
        question.Position,
        question.Prompt,
        [new("A", question.OptionA), new("B", question.OptionB), new("C", question.OptionC), new("D", question.OptionD)]);

    private static string OptionText(QuizQuestion question, string key) => key switch
    {
        "A" => question.OptionA,
        "B" => question.OptionB,
        "C" => question.OptionC,
        "D" => question.OptionD,
        _ => throw new ArgumentOutOfRangeException(nameof(key))
    };

    private static QuizAttemptResponse ToAttemptResponse(QuizAttempt attempt) => new(
        attempt.Id,
        attempt.ParticipantName,
        attempt.Score,
        attempt.TotalQuestions,
        attempt.SubmittedAtUtc,
        attempt.Answers.OrderBy(answer => answer.Position)
            .Select(answer => new QuizAnswerResponse(answer.QuestionId, answer.Position, answer.Prompt, answer.SelectedOption, answer.SelectedText, answer.IsCorrect))
            .ToArray());
}
