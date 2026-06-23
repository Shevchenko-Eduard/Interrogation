using Interrogation.Client.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO.Compression;
using System.Text;
using Interrogation.Client.Models;

var cryptography = new OpenSslCryptographyService();
Assert(cryptography.IsAvailable, $"Cryptography unavailable: {cryptography.EngineDescription}");

const string password = "Practice-Strong-2026";
const string plainText = "Протокол: свидетель Иванов, адрес Краснодар.\nВторая строка документа.";

var encrypted = await cryptography.EncryptAsync(plainText, password);
Assert(!encrypted.Contains("Иванов", StringComparison.Ordinal), "Ciphertext contains plaintext");

var decrypted = await cryptography.DecryptAsync(encrypted, password);
Assert(decrypted == plainText, "AES-GCM round-trip changed the document text");

try
{
    await cryptography.DecryptAsync(encrypted, "Definitely-Wrong-Password");
    throw new InvalidOperationException("Wrong password was accepted");
}
catch (InvalidOperationException)
{
}

Console.WriteLine($"PASS: {cryptography.EngineDescription}");
Console.WriteLine("PASS: UTF-8 encrypt/decrypt round-trip");
Console.WriteLine("PASS: wrong password rejected");

await using var docxStream = new MemoryStream();
using (var wordDocument = WordprocessingDocument.Create(docxStream, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
{
    var mainPart = wordDocument.AddMainDocumentPart();
    mainPart.Document = new Document(new Body(
        new Paragraph(new Run(new Text("Протокол обыска"))),
        new Paragraph(new Run(new Text("Свидетель Иванов")))));
    mainPart.Document.Save();
}

docxStream.Position = 0;
var documentReader = new DocumentContentReader();
var importedText = await documentReader.ReadAsync("protocol.docx", docxStream);
Assert(importedText == $"Протокол обыска{Environment.NewLine}Свидетель Иванов", ".docx paragraphs were imported incorrectly");
Console.WriteLine("PASS: .docx paragraph import");

await using var odtStream = new MemoryStream();
using (var archive = new ZipArchive(odtStream, ZipArchiveMode.Create, leaveOpen: true))
{
    var contentEntry = archive.CreateEntry("content.xml");
    await using var contentStream = contentEntry.Open();
    await using var writer = new StreamWriter(contentStream, new UTF8Encoding(false));
    await writer.WriteAsync("""
        <?xml version="1.0" encoding="UTF-8"?>
        <office:document-content xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0">
          <office:body><office:text><text:h>Протокол</text:h><text:p>Место происшествия</text:p></office:text></office:body>
        </office:document-content>
        """);
}

odtStream.Position = 0;
var importedOdt = await documentReader.ReadAsync("protocol.odt", odtStream);
Assert(importedOdt == $"Протокол{Environment.NewLine}Место происшествия", ".odt paragraphs were imported incorrectly");
Console.WriteLine("PASS: .odt paragraph import");

var exporter = new DocumentExportService();
var originalDocxBytes = docxStream.ToArray();
var docxItem = new DocumentItem
{
    Id = 1, Name = "protocol.docx", CaseNumber = "test", Owner = "test", Status = "test",
    Content = importedText, UpdatedAt = DateTimeOffset.Now,
    OriginalFileBytes = originalDocxBytes, OriginalText = importedText, SourceFormat = "docx"
};
Assert(exporter.ExportDocx(importedText, docxItem).SequenceEqual(originalDocxBytes), "Unchanged DOCX formatting package was not preserved");
var changedDocx = exporter.ExportDocx($"Изменённый протокол{Environment.NewLine}Свидетель Петров", docxItem);
using var changedDocxStream = new MemoryStream(changedDocx);
var changedDocxText = await documentReader.ReadAsync("changed.docx", changedDocxStream);
Assert(changedDocxText.Contains("Свидетель Петров", StringComparison.Ordinal), "Changed DOCX export failed");

var odtItem = new DocumentItem
{
    Id = 2, Name = "protocol.odt", CaseNumber = "test", Owner = "test", Status = "test",
    Content = importedOdt, UpdatedAt = DateTimeOffset.Now,
    OriginalFileBytes = odtStream.ToArray(), OriginalText = importedOdt, SourceFormat = "odt"
};
var changedOdt = exporter.ExportOdt($"Новый протокол{Environment.NewLine}Новый адрес", odtItem);
using var changedOdtStream = new MemoryStream(changedOdt);
var changedOdtText = await documentReader.ReadAsync("changed.odt", changedOdtStream);
Assert(changedOdtText.Contains("Новый адрес", StringComparison.Ordinal), "Changed ODT export failed");
Console.WriteLine("PASS: DOCX/ODT export and source preservation");

var integrityService = new ContainerIntegrityService();
var integrityValues = new[] { encrypted, "[ENC-FRAGMENT-1]", "cipher-fragment" };
var integrity = integrityService.Create(integrityValues, password);
Assert(integrityService.Verify(integrityValues, password, integrity), "Container integrity verification failed");
Assert(!integrityService.Verify([encrypted + "changed", "[ENC-FRAGMENT-1]", "cipher-fragment"], password, integrity), "Container tampering was not detected");
Console.WriteLine("PASS: container tampering rejected");

var auditPath = Path.Combine(Path.GetTempPath(), $"interrogation-audit-{Guid.NewGuid():N}.jsonl");
var auditWriter = new JsonLineAuditLogService(auditPath);
auditWriter.Append(new(DateTimeOffset.Now, "Тест", "Шифрование", "protocol.odt", "Успешно"));
var restoredAudit = new JsonLineAuditLogService(auditPath).LoadRecent(10);
Assert(restoredAudit.Count == 1 && restoredAudit[0].Action == "Шифрование", "Audit log was not restored");
File.Delete(auditPath);
Console.WriteLine("PASS: persistent audit log round-trip");

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
