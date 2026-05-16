using PartSphere.Services;

namespace PartSphere.BackgroundServices
{
    public class DailyCronService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyCronService> _logger;

        public DailyCronService(IServiceProvider serviceProvider, ILogger<DailyCronService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Cron Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Daily Cron Service is working.");

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var creditService = scope.ServiceProvider.GetRequiredService<ICreditService>();

                    await creditService.SendOverdueRemindersAsync();

                    _logger.LogInformation("Successfully processed automatic email reminders for unpaid credits.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing daily cron jobs.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("Daily Cron Service is stopping.");
        }
    }
}
