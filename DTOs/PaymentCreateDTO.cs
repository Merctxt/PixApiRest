using System.ComponentModel.DataAnnotations;
using PixApiRest.Entities;

namespace PixApiRest.DTOs;

/// <summary>
/// DTO para criação de pagamento PIX
/// </summary>
public class PaymentCreateDTO
{
    /// <summary>
    /// Valor do pagamento
    /// </summary>
    /// <example>100.00</example>
    [Required(ErrorMessage = "O valor é obrigatório")]
    [Range(0.01, 99999999.99, ErrorMessage = "O valor deve estar entre R$ 0,01 e R$ 99.999.999,99")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Descrição do pagamento
    /// </summary>
    /// <example>Pagamento de produto</example>
    [MaxLength(140, ErrorMessage = "A descrição deve ter no máximo 140 caracteres")]
    public string? Description { get; set; }

    /// <summary>
    /// Identificador único da transação (txid)
    /// </summary>
    /// <example>PEDIDO123</example>
    [MaxLength(25, ErrorMessage = "O identificador de transação deve ter no máximo 25 caracteres")]
    [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "O identificador deve conter apenas letras e números")]
    public string? Txid { get; set; }

    /// <summary>
    /// Chave PIX do recebedor
    /// </summary>
    /// <example>email@exemplo.com</example>
    [Required(ErrorMessage = "A chave PIX é obrigatória")]
    [MaxLength(77, ErrorMessage = "A chave PIX deve ter no máximo 77 caracteres")]
    public string PixKey { get; set; } = string.Empty;

    /// <summary>
    /// Tipo da chave PIX
    /// </summary>
    [Required(ErrorMessage = "O tipo de chave PIX é obrigatório")]
    public PixKeyType PixKeyType { get; set; }

    /// <summary>
    /// Nome do recebedor
    /// </summary>
    /// <example>Venus Store</example>
    [Required(ErrorMessage = "O nome do recebedor é obrigatório")]
    [MaxLength(25, ErrorMessage = "O nome deve ter no máximo 25 caracteres")]
    public string ReceiverName { get; set; } = string.Empty;

    /// <summary>
    /// Cidade do recebedor
    /// </summary>
    /// <example>SAO PAULO</example>
    [Required(ErrorMessage = "A cidade do recebedor é obrigatória")]
    [MaxLength(15, ErrorMessage = "A cidade deve ter no máximo 15 caracteres")]
    public string ReceiverCity { get; set; } = string.Empty;

    /// <summary>
    /// Código de categoria do comerciante (MCC)
    /// </summary>
    /// <example>0000</example>
    [RegularExpression("^[0-9]{4}$", ErrorMessage = "O código de categoria deve ter 4 dígitos")]
    public string? MerchantCategoryCode { get; set; } = "0000";
}
