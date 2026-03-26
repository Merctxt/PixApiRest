using PixApiRest.Entities;

namespace PixApiRest.DTOs;

/// <summary>
/// DTO de resposta de pagamento PIX
/// </summary>
public class PaymentResponseDTO
{
    /// <summary>
    /// ID do pagamento
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Valor do pagamento
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Descrição do pagamento
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Status do pagamento
    /// </summary>
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Payload PIX no padrão EMV
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// Chave PIX do recebedor
    /// </summary>
    public string PixKey { get; set; } = string.Empty;

    /// <summary>
    /// Nome do recebedor
    /// </summary>
    public string ReceiverName { get; set; } = string.Empty;

    /// <summary>
    /// Cidade do recebedor
    /// </summary>
    public string ReceiverCity { get; set; } = string.Empty;

    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data de atualização
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Data de aprovação
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
}
