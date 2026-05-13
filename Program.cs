using Microsoft.Extensions.Configuration;
using SslDomainTakip.Models;
using SslDomainTakip.Services;


var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var settings = config.GetSection("Settings").Get<AppSettings>()
    ?? throw new Exception("appsettings.json okunamadı!");


var sslChecker = new SslCheckService();
var domainChecker = new DomainCheckService();
var notifier = new NotificationService(settings);

Console.WriteLine("====================================");
Console.WriteLine("  SSL & Domain Takip Başlatıldı");
Console.WriteLine($"  Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}");
Console.WriteLine("====================================\n");

var warnings = new List<CheckResult>();


foreach (var domain in settings.Domains)
{
    Console.WriteLine($"🔍 {domain} kontrol ediliyor...");

    // SSL Kontrolü
    Console.Write("  [SSL]    ");
    var sslResult = await sslChecker.CheckAsync(domain);
    Console.WriteLine(sslResult.StatusText);
    if (sslResult.IsWarning(settings.WarningDays))
        warnings.Add(sslResult);

    // Domain Kontrolü
    Console.Write("  [Domain] ");
    var domainResult = await domainChecker.CheckAsync(domain);
    Console.WriteLine(domainResult.StatusText);
    if (domainResult.IsWarning(settings.WarningDays))
        warnings.Add(domainResult);

    Console.WriteLine();
}


if (warnings.Count == 0)
{
    Console.WriteLine("Tüm domain ve sertifikalar güncel. Bildirim gönderilmedi.");
}
else
{
    Console.WriteLine($"{warnings.Count} uyarı bulundu! Bildirimler gönderiliyor...\n");

    var message = BuildMessage(warnings, settings.WarningDays);

    await notifier.SendTelegramAsync(message);
    await notifier.SendEmailAsync("⚠️ SSL/Domain Bitiş Uyarısı", message);
}

Console.WriteLine("\n====================================");
Console.WriteLine("  Kontrol tamamlandı.");
Console.WriteLine("====================================");


static string BuildMessage(List<CheckResult> warnings, int warningDays)
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine($"⚠️ SSL/Domain Bitiş Uyarısı - {DateTime.Now:dd.MM.yyyy}");
    sb.AppendLine($"Son {warningDays} gün içinde sona erecek kayıtlar:\n");

    foreach (var w in warnings)
    {
        var icon = w.Type == CheckType.SSL ? "🔒" : "🌐";
        var type = w.Type == CheckType.SSL ? "SSL Sertifikası" : "Domain";
        sb.AppendLine($"{icon} {w.Domain} - {type}");
        sb.AppendLine($"   Bitiş: {w.ExpireDate:dd.MM.yyyy} ({w.DaysLeft} gün kaldı)");
        sb.AppendLine();
    }

    sb.AppendLine("Lütfen gerekli yenileme işlemlerini yapın.");
    return sb.ToString();
}
