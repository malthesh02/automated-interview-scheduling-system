using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutomatedInterviewSchedulingSystem.Data;
using AutomatedInterviewSchedulingSystem.Models;

namespace AutomatedInterviewSchedulingSystem.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(
            string action,
            string description,
            AppLogLevel level = AppLogLevel.Info,
            int? userId = null)
        {
            try
            {
                var log = new Log
                {
                    Action = action,
                    Description = description,
                    Level = level,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Logs.Add(log);
                await _context.SaveChangesAsync();

                // Also log to console in development
                Console.WriteLine($"[{level}] {action}: {description}");
            }
            catch (Exception ex)
            {
                // Fallback logging to console if database logging fails
                Console.WriteLine($"Error logging to database: {ex.Message}");
                Console.WriteLine($"[{level}] {action}: {description}");
            }
        }

        public async Task<List<Log>> GetLogsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            AppLogLevel? level = null)
        {
            var query = _context.Logs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.CreatedAt <= endDate.Value);

            if (level.HasValue)
                query = query.Where(l => l.Level == level.Value);

            return await query
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Log>> GetRecentLogsAsync(int count = 100)
        {
            return await _context.Logs
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task ClearOldLogsAsync(int daysToKeep = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

                var oldLogs = await _context.Logs
                    .Where(l => l.CreatedAt < cutoffDate)
                    .ToListAsync();

                if (oldLogs.Any())
                {
                    _context.Logs.RemoveRange(oldLogs);
                    await _context.SaveChangesAsync();

                    await LogActionAsync(
                        "Logs Cleared",
                        $"Cleared {oldLogs.Count} logs older than {daysToKeep} days",
                        AppLogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing old logs: {ex.Message}");
            }
        }
    }
}
