namespace SslDomainTakip.Models;

public class CheckResult
{
    public string Domain { get; set; } = string.Empty;
    public CheckType Type { get; set; }
    public DateTime? ExpireDate { get; set; }
    public int? DaysLeft { get; set; }
    public bool HasError { get; set; }
    public string? ErrorMessage { get; set; }

    public bool IsWarning(int warningDays) =>
        !HasError && DaysLeft.HasValue && DaysLeft.Value <= warningDays;

    public string StatusText => HasError
        ? $"HATA: {ErrorMessage}"
        : $"Bitiş: {ExpireDate:dd.MM.yyyy} ({DaysLeft} gün kaldı)";
}

public enum CheckType
{
    SSL,
    Domain
}
