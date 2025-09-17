using TelegramTestBot.Domain.Models;

namespace TelegramTestBot.Domain.Models
{
    public class TestDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long OwnerUserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<Question> Questions { get; set; } = new();
    }
}