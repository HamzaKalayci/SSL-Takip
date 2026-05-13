using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using SslDomainTakip.Models;

namespace SslDomainTakip.Services;

public class SslCheckService
{
    public async Task<CheckResult> CheckAsync(string domain)
    {
        var result = new CheckResult
        {
            Domain = domain,
            Type = CheckType.SSL
        };

        try
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(domain, 443);

            using var sslStream = new SslStream(tcpClient.GetStream(), false,
                (sender, cert, chain, errors) => true);

            await sslStream.AuthenticateAsClientAsync(domain);

            var cert = new X509Certificate2(sslStream.RemoteCertificate!);
            result.ExpireDate = cert.NotAfter.ToUniversalTime();
            result.DaysLeft = (int)(result.ExpireDate.Value - DateTime.UtcNow).TotalDays;
        }
        catch (Exception ex)
        {
            result.HasError = true;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }
}
