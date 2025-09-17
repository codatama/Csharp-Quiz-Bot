using TelegramTestBot.Domain.Models;

namespace TelegramTestBot.Domain.Services
{
    public class StartCommandHandler
    {
        public BotResponse Handle()
        {
            return new BotResponse
            {
                Text = "👋 Привет! Я бот для проверки знаний.\n\nВыбери действие ниже:",
                Buttons = new List<string>
                {
                    "📤 Загрузить тест",
                    "🧪 Выбрать тест"
                }
            };
        }
    }
}