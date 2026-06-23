using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO.Compression;
using System.Xml.Linq;

namespace Interrogation.Client.Services;

public sealed class DocumentContentReader
{
    public async Task<string> ReadAsync(string fileName, Stream stream)
    {
        var format = DetectFormat(fileName, stream);
        if (format == "odt")
        {
            return ReadOpenDocument(stream);
        }

        if (format != "docx")
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

    public static string DetectFormat(string fileName, Stream stream)
    {
        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        if (extension is "docx" or "odt")
        {
            return extension;
        }

        if (!stream.CanSeek)
        {
            return extension;
        }

        var position = stream.Position;
        try
        {
            stream.Position = 0;
            if (!LooksLikeZip(stream))
            {
                return extension;
            }

            stream.Position = 0;
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
            if (archive.GetEntry("word/document.xml") is not null)
            {
                return "docx";
            }
            if (archive.GetEntry("content.xml") is not null &&
                archive.GetEntry("mimetype") is not null)
            {
                return "odt";
            }
        }
        catch (InvalidDataException)
        {
            return extension;
        }
        finally
        {
            stream.Position = position;
        }

        return extension;
    }

    public static string EnsureFileNameExtension(string fileName, string? extension, string? contentType, byte[] content)
    {
        if (!string.IsNullOrWhiteSpace(Path.GetExtension(fileName)))
        {
            return fileName;
        }

        var normalizedExtension = NormalizeExtension(extension, contentType, content);
        return string.IsNullOrWhiteSpace(normalizedExtension)
            ? fileName
            : fileName + "." + normalizedExtension;
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

    private static string NormalizeExtension(string? extension, string? contentType, byte[] content)
    {
        var value = extension?.Trim().TrimStart('.').ToLowerInvariant();
        if (value is "docx" or "odt" or "txt" or "md" or "enc")
        {
            return value;
        }
        if (contentType?.Contains("wordprocessingml.document", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "docx";
        }
        if (contentType?.Contains("opendocument.text", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "odt";
        }

        using var stream = new MemoryStream(content);
        return DetectFormat(string.Empty, stream);
    }

    private static bool LooksLikeZip(Stream stream)
    {
        Span<byte> header = stackalloc byte[4];
        return stream.Read(header) == 4 &&
               header[0] == 0x50 &&
               header[1] == 0x4B;
    }
}
