using Microsoft.EntityFrameworkCore;
using PixApiRest.Data;

namespace PixApiRest.Services;

public class PaymentCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentCleanupService> _logger;
    private readonly int _limitTimeSeconds;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public PaymentCleanupService(IServiceProvider serviceProvider, ILogger<PaymentCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _limitTimeSeconds = int.TryParse(Environment.GetEnvironmentVariable("LIMIT_TIME_PIX"), out var limit) 
            ? limit 
            : 3600; // Default: 1 hora
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de limpeza de pagamentos iniciado. Limite: {LimitTime} segundos", _limitTimeSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredPaymentsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar pagamentos expirados");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredPaymentsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PixDbContext>();

        var cutoffTime = DateTime.UtcNow.AddSeconds(-_limitTimeSeconds);

        var expiredPayments = await dbContext.Payments
            .Where(p => p.CreatedAt < cutoffTime)
            .ToListAsync();

        if (expiredPayments.Count > 0)
        {
            _logger.LogInformation("Removendo {Count} pagamentos expirados (criados antes de {CutoffTime})", 
                expiredPayments.Count, cutoffTime);

            dbContext.Payments.RemoveRange(expiredPayments);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Pagamentos expirados removidos com sucesso");
        }
    }
}
