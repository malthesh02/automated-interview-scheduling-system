using System;
using System.Threading.Tasks;

namespace AutomatedInterviewSchedulingSystem.Services
{
    public interface IEmailService
    {
        Task SendStatusUpdateEmailAsync(string toEmail, string toName, string status, string message);
        Task SendInterviewScheduledEmailAsync(string toEmail, string toName, string otherPartyName, DateTime interviewDate, TimeSpan startTime);
        Task SendInterviewRescheduledEmailAsync(string toEmail, string toName, string otherPartyName, DateTime newDate, TimeSpan newStartTime, string reason);
        Task SendInterviewCancelledEmailAsync(string toEmail, string toName, string otherPartyName, DateTime interviewDate, TimeSpan startTime, string reason);
        Task SendWelcomeEmailAsync(string toEmail, string name);
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    }
}