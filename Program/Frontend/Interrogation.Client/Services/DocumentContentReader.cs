using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO.Compression;
using System.Xml.Linq;

namespace Interrogation.Client.Services;

public sealed class DocumentContentReader
{
    public async Task<string> ReadAsync(string fileName, Stream stream)
    {
        if (fileName.EndsWith(".odt", StringComparison.OrdinalIgnoreCase))
        {
            return ReadOpenDocument(stream);
        }

        if (!fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }

        using var document = WordprocessingDocument.Open(stream, false);
        var mainPart = document.MainDocumentPart
            ?? throw new InvalidDataException("В документе отсутствует основная часть");
        var body = mainPart.Document?.Body
            ?? throw new InvalidDataException("В документе отсутствует содержимое");

        var paragraphs = body
            .Descendants<Paragraph>()
            .Select(paragraph => paragraph.InnerText)
            .ToArray();

        return string.Join(Environment.NewLine, paragraphs);
    }

    private static string ReadOpenDocument(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var contentEntry = archive.GetEntry("content.xml")
            ?? throw new InvalidDataException("В ODT отсутствует content.xml");
        using var contentStream = contentEntry.Open();
        var content = XDocument.Load(contentStream);
        XNamespace textNamespace = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";

        var paragraphs = content
            .Descendants()
            .Where(element => element.Name == textNamespace + "p" || element.Name == textNamespace + "h")
            .Select(element => string.Concat(element.DescendantNodes().OfType<XText>().Select(node => node.Value)))
            .ToArray();

        return string.Join(Environment.NewLine, paragraphs);
    }
}
