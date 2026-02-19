using System.Globalization;
using System.Text;

namespace PixApiRest.Services;

public class PixPayloadService
{
    public string GerarPayload(string chave, decimal valor, string nomeRecebedor,
                                string cidadeRecebedor, string txid, string? merchantCategoryCode)
    {
        var nomeSanitizado = SanitizarNome(nomeRecebedor);
        var cidadeSanitizada = SanitizarCidade(cidadeRecebedor);
        var txidSanitizado = SanitizarTxid(txid);
        var mcc = merchantCategoryCode ?? "0000";

        var payload = new StringBuilder();

        // Payload Format Indicator
        payload.Append(FormatarCampo("00", "01"));

        // Merchant Account Information
        var merchantAccountInfo = FormatarCampo("00", "BR.GOV.BCB.PIX") + FormatarCampo("01", chave);
        payload.Append(FormatarCampo("26", merchantAccountInfo));

        // Merchant Category Code
        payload.Append(FormatarCampo("52", mcc));

        // Transaction Currency (986 = BRL)
        payload.Append(FormatarCampo("53", "986"));

        // Transaction Amount
        payload.Append(FormatarCampo("54", FormatarValor(valor)));

        // Country Code
        payload.Append(FormatarCampo("58", "BR"));

        // Merchant Name
        payload.Append(FormatarCampo("59", nomeSanitizado));

        // Merchant City
        payload.Append(FormatarCampo("60", cidadeSanitizada));

        // Additional Data Field Template (TXID)
        var additionalData = FormatarCampo("05", txidSanitizado);
        payload.Append(FormatarCampo("62", additionalData));

        // CRC16
        var payloadComCrc = payload.ToString() + "6304";
        var crc = CalcularCRC16(payloadComCrc);

        return payloadComCrc + crc;
    }

    private string FormatarCampo(string id, string valor)
    {
        return id + valor.Length.ToString("D2") + valor;
    }

    private string FormatarValor(decimal valor)
    {
        return valor.ToString("F2", CultureInfo.InvariantCulture);
    }

    private string RemoverAcentos(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return string.Empty;

        var normalizedString = texto.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    private string SanitizarNome(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return string.Empty;

        var sanitizado = RemoverAcentos(nome);
        sanitizado = System.Text.RegularExpressions.Regex.Replace(sanitizado, @"[^a-zA-Z0-9 ]", "").Trim();

        return sanitizado.Length > 25 ? sanitizado[..25] : sanitizado;
    }

    private string SanitizarCidade(string cidade)
    {
        if (string.IsNullOrEmpty(cidade)) return string.Empty;

        var sanitizado = RemoverAcentos(cidade);
        sanitizado = System.Text.RegularExpressions.Regex.Replace(sanitizado, @"[^a-zA-Z0-9 ]", "").Trim();

        return sanitizado.Length > 15 ? sanitizado[..15] : sanitizado;
    }

    private string SanitizarTxid(string? txid)
    {
        if (string.IsNullOrEmpty(txid)) return "***";

        var sanitizado = System.Text.RegularExpressions.Regex.Replace(txid, @"[^a-zA-Z0-9]", "");

        return sanitizado.Length > 25 ? sanitizado[..25] : sanitizado;
    }

    private string CalcularCRC16(string payload)
    {
        int crc = 0xFFFF;
        int polynomial = 0x1021;
        byte[] bytes = Encoding.UTF8.GetBytes(payload);

        foreach (byte b in bytes)
        {
            crc ^= (b & 0xFF) << 8;
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) != 0)
                {
                    crc = (crc << 1) ^ polynomial;
                }
                else
                {
                    crc = crc << 1;
                }
            }
        }

        crc &= 0xFFFF;
        return crc.ToString("X4");
    }
}
