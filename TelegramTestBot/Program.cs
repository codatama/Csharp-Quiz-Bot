using Telegram.Bot;
using Microsoft.Extensions.Configuration;
using TelegramTestBot.Infrastructure;
using System.Data.Common;
using TelegramTestBot.Infrastructure.Mongo;
using TelegramTestBot.Infrastructure.Parsers;
using TelegramTestBot.Infrastructure.Pdf;

class Program
{
    static async Task Main()
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        var configuration = configBuilder.Build();
        var botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();

        var mongoSettings = configuration.GetSection("MongoSettings");
        var connectionString = mongoSettings["ConnectionString"];
        var dbName = mongoSettings["DatabaseName"];

        if (string.IsNullOrWhiteSpace(botConfig?.Token))
        {
            Console.WriteLine("❌ Токен не найден.");
            return;
        }

        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(dbName))
        {
            Console.WriteLine("❌ Ошибка конфигурации MongoDB.");
            return;
        }

        var botClient = new TelegramBotClient(botConfig.Token);
        var testRepository = new MongoTestRepository(connectionString, dbName);
        var pdfTextExtractor = new PdfTextExtractor();
        var testParser = new PdfTestParser();

        var updateHandler = new UpdateHandler(botClient, testRepository, pdfTextExtractor, testParser);

        using var cts = new CancellationTokenSource();

        botClient.StartReceiving(
            async (client, update, token) => await updateHandler.HandleAsync(update),
            (client, exception, token) =>
            {
                Console.WriteLine($"❌ Ошибка: {exception.Message}");
                return Task.CompletedTask;
            },
            cancellationToken: cts.Token
        );

        Console.WriteLine("✅ Бот запущен. Ожидаем команды...");
        await Task.Delay(-1, cts.Token);
    }
}
