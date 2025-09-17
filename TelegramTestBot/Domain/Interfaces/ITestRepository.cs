using TelegramTestBot.Domain.Models;

namespace TelegramTestBot.Domain.Interfaces
{
    public interface ITestRepository
    {
        Task SaveAsync(TestDocument test);
        Task<TestDocument?> GetByIdAsync(string id);
        Task<List<TestDocument>> GetAllAsync();
    }
}