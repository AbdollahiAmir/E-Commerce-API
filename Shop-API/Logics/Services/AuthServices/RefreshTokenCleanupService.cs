using Microsoft.EntityFrameworkCore;
using Shop_API.Data;

namespace Shop_API.Logics.Services.AuthServices
{
    public class RefreshTokenCleanupService : BackgroundService
    {
        private const int BatchSize = 1000;

        private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
        private static readonly TimeSpan Retention = TimeSpan.FromDays(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RefreshTokenCleanupService> _logger;

        public RefreshTokenCleanupService(IServiceScopeFactory scopeFactory, ILogger<RefreshTokenCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(InitialDelay, stoppingToken);

                using var timer = new PeriodicTimer(Interval);

                do
                {
                    await CleanAsync(stoppingToken);
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task CleanAsync(CancellationToken cancellationToken)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var threshold = DateTime.UtcNow.Subtract(Retention);

                var total = 0;

                while (true)
                {
                    var ids = await context.RefreshTokens
                        .Where(t => t.ExpiresAt < threshold)
                        .Select(t => t.Id)
                        .Take(BatchSize)
                        .ToListAsync(cancellationToken);

                    if (ids.Count == 0)
                    {
                        break;
                    }

                    total += await context.RefreshTokens
                        .Where(t => ids.Contains(t.Id))
                        .ExecuteDeleteAsync(cancellationToken);

                    if (ids.Count < BatchSize)
                    {
                        break;
                    }
                }

                if (total > 0)
                {
                    _logger.LogInformation("{Count} نشست منقضی پاک شد.", total);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "پاک‌سازی نشست‌های منقضی ناموفق بود.");
            }
        }
    }
}