using System.Text.RegularExpressions;
using TelegramTestBot.Domain.Models;

namespace TelegramTestBot.Infrastructure.Parsers
{
    public interface ITestParser
    {
        TestDocument Parse(string rawText);
    }

    public class PdfTestParser : ITestParser
    {
        private static readonly Regex CorrectLetterRegex =
            new(@"����������\s+([A-E�-�])", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public TestDocument Parse(string rawText)
        {
            var lines = rawText.Split('\n')
                               .Select(line => line.Trim())
                               .Where(line => !string.IsNullOrWhiteSpace(line))
                               .ToList();

            var title = ExtractTitle(lines);
            var correctLetter = ExtractCorrectLetter(lines);
            var questions = ExtractQuestions(rawText, correctLetter);

            return new TestDocument
            {
                Title = title,
                Questions = questions
            };
        }

        private string ExtractTitle(List<string> lines)
        {
            return lines.FirstOrDefault() ?? "Untitled Test";
        }

        private string ExtractCorrectLetter(List<string> lines)
        {
            var joined = string.Join("\n", lines);
            var match = CorrectLetterRegex.Match(joined);
            if (!match.Success) return "A";

            var letter = match.Groups[1].Value.ToUpperInvariant();
            return NormalizeLetter(letter);
        }

        private List<Question> ExtractQuestions(string text, string correctLetter)
        {
            var questions = new List<Question>();

            var blocks = Regex.Matches(text, @"\d+\.\s.*?(?=\d+\.\s|$)", RegexOptions.Singleline);

            foreach (Match block in blocks)
            {
                var raw = block.Value.Trim();

                var questionMatch = Regex.Match(raw, @"^\d+\.\s*(.*?)(?=\s+[�-�A-E]\s)");
                var questionText = questionMatch.Success ? questionMatch.Groups[1].Value.Trim() : "��� ������";

                var optionMatches = Regex.Matches(raw, @"(?<=\s|^)([�-�A-E])\s+(.+?)(?=\s+[�-�A-E]\s|$)", RegexOptions.Singleline);

                var options = new List<string>();
                int correctIndex = 0;

                foreach (Match opt in optionMatches)
                {
                    var letter = NormalizeLetter(opt.Groups[1].Value);
                    var optionText = opt.Groups[2].Value.Trim();

                    if (optionText.Length > 60)
                        continue;

                    options.Add(optionText);
                    if (letter == correctLetter)
                        correctIndex = options.Count - 1;
                }

                if (options.Count >= 2)
                {
                    questions.Add(new Question
                    {
                        Text = questionText,
                        Options = options,
                        CorrectOptionIndex = correctIndex
                    });
                }
            }

            return questions;
        }


        private string NormalizeLetter(string raw)
        {
            return raw.ToUpperInvariant() switch
            {
                "�" => "A",
                "�" => "B",
                "�" => "C",
                "D" => "D",
                "�" => "E",
                _ => raw.ToUpperInvariant()
            };
        }
    }
}
