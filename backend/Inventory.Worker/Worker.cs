using Inventory.Worker.Data;

namespace Inventory.Worker;

public class Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();
            var exists = await context.Database.CanConnectAsync(stoppingToken);
            logger.LogInformation(
                "SQL Server conectado: {connected}",
                exists
            );
            await Task.Delay(1000, stoppingToken);
        }
    }
}