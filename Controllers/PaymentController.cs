using Microsoft.AspNetCore.Mvc;
using PixApiRest.DTOs;
using PixApiRest.Entities;
using PixApiRest.Services;

namespace PixApiRest.Controllers;

/// <summary>
/// Endpoints para gerenciamento de pagamentos PIX
/// </summary>
[ApiController]
[Route("api/payments")]
[Produces("application/json")]
[Tags("Pagamentos PIX")]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly QrCodeService _qrCodeService;

    public PaymentController(PaymentService paymentService, QrCodeService qrCodeService)
    {
        _paymentService = paymentService;
        _qrCodeService = qrCodeService;
    }

    /// <summary>
    /// Criar pagamento PIX
    /// </summary>
    /// <param name="dto">Dados para criação do pagamento</param>
    /// <returns>Pagamento criado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentResponseDTO>> CreatePayment([FromBody] PaymentCreateDTO dto)
    {
        var result = await _paymentService.CreatePaymentAsync(dto);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Buscar pagamento por ID
    /// </summary>
    /// <param name="id">ID do pagamento</param>
    /// <returns>Pagamento encontrado</returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(PaymentResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponseDTO>> FindById(long id)
    {
        var result = await _paymentService.FindByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Buscar pagamento por TXID
    /// </summary>
    /// <param name="txid">TXID do pagamento</param>
    /// <returns>Pagamento encontrado</returns>
    [HttpGet("txid/{txid}")]
    [ProducesResponseType(typeof(PaymentResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponseDTO>> FindByTxid(string txid)
    {
        var result = await _paymentService.FindByTxidAsync(txid);
        return Ok(result);
    }

    /// <summary>
    /// Listar pagamentos
    /// </summary>
    /// <returns>Lista de pagamentos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<PaymentResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PaymentResponseDTO>>> FindAll()
    {
        var result = await _paymentService.FindAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Listar pagamentos por status
    /// </summary>
    /// <param name="status">Status do pagamento</param>
    /// <returns>Lista de pagamentos</returns>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(List<PaymentResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PaymentResponseDTO>>> FindByStatus(PaymentStatus status)
    {
        var result = await _paymentService.FindByStatusAsync(status);
        return Ok(result);
    }

    /// <summary>
    /// Atualizar pagamento
    /// </summary>
    /// <param name="id">ID do pagamento</param>
    /// <param name="dto">Dados para atualização</param>
    /// <returns>Pagamento atualizado</returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(PaymentResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponseDTO>> UpdatePayment(long id, [FromBody] PaymentUpdateDTO dto)
    {
        var result = await _paymentService.UpdatePaymentAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Aprovar pagamento
    /// </summary>
    /// <param name="id">ID do pagamento</param>
    /// <returns>Pagamento aprovado</returns>
    [HttpPatch("{id:long}/approve")]
    [ProducesResponseType(typeof(PaymentResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponseDTO>> ApprovePayment(long id)
    {
        var result = await _paymentService.ApprovePaymentAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Cancelar pagamento
    /// </summary>
    /// <param name="id">ID do pagamento</param>
    /// <returns>Pagamento cancelado</returns>
    [HttpPatch("{id:long}/cancel")]
    [ProducesResponseType(typeof(PaymentResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponseDTO>> CancelPayment(long id)
    {
        var result = await _paymentService.CancelPaymentAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Excluir pagamento
    /// </summary>
    /// <param name="id">ID do pagamento</param>
    /// <returns>Sem conteúdo</returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePayment(long id)
    {
        await _paymentService.DeletePaymentAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Obter payload PIX
    /// </summary>
    /// <param name="id">ID do pagamento</param>
    /// <returns>Payload PIX</returns>
    [HttpGet("{id:long}/payload")]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayload(long id)
    {
        var payload = await _paymentService.GetPayloadAsync(id);
        return Content(payload, "text/plain");
    }

    /// <summary>
    /// Gerar QR Code
    /// </summary>
    /// <param name="id">ID do pagamento</param>
    /// <param name="width">Largura do QR Code (padrão: 300)</param>
    /// <param name="height">Altura do QR Code (padrão: 300)</param>
    /// <returns>Imagem do QR Code em PNG</returns>
    [HttpGet("{id:long}/qrcode")]
    [Produces("image/png")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQrCode(long id, [FromQuery] int width = 300, [FromQuery] int height = 300)
    {
        var payload = await _paymentService.GetPayloadAsync(id);
        var qrCodeImage = _qrCodeService.GerarQrCode(payload, width, height);
        return File(qrCodeImage, "image/png");
    }
}
