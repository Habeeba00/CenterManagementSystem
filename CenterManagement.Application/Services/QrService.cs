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
        
        // First, try to decode as Base64 (this is what the QR image contains)
        try 
        { 
            // Sanitize: restore '+' chars that scanners convert to spaces,
            // and re-pad Base64 if trailing '=' was stripped.
            var sanitized = qrCode.Trim().Replace(" ", "+");
            int mod4 = sanitized.Length % 4;
            if (mod4 > 0) sanitized += new string('=', 4 - mod4);

            return Encoding.UTF8.GetString(Convert.FromBase64String(sanitized)); 
        }
        catch 
        { 
            // Base64 decoding failed — the input may be a raw userId (GUID).
            // The Student Profile page displays the raw UserId as "Manual Entry Code",
            // so we must accept it directly if it looks like a valid GUID.
            var trimmed = qrCode.Trim();
            if (Guid.TryParse(trimmed, out _))
            {
                return trimmed;
            }
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
