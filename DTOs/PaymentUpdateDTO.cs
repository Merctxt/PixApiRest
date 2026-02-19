using System.ComponentModel.DataAnnotations;

namespace PixApiRest.DTOs;

/// <summary>
/// DTO para atualização de pagamento PIX
/// </summary>
public class PaymentUpdateDTO
{
    /// <summary>
    /// Valor do pagamento
    /// </summary>
    /// <example>150.00</example>
    [Range(0.01, 99999999.99, ErrorMessage = "O valor deve estar entre R$ 0,01 e R$ 99.999.999,99")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// Descrição do pagamento
    /// </summary>
    /// <example>Pagamento atualizado</example>
    [MaxLength(140, ErrorMessage = "A descrição deve ter no máximo 140 caracteres")]
    public string? Description { get; set; }
}
