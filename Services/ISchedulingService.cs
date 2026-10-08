using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutomatedInterviewSchedulingSystem.DTOs;
using AutomatedInterviewSchedulingSystem.Models;

namespace AutomatedInterviewSchedulingSystem.Services
{
    public interface ISchedulingService
    {
        // Schedule Management
        Task<SchedulingResultDto> ScheduleInterviewAsync(ScheduleRequestDto request);
        Task<SchedulingResultDto> AIScheduleInterviewAsync(AIScheduleRequestDto request);
        Task<SchedulingResultDto> RescheduleInterviewAsync(RescheduleRequestDto request);
        Task<bool> CancelInterviewAsync(int interviewId, string reason);
        
        // Conflict Detection
        Task<List<ConflictDto>> DetectConflictsAsync(int interviewId);
        Task<List<ConflictDto>> GetAllConflictsAsync();
        Task<List<ConflictDto>> GetUnresolvedConflictsAsync();
        Task<bool> ResolveConflictAsync(ResolveConflictDto request);
        
        // Availability Management
        Task<List<AvailabilityDto>> GetAvailableSlotsAsync(int interviewerId, DateTime startDate, DateTime endDate);
        Task<List<AvailabilityDto>> FindBestAvailableSlotsAsync(int interviewerId, DateTime startDate, DateTime endDate, int durationMinutes);
        
        // Interview Queries
        Task<List<InterviewDto>> GetInterviewsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<InterviewDto>> GetInterviewsByCandidateAsync(int candidateId);
        Task<List<InterviewDto>> GetInterviewsByInterviewerAsync(int interviewerId);
        Task<InterviewDto> GetInterviewByIdAsync(int interviewId);
    }
}
