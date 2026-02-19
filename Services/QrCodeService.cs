using QRCoder;

namespace PixApiRest.Services;

public class QrCodeService
{
    private const int DefaultWidth = 300;
    private const int DefaultHeight = 300;

    public byte[] GerarQrCode(string payload)
    {
        return GerarQrCode(payload, DefaultWidth, DefaultHeight);
    }

    public byte[] GerarQrCode(string payload, int width, int height)
    {
        try
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.H);
            using var qrCode = new PngByteQRCode(qrCodeData);
            
            // Calculate pixels per module to achieve desired dimensions
            int pixelsPerModule = Math.Max(width, height) / qrCodeData.ModuleMatrix.Count;
            if (pixelsPerModule < 1) pixelsPerModule = 1;
            
            return qrCode.GetGraphic(pixelsPerModule);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao gerar QR Code", ex);
        }
    }
}
