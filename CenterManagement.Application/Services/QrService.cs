using System.Text;
using CenterManagement.Application.Interfaces;
using QRCoder;

namespace CenterManagement.Application.Services;

public class QrService : IQrService
{
    public string GenerateStudentQrCode(string userId)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(userId));

    public string? DecodeQrCode(string qrCode)
    {
        if (string.IsNullOrWhiteSpace(qrCode)) return null;
        try 
        { 
            // Sanitize: restore '+' chars that scanners convert to spaces,
            // and re-pad Base64 if trailing '=' was stripped.
            qrCode = qrCode.Trim().Replace(" ", "+");
            int mod4 = qrCode.Length % 4;
            if (mod4 > 0) qrCode += new string('=', 4 - mod4);

            return Encoding.UTF8.GetString(Convert.FromBase64String(qrCode)); 
        }
        catch 
        { 
            return null; 
        }
    }

    public byte[] GenerateQrCodeImage(string content)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        return qrCode.GetGraphic(10);
    }
}
