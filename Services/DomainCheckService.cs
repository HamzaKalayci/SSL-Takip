using System.Net.Sockets;
using System.Text.RegularExpressions;
using Whois;
using SslDomainTakip.Models;

namespace SslDomainTakip.Services;

public class DomainCheckService
{
    private readonly WhoisLookup _whois = new();

    public async Task<CheckResult> CheckAsync(string domain)
    {
        var result = new CheckResult
        {
            Domain = domain,
            Type = CheckType.Domain
        };

        try
        {
            // Önce standart WHOIS kütüphanesiyle dene
            var response = await _whois.LookupAsync(domain);
            if (response?.Expiration != null)
            {
                result.ExpireDate = response.Expiration.Value.ToUniversalTime();
                result.DaysLeft = (int)(result.ExpireDate.Value - DateTime.UtcNow).TotalDays;
                return result;
            }

            // .com.tr gibi domainler için manuel WHOIS sorgusu
            var whoisServer = domain.EndsWith(".tr") ? "whois.nic.tr" : "whois.verisign-grs.com";
            var rawWhois = await QueryWhoisServerAsync(whoisServer, domain);
            var expireDate = ParseExpireDate(rawWhois);

            if (expireDate != null)
            {
                result.ExpireDate = expireDate.Value.ToUniversalTime();
                result.DaysLeft = (int)(result.ExpireDate.Value - DateTime.UtcNow).TotalDays;
            }
            else
            {
                result.HasError = true;
                result.ErrorMessage = "WHOIS'tan bitiş tarihi alınamadı.";
            }
        }
        catch (Exception ex)
        {
            result.HasError = true;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private static async Task<string> QueryWhoisServerAsync(string server, string domain)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(server, 43);

        await using var stream = client.GetStream();
        await using var writer = new StreamWriter(stream) { AutoFlush = true };
        using var reader = new StreamReader(stream);

        await writer.WriteLineAsync(domain);
        return await reader.ReadToEndAsync();
    }

    private static DateTime? ParseExpireDate(string whoisText)
    {
        var patterns = new[]
        {
            @"Expir\w+\s*Date\s*[:\.]?\s*(.+)",
            @"paid-till\s*[:\.]?\s*(.+)",
            @"Registry Expiry Date\s*[:\.]?\s*(.+)",
            @"Expiry date\s*[:\.]?\s*(.+)",
            @"Renewal date\s*[:\.]?\s*(.+)"
        };

        foreach (var pattern in patterns)
        {
            var match = Regex.Match(whoisText, pattern, RegexOptions.IgnoreCase);
            if (!match.Success) continue;

            var raw = match.Groups[1].Value.Trim()
                .Split('\n')[0].Trim()
                .Replace("T", " ").Replace("Z", "")
                .Split('.')[0].Trim();

            var formats = new[]
            {
                "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd",
                "dd.MM.yyyy", "dd-MMM-yyyy", "MM/dd/yyyy"
            };

            foreach (var fmt in formats)
            {
                if (DateTime.TryParseExact(raw, fmt,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var date))
                    return date;
            }
        }

        return null;
    }
}