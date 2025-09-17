using UglyToad.PdfPig;
using System.Text;

namespace TelegramTestBot.Infrastructure.Pdf
{
    public interface IPdfTextExtractor
    {
        string ExtractText(Stream pdfStream);
    }

    public class PdfTextExtractor : IPdfTextExtractor
    {
        public string ExtractText(Stream pdfStream)
        {

            if (!pdfStream.CanSeek) throw new InvalidOperationException("Stream must be seekable.");
            pdfStream.Position = 0;

            var sb = new StringBuilder();

            using (var doc = PdfDocument.Open(pdfStream))
            {
                foreach (var page in doc.GetPages())
                {

                    sb.AppendLine(page.Text);
                    sb.AppendLine();
                }
            }

            return NormalizeText(sb.ToString());
        }

        private static string NormalizeText(string text)
        {

            var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
            var lines = normalized.Split('\n')
                                  .Select(line => line.TrimEnd())
                                  .ToArray();
            return string.Join('\n', lines);
        }
    }
}