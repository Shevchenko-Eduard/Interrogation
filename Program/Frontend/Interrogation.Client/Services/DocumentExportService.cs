using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Interrogation.Client.Models;

namespace Interrogation.Client.Services;

public sealed class DocumentExportService
{
    public byte[] ExportDocx(string text, DocumentItem document)
    {
        if (document.SourceFormat == "docx" && document.OriginalFileBytes is not null)
        {
            text = SanitizeXmlText(text);
            if (text == document.OriginalText) return document.OriginalFileBytes;
            var updated = TryUpdateDocx(document.OriginalFileBytes, text);
            if (updated is not null) return updated;
        }
        using var stream = new MemoryStream();
        using (var word = WordprocessingDocument.Create(stream, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
        {
            var part = word.AddMainDocumentPart();
            var body = new Body();
            foreach (var line in Lines(text)) body.Append(new Paragraph(new Run(new Text(line))));
            part.Document = new Document(body);
            part.Document.Save();
        }
        return stream.ToArray();
    }

    public byte[] ExportOdt(string text, DocumentItem document)
    {
        if (document.SourceFormat == "odt" && document.OriginalFileBytes is not null)
        {
            text = SanitizeXmlText(text);
            if (text == document.OriginalText) return document.OriginalFileBytes;
            var updated = TryUpdateOdt(document.OriginalFileBytes, text);
            if (updated is not null) return updated;
        }
        return CreateOdt(text);
    }

    private static byte[]? TryUpdateDocx(byte[] source, string text)
    {
        using var stream = new MemoryStream();
        stream.Write(source);
        stream.Position = 0;
        using (var word = WordprocessingDocument.Open(stream, true))
        {
            var mainDocument = word.MainDocumentPart?.Document;
            var paragraphs = mainDocument?.Body?.Descendants<Paragraph>().ToArray();
            var lines = Lines(SanitizeXmlText(text));
            if (mainDocument is null || paragraphs is null || paragraphs.Length != lines.Length) return null;
            for (var index = 0; index < paragraphs.Length; index++)
            {
                var nodes = paragraphs[index].Descendants<Text>().ToArray();
                if (nodes.Length == 0) paragraphs[index].Append(new Run(new Text(lines[index])));
                else
                {
                    nodes[0].Text = lines[index];
                    foreach (var node in nodes.Skip(1)) node.Text = string.Empty;
                }
            }
            mainDocument.Save();
        }
        return stream.ToArray();
    }

    private static byte[]? TryUpdateOdt(byte[] source, string text)
    {
        using var input = new MemoryStream(source);
        using var output = new MemoryStream();
        using var sourceArchive = new ZipArchive(input, ZipArchiveMode.Read);
        using (var targetArchive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            var lines = Lines(SanitizeXmlText(text));
            XNamespace textNs = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
            foreach (var entry in sourceArchive.Entries)
            {
                var target = targetArchive.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                using var sourceStream = entry.Open();
                using var targetStream = target.Open();
                if (entry.FullName != "content.xml") { sourceStream.CopyTo(targetStream); continue; }
                var xml = XDocument.Load(sourceStream);
                var paragraphs = xml.Descendants().Where(x => x.Name == textNs + "p" || x.Name == textNs + "h").ToArray();
                if (paragraphs.Length != lines.Length) return null;
                for (var index = 0; index < paragraphs.Length; index++) paragraphs[index].Value = lines[index];
                xml.Save(targetStream);
            }
        }
        return output.ToArray();
    }

    private static byte[] CreateOdt(string text)
    {
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            var mime = archive.CreateEntry("mimetype", CompressionLevel.NoCompression);
            using (var writer = new StreamWriter(mime.Open())) writer.Write("application/vnd.oasis.opendocument.text");
            XNamespace office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
            XNamespace textNs = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";
            var xml = new XDocument(new XElement(office + "document-content",
                new XAttribute(XNamespace.Xmlns + "office", office), new XAttribute(XNamespace.Xmlns + "text", textNs),
                new XElement(office + "body", new XElement(office + "text", Lines(text).Select(line => new XElement(textNs + "p", line))))));
            using var stream = archive.CreateEntry("content.xml").Open();
            xml.Save(stream);
        }
        return output.ToArray();
    }

    private static string[] Lines(string text) => SanitizeXmlText(text).ReplaceLineEndings("\n").Split('\n');

    private static string SanitizeXmlText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return string.Concat(text.Where(XmlConvert.IsXmlChar));
    }
}
