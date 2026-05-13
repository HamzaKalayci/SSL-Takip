namespace SslDomainTakip.Models;

public class AppSettings
{
    public int WarningDays { get; set; } = 10;
    public List<string> Domains { get; set; } = new();
    public TelegramSettings Telegram { get; set; } = new();
    public EmailSettings Email { get; set; } = new();
}

public class TelegramSettings
{
    public string BotToken { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
}

public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
}
