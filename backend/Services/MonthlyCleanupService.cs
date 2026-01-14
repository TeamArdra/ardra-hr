namespace backend.Services;

public class MonthlyCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MonthlyCleanupService> _logger;

    public MonthlyCleanupService(
        IServiceProvider serviceProvider,
        ILogger<MonthlyCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Monthly Cleanup Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var reviewService = scope.ServiceProvider.GetRequiredService<IReviewService>();
                    await reviewService.CleanupOldReviewsAsync();
                }

                // Calculate time until first day of next month at 00:00 UTC
                var now = DateTime.UtcNow;
                var year = now.Month == 12 ? now.Year + 1 : now.Year;
                var month = now.Month == 12 ? 1 : now.Month + 1;
                var nextMonth = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                
                var delay = nextMonth - now;
                
                _logger.LogInformation(
                    $"Next cleanup scheduled for {nextMonth:yyyy-MM-dd HH:mm:ss} UTC (in {delay.TotalHours:F2} hours)");

                await Task.Delay(delay, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during monthly cleanup");
                // Wait 1 hour before retrying in case of error
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("Monthly Cleanup Service stopped");
    }
}