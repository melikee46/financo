using System.Globalization;
using System.Text;
using System.Text.Json;
using Finaco.Api.Domain;
using Finaco.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Finaco.Api.Application.Imports;

public sealed class ImportPreviewService(FinacoDbContext db)
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    public async Task<ImportPreviewResponse> PreviewAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0) throw new CsvImportException("CSV dosyası boş.");
        if (file.Length > MaxFileSize) throw new CsvImportException("CSV dosyası 5 MB sınırını aşamaz.");

        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var lines = new List<string>();
        while (await reader.ReadLineAsync(cancellationToken) is { } line) lines.Add(line);
        if (lines.Count < 2) throw new CsvImportException("CSV dosyasında başlık ve en az bir veri satırı bulunmalı.");

        var headers = ParseCsvLine(lines[0]).Select(NormalizeHeader).ToArray();
        var dateIndex = FindHeader(headers, "tarih", "date");
        var descriptionIndex = FindHeader(headers, "aciklama", "description", "açıklama");
        var amountIndex = FindHeader(headers, "tutar", "amount");
        var currencyIndex = FindHeader(headers, "parabirimi", "currency", "currencycode", "para birimi");

        var missingHeaders = new List<string>();
        if (dateIndex < 0) missingHeaders.Add("tarih/date");
        if (descriptionIndex < 0) missingHeaders.Add("açıklama/description");
        if (amountIndex < 0) missingHeaders.Add("tutar/amount");
        if (missingHeaders.Count > 0)
            throw new CsvImportException($"Zorunlu sütunlar eksik: {string.Join(", ", missingHeaders)}.");

        var parsedRows = lines.Skip(1).Select((line, index) => ParseRow(line, index + 2, dateIndex, descriptionIndex, amountIndex, currencyIndex)).ToList();
        var job = new ImportJob { Id = Guid.NewGuid(), FileName = Path.GetFileName(file.FileName), Status = ImportStatus.Pending, CreatedAt = DateTimeOffset.UtcNow };
        job.Rows = parsedRows.Select(row => new ImportRow
        {
            Id = Guid.NewGuid(), ImportJobId = job.Id, RowNumber = row.RowNumber,
            ParsedValuesJson = JsonSerializer.Serialize(new ParsedRow(row.TransactionDate, row.Description, row.Amount, row.CurrencyCode)),
            ValidationErrorsJson = JsonSerializer.Serialize(row.Errors)
        }).ToList();
        db.ImportJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        return new ImportPreviewResponse(job.Id, job.FileName, parsedRows.Count, parsedRows.Count(x => x.Errors.Count == 0), parsedRows.Count(x => x.Errors.Count > 0), parsedRows.Select(ToResponse).ToList());
    }

    public async Task<ImportConfirmResponse?> ConfirmAsync(Guid importJobId, CancellationToken cancellationToken)
    {
        var job = await db.ImportJobs
            .Include(importJob => importJob.Rows)
            .SingleOrDefaultAsync(importJob => importJob.Id == importJobId, cancellationToken);

        if (job is null) return null;

        var validRows = job.Rows
            .Where(row => DeserializeValidationErrors(row.ValidationErrorsJson).Count == 0)
            .ToList();
        var invalidRows = job.Rows.Count - validRows.Count;

        if (job.Status == ImportStatus.Confirmed)
        {
            return new ImportConfirmResponse(job.Id, job.Status, job.Rows.Count, validRows.Count, invalidRows, validRows.Count, true);
        }

        if (validRows.Count == 0)
            throw new CsvImportException("Onaylanabilecek geçerli işlem bulunmuyor.");

        var accounts = await db.Accounts.OrderBy(account => account.CreatedAt).ToListAsync(cancellationToken);
        if (accounts.Count == 0)
            throw new CsvImportException("İşlemleri kaydetmek için en az bir hesap tanımlanmalıdır.");
        if (accounts.Count > 1)
            throw new CsvImportException("Birden fazla hesap bulundu. İşlem hesabı seçimi henüz desteklenmiyor.");

        var account = accounts[0];
        var transactions = new List<Transaction>(validRows.Count);

        foreach (var row in validRows)
        {
            var parsed = DeserializeParsedRow(row.ParsedValuesJson);
            if (parsed.TransactionDate is null || parsed.Amount is null || string.IsNullOrWhiteSpace(parsed.Description) || parsed.Amount <= 0 || parsed.CurrencyCode.Length != 3)
                throw new CsvImportException($"{row.RowNumber}. satırın kaydedilmiş verisi geçersiz.");

            transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                TransactionDate = parsed.TransactionDate.Value,
                DescriptionOriginal = parsed.Description,
                DescriptionNormalized = null,
                Amount = parsed.Amount.Value,
                CurrencyCode = parsed.CurrencyCode,
                Type = TransactionType.Expense,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.Transactions.AddRange(transactions);
        job.Status = ImportStatus.Confirmed;
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ImportConfirmResponse(job.Id, job.Status, job.Rows.Count, validRows.Count, invalidRows, transactions.Count, false);
    }

    private static ParsedImportRow ParseRow(string line, int rowNumber, int dateIndex, int descriptionIndex, int amountIndex, int currencyIndex)
    {
        var values = ParseCsvLine(line);
        var errors = new List<string>();
        var dateText = ValueAt(values, dateIndex);
        var description = ValueAt(values, descriptionIndex);
        var amountText = ValueAt(values, amountIndex);
        var currency = currencyIndex >= 0 ? ValueAt(values, currencyIndex).ToUpperInvariant() : "TRY";
        DateOnly? date = TryParseDate(dateText, out var parsedDate) ? parsedDate : null;
        if (date is null) errors.Add("Tarih Türkçe veya ISO biçiminde okunamadı.");
        if (string.IsNullOrWhiteSpace(description)) errors.Add("Açıklama boş olamaz.");
        if (!TryParseAmount(amountText, out var parsedAmount) || parsedAmount == 0) errors.Add("Tutar geçerli ve sıfırdan farklı olmalı.");
        if (currency.Length != 3) errors.Add("Para birimi üç harfli olmalı.");
        return new ParsedImportRow(rowNumber, date, description, parsedAmount == 0 ? null : Math.Abs(parsedAmount), currency, errors);
    }

    private static ImportRowPreviewResponse ToResponse(ParsedImportRow row) => new(row.RowNumber, row.Errors.Count == 0, row.TransactionDate?.ToString("yyyy-MM-dd"), row.Description, row.Amount, row.CurrencyCode, row.Errors);
    private static string ValueAt(IReadOnlyList<string> values, int index) => index >= 0 && index < values.Count ? values[index].Trim() : string.Empty;
    private static int FindHeader(IReadOnlyList<string> headers, params string[] names) => Array.FindIndex(headers.ToArray(), header => names.Contains(header, StringComparer.OrdinalIgnoreCase));
    private static string NormalizeHeader(string header) => header.Trim().Trim('\uFEFF').ToLowerInvariant().Replace("_", string.Empty).Replace("-", string.Empty);

    internal static bool TryParseDate(string value, out DateOnly date)
    {
        var formats = new[] { "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" };
        return DateOnly.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
            || DateOnly.TryParse(value.Trim(), CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out date);
    }

    internal static bool TryParseAmount(string value, out decimal amount)
    {
        var normalized = value.Trim().Replace("₺", string.Empty).Replace("TL", string.Empty, StringComparison.OrdinalIgnoreCase).Replace(" ", string.Empty);
        if (normalized.Contains(',') && normalized.Contains('.')) normalized = normalized.Replace(".", string.Empty).Replace(',', '.');
        else if (normalized.Contains(',')) normalized = normalized.Replace(',', '.');
        else if (normalized.Count(c => c == '.') > 1) normalized = normalized.Replace(".", string.Empty);
        return decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out amount);
    }

    internal static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>(); var value = new StringBuilder(); var quoted = false;
        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];
            if (character == '"' && quoted && i + 1 < line.Length && line[i + 1] == '"') { value.Append('"'); i++; continue; }
            if (character == '"') { quoted = !quoted; continue; }
            if (character == ',' && !quoted) { values.Add(value.ToString()); value.Clear(); continue; }
            value.Append(character);
        }
        values.Add(value.ToString()); return values;
    }

    private static IReadOnlyList<string> DeserializeValidationErrors(string json) =>
        JsonSerializer.Deserialize<List<string>>(json) ?? new List<string> { "Satır doğrulama sonucu okunamadı." };

    private static ParsedRow DeserializeParsedRow(string json) =>
        JsonSerializer.Deserialize<ParsedRow>(json) ?? throw new CsvImportException("İçe aktarma satır verisi okunamadı.");

    private sealed record ParsedImportRow(int RowNumber, DateOnly? TransactionDate, string Description, decimal? Amount, string CurrencyCode, IReadOnlyList<string> Errors);
    private sealed record ParsedRow(DateOnly? TransactionDate, string Description, decimal? Amount, string CurrencyCode);
}

public sealed class CsvImportException(string message) : Exception(message);
