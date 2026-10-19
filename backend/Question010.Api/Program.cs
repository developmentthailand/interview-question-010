using Example.Com.InterviewQuestion010.Data;
using Example.Com.InterviewQuestion010.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Quiz") ?? "Data Source=quiz.db"));
builder.Services.AddOpenApi();

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var database = scope.ServiceProvider.GetRequiredService<QuizDbContext>();
    await database.Database.EnsureCreatedAsync();
    if (!await database.Questions.AnyAsync())
    {
        database.Questions.AddRange(
            new QuizQuestion { Id = 1, Position = 1, Prompt = "ข้อใดต่างจากข้ออื่น", OptionA = "3", OptionB = "5", OptionC = "9", OptionD = "11", CorrectOption = "C" },
            new QuizQuestion { Id = 2, Position = 2, Prompt = "X + 2 = 4 จงหาค่า X", OptionA = "1", OptionB = "2", OptionC = "3", OptionD = "4", CorrectOption = "B" });
        await database.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
