using System.IO;
using QRCoder;

namespace Ventas.Desktop.Helpers
{
    public static class QrBuilder
    {
        public static byte[] GetQrBytes(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}
