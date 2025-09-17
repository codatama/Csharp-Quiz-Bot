using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramTestBot.Domain.Services;
using TelegramTestBot.Infrastructure.Pdf;
using TelegramTestBot.Infrastructure.Parsers;
using TelegramTestBot.Infrastructure.Mongo;
using TelegramTestBot.Domain.Models;

public class UpdateHandler
{
    private readonly TelegramBotClient _botClient;
    private readonly MongoTestRepository _testRepository;
    private readonly IPdfTextExtractor _pdfTextExtractor;
    private readonly ITestParser _testParser;

    private readonly Dictionary<long, TestSession> _userSessions = new();


    public UpdateHandler(TelegramBotClient botClient, MongoTestRepository testRepository,
                     IPdfTextExtractor pdfTextExtractor, ITestParser testParser)
    {
        _botClient = botClient;
        _testRepository = testRepository;
        _pdfTextExtractor = pdfTextExtractor;
        _testParser = testParser;
    }

    public async Task HandleAsync(Update update)
    {
        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            var message = update.Message;
            Console.WriteLine($"📩 Получено сообщение: {message.Text}");

            if (message.Text == "/start")
            {
                var handler = new StartCommandHandler();
                var response = handler.Handle();

                var keyboard = new InlineKeyboardMarkup(new[]
                {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(response.Buttons[0], "upload_test"),
                    InlineKeyboardButton.WithCallbackData(response.Buttons[1], "select_test")
                }
            });

                await _botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: response.Text,
                    replyMarkup: keyboard
                );
            }
        }

        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data == "upload_test")
        {
            var userId = update.CallbackQuery.From.Id;
            Console.WriteLine($"🔘 Пользователь {userId} нажал 'Загрузить тест'");

            await _botClient.SendTextMessageAsync(
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: "📄 Отправьте ваш тест в формате .pdf"
            );
        }

        if (update.Type == UpdateType.Message && update.Message?.Document != null && update.Message.From != null)
        {
            var document = update.Message.Document;
            var userId = update.Message!.From!.Id;

            if (document.MimeType == "application/pdf")
            {
                var file = await _botClient.GetFileAsync(document.FileId);
                var filePath = file.FilePath ?? throw new Exception("filePath is null");

                using var stream = new MemoryStream();
                await _botClient.DownloadFileAsync(filePath, stream);
                stream.Position = 0;

                var rawText = _pdfTextExtractor.ExtractText(stream);
                var test = _testParser.Parse(rawText);
                test.OwnerUserId = userId;

                await _testRepository.SaveAsync(test);

                await _botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "✅ Тест успешно сохранен!"
                );
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "❌ Пожалуйста, отправьте файл в формате .pdf"
                );
            }
        }

        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data == "select_test")
        {
            var userId = update.CallbackQuery.From.Id;
            Console.WriteLine($"📘 Пользователь {userId} нажал 'Выбрать тест'");

            var tests = await _testRepository.GetByUserIdAsync(userId);

            if (tests.Count == 0)
            {
                await _botClient.SendTextMessageAsync(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    text: "📭 У вас пока нет сохранённых тестов."
                );
                return;
            }

            var listText = string.Join("\n", tests.Select((t, i) => $"{i + 1}. {t.Title}"));

            var keyboard = new InlineKeyboardMarkup(new[]
            {
        new[] { InlineKeyboardButton.WithCallbackData("Выбрать тест", "choose_test") }
    });

            await _botClient.SendTextMessageAsync(
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: $"📋 Список ваших тестов:\n{listText}",
                replyMarkup: keyboard
            );
        }

        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data == "choose_test")
        {
            await _botClient.SendTextMessageAsync(
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: "✏️ Напишите номер теста для прохождения."
            );
        }

        if (update.Type == UpdateType.Message && int.TryParse(update.Message?.Text, out int testNumber))
        {
            var userId = update.Message.From.Id;
            var tests = await _testRepository.GetByUserIdAsync(userId);

            if (testNumber < 1 || testNumber > tests.Count)
            {
                await _botClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: "❌ Неверный номер теста."
                );
                return;
            }

            var selectedTest = tests[testNumber - 1];

            _userSessions[userId] = new TestSession
            {
                Test = selectedTest,
                RemainingQuestions = selectedTest.Questions.OrderBy(q => Guid.NewGuid()).ToList(),
                CorrectAnswers = 0
            };

            await SendNextQuestion(userId, update.Message.Chat.Id, null);
        }

        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data?.StartsWith("answer_") == true)
        {
            var userId = update.CallbackQuery.From.Id;
            if (!_userSessions.ContainsKey(userId)) return;

            var session = _userSessions[userId];
            var selectedIndex = int.Parse(update.CallbackQuery.Data.Split('_')[1]);

            if (selectedIndex == 0)
                session.CorrectAnswers++;

            await SendNextQuestion(userId, update.CallbackQuery.Message.Chat.Id, session.LastQuestion.Options[selectedIndex]);
        }

    }

    private async Task SendNextQuestion(long userId, long chatId, string? previousAnswer)
    {
        var session = _userSessions[userId];

        if (session.LastQuestion != null && previousAnswer != null)
        {
            var correct = session.LastQuestion.Options[0];
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"✅ Правильный ответ: {correct}"
            );
        }

        if (session.RemainingQuestions.Count == 0)
        {
            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"🏁 Тест завершён. Правильных ответов: {session.CorrectAnswers} из {session.Test.Questions.Count}."
            );
            _userSessions.Remove(userId);
            return;
        }

        var question = session.RemainingQuestions[0];
        session.RemainingQuestions.RemoveAt(0);
        session.LastQuestion = question;

        var keyboard = new InlineKeyboardMarkup(
            question.Options.Select((opt, i) =>
                InlineKeyboardButton.WithCallbackData($"{(char)('A' + i)}", $"answer_{i}")
            )
        );

        await _botClient.SendTextMessageAsync(
            chatId: chatId,
            text: $"❓ {question.Text}\n\n" + string.Join("\n", question.Options.Select((opt, i) => $"{(char)('A' + i)}. {opt}")),
            replyMarkup: keyboard
        );
    }

}