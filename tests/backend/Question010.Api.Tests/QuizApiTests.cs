using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Example.Com.InterviewQuestion010.Tests;

public sealed class QuizApiTests
{
    [Fact]
    public async Task Questions_include_options_but_never_expose_the_answer_key()
    {
        using var factory = new QuizApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/quiz/questions");
        var json = await response.Content.ReadAsStringAsync();
        using var body = JsonDocument.Parse(json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, body.RootElement.GetArrayLength());
        Assert.Equal("ข้อใดต่างจากข้ออื่น", body.RootElement[0].GetProperty("prompt").GetString());
        Assert.Equal(4, body.RootElement[0].GetProperty("options").GetArrayLength());
        Assert.False(body.RootElement[0].TryGetProperty("correctOption", out _));
    }

    [Fact]
    public async Task Submit_computes_score_on_the_server_and_persists_selected_answers()
    {
        using var factory = new QuizApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/quiz/attempts", new
        {
            participantName = " Narin ",
            answers = new[] { new { questionId = 1, selectedOption = "C" }, new { questionId = 2, selectedOption = "D" } },
            score = 2
        });
        var attempt = await response.Content.ReadFromJsonAsync<AttemptDto>();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(attempt);
        Assert.True(attempt.Id > 0);
        Assert.Equal("Narin", attempt.ParticipantName);
        Assert.Equal(1, attempt.Score);
        Assert.Equal(2, attempt.TotalQuestions);
        Assert.Equal("9", attempt.Answers[0].SelectedText);
        Assert.True(attempt.Answers[0].IsCorrect);
        Assert.False(attempt.Answers[1].IsCorrect);

        var persisted = await client.GetFromJsonAsync<AttemptDto>($"/api/quiz/attempts/{attempt.Id}");
        Assert.NotNull(persisted);
        Assert.Equal(1, persisted.Score);
        Assert.Equal("D", persisted.Answers[1].SelectedOption);
    }

    [Fact]
    public async Task Submit_rejects_missing_answers()
    {
        using var factory = new QuizApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/quiz/attempts", new
        {
            participantName = "Narin",
            answers = new[] { new { questionId = 1, selectedOption = "C" } }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record AttemptDto(int Id, string ParticipantName, int Score, int TotalQuestions, AnswerDto[] Answers);
    private sealed record AnswerDto(int QuestionId, int Position, string Prompt, string SelectedOption, string SelectedText, bool IsCorrect);
}
