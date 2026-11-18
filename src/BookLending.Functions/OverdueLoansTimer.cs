using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BookLending.Functions;

public class OverdueLoansTimer
{
    private readonly ILogger _logger;

    public OverdueLoansTimer(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<OverdueLoansTimer>();
    }

    [Function("OverdueLoansTimer")]
    public void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}