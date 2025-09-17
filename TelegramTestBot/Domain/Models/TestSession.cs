using TelegramTestBot.Domain.Models;

public class TestSession
{
    public TestDocument Test { get; set; }
    public List<Question> RemainingQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public Question LastQuestion { get; set; }
}