using System.Net;
using System.Net.Mail;
using SslDomainTakip.Models;
using Telegram.Bot;

namespace SslDomainTakip.Services;

public class NotificationService
{
    private readonly AppSettings _settings;

    public NotificationService(AppSettings settings)
    {
        _settings = settings;
    }

    public async Task SendTelegramAsync(string message)
    {
        try
        {
            var bot = new TelegramBotClient(_settings.Telegram.BotToken);
            await bot.SendMessage(
                chatId: _settings.Telegram.ChatId,
                text: message,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
            );
            Console.WriteLine("Telegram bildirimi gönderildi.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Telegram hatası: {ex.Message}");
        }
    }

    public async Task SendEmailAsync(string subject, string body)
    {
        try
        {
            var cfg = _settings.Email;

            using var smtp = new SmtpClient(cfg.SmtpServer, cfg.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(cfg.SenderEmail, cfg.SenderPassword)
            };

            var mail = new MailMessage
            {
                From = new MailAddress(cfg.SenderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mail.To.Add(cfg.RecipientEmail);

            await smtp.SendMailAsync(mail);
            Console.WriteLine("Email bildirimi gönderildi.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email hatası: {ex.Message}");
        }
    }
}