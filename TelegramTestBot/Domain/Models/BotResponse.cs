namespace TelegramTestBot.Domain.Models
{
    public class BotResponse
    {
        public string Text { get; set; } = string.Empty;
        public List<string> Buttons { get; set; } = new();
    }
}