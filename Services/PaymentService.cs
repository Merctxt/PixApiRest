using Microsoft.EntityFrameworkCore;
using PixApiRest.Data;
using PixApiRest.DTOs;
using PixApiRest.Entities;
using PixApiRest.Exceptions;

namespace PixApiRest.Services;

public class PaymentService
{
    private readonly PixDbContext _context;
    private readonly PixPayloadService _pixPayloadService;
    private readonly ILogger<PaymentService> _logger;
    private readonly string _defaultReceiverCity;

    public PaymentService(
        PixDbContext context, 
        PixPayloadService pixPayloadService, 
        ILogger<PaymentService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _pixPayloadService = pixPayloadService;
        _logger = logger;
        _defaultReceiverCity = Environment.GetEnvironmentVariable("PIX_RECEIVER_CITY") ?? "SAO PAULO";
    }

    public async Task<PaymentResponseDTO> CreatePaymentAsync(PaymentCreateDTO dto)
    {
        _logger.LogInformation("Criando novo pagamento PIX com valor: {Amount}", dto.Amount);

        var receiverCity = !string.IsNullOrEmpty(dto.ReceiverCity) ? dto.ReceiverCity : _defaultReceiverCity;
        var merchantCategoryCode = dto.MerchantCategoryCode ?? "0000";

        var payload = _pixPayloadService.GerarPayload(
            dto.PixKey,
            dto.Amount,
            dto.ReceiverName,
            receiverCity,
            merchantCategoryCode);

        var payment = new Payment
        {
            Amount = dto.Amount,
            Description = dto.Description,
            Status = PaymentStatus.PENDING,
            Payload = payload,
            PixKey = dto.PixKey,
            ReceiverName = dto.ReceiverName,
            ReceiverCity = receiverCity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pagamento criado com sucesso. ID: {Id}", payment.Id);

        return ToResponseDTO(payment);
    }

    public async Task<PaymentResponseDTO> FindByIdAsync(long id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            throw new ResourceNotFoundException("Pagamento", id);
        }
        return ToResponseDTO(payment);
    }

    public async Task<List<PaymentResponseDTO>> FindAllAsync()
    {
        var payments = await _context.Payments.ToListAsync();
        return payments.Select(ToResponseDTO).ToList();
    }

    public async Task<List<PaymentResponseDTO>> FindByStatusAsync(PaymentStatus status)
    {
        var payments = await _context.Payments.Where(p => p.Status == status).ToListAsync();
        return payments.Select(ToResponseDTO).ToList();
    }

    public async Task<PaymentResponseDTO> ApprovePaymentAsync(long id)
    {
        _logger.LogInformation("Aprovando pagamento ID: {Id}", id);

        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            throw new ResourceNotFoundException("Pagamento", id);
        }

        if (payment.Status == PaymentStatus.APPROVED)
        {
            throw new BusinessException("Pagamento já foi aprovado");
        }

        if (payment.Status == PaymentStatus.CANCELLED)
        {
            throw new BusinessException("Não é possível aprovar um pagamento cancelado");
        }

        payment.Status = PaymentStatus.APPROVED;
        payment.ApprovedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pagamento aprovado com sucesso. ID: {Id}", payment.Id);

        return ToResponseDTO(payment);
    }

    public async Task<PaymentResponseDTO> CancelPaymentAsync(long id)
    {
        _logger.LogInformation("Cancelando pagamento ID: {Id}", id);

        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            throw new ResourceNotFoundException("Pagamento", id);
        }

        if (payment.Status == PaymentStatus.APPROVED)
        {
            throw new BusinessException("Não é possível cancelar um pagamento já aprovado");
        }

        if (payment.Status == PaymentStatus.CANCELLED)
        {
            throw new BusinessException("Pagamento já foi cancelado");
        }

        payment.Status = PaymentStatus.CANCELLED;
        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pagamento cancelado com sucesso. ID: {Id}", payment.Id);

        return ToResponseDTO(payment);
    }

    public async Task DeletePaymentAsync(long id)
    {
        _logger.LogInformation("Excluindo pagamento ID: {Id}", id);

        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            throw new ResourceNotFoundException("Pagamento", id);
        }

        if (payment.Status == PaymentStatus.APPROVED)
        {
            throw new BusinessException("Não é possível excluir um pagamento já aprovado");
        }

        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pagamento excluído com sucesso. ID: {Id}", id);
    }

    public async Task<string> GetPayloadAsync(long id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            throw new ResourceNotFoundException("Pagamento", id);
        }
        return payment.Payload ?? string.Empty;
    }

    private PaymentResponseDTO ToResponseDTO(Payment payment)
    {
        return new PaymentResponseDTO
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Description = payment.Description,
            Status = payment.Status,
            Payload = payment.Payload,
            PixKey = payment.PixKey,
            ReceiverName = payment.ReceiverName,
            ReceiverCity = payment.ReceiverCity,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt,
            ApprovedAt = payment.ApprovedAt
        };
    }
}
