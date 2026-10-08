using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutomatedInterviewSchedulingSystem.Models;

namespace AutomatedInterviewSchedulingSystem.Services
{
    public interface ILogService
    {
       Task LogActionAsync(string action, string description, AppLogLevel level = AppLogLevel.Info, int? userId = null);
        Task<List<Log>> GetLogsAsync(DateTime? startDate = null, DateTime? endDate = null, AppLogLevel? level = null);
        Task<List<Log>> GetRecentLogsAsync(int count = 100);
        Task ClearOldLogsAsync(int daysToKeep = 90);
    }
}
