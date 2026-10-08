using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutomatedInterviewSchedulingSystem.Data;
using AutomatedInterviewSchedulingSystem.DTOs;
using AutomatedInterviewSchedulingSystem.Models;

namespace AutomatedInterviewSchedulingSystem.Services
{
    public class SchedulingService : ISchedulingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogService _logService;

        public SchedulingService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogService logService)
        {
            _context = context;
            _emailService = emailService;
            _logService = logService;
        }

        #region Schedule Management

        public async Task<SchedulingResultDto> ScheduleInterviewAsync(ScheduleRequestDto request)
        {
            try
            {
                // Validate candidate exists
                var candidate = await _context.Candidates
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.CandidateId == request.CandidateId);

                if (candidate == null)
                {
                    return new SchedulingResultDto
                    {
                        Success = false,
                        Message = "Candidate not found"
                    };
                }

                // Validate interviewer exists
                var interviewer = await _context.Interviewers
                    .Include(i => i.User)
                    .FirstOrDefaultAsync(i => i.InterviewerId == request.InterviewerId);

                if (interviewer == null)
                {
                    return new SchedulingResultDto
                    {
                        Success = false,
                        Message = "Interviewer not found"
                    };
                }

                // Check if interviewer is available at requested time
                var isAvailable = await CheckInterviewerAvailabilityAsync(
                    request.InterviewerId,
                    request.InterviewDate,
                    request.StartTime,
                    request.EndTime);

                if (!isAvailable)
                {
                    var alternatives = await FindBestAvailableSlotsAsync(
                        request.InterviewerId,
                        request.InterviewDate,
                        request.InterviewDate.AddDays(7),
                        (int)(request.EndTime - request.StartTime).TotalMinutes);

                    return new SchedulingResultDto
                    {
                        Success = false,
                        Message = "Interviewer is not available at the requested time",
                        AlternativeSlots = alternatives.ToArray()
                    };
                }

                // Check for conflicts
                var conflicts = await DetectPotentialConflictsAsync(
                    request.CandidateId,
                    request.InterviewerId,
                    request.InterviewDate,
                    request.StartTime,
                    request.EndTime);

                if (conflicts.Any())
                {
                    return new SchedulingResultDto
                    {
                        Success = false,
                        Message = "Scheduling conflicts detected",
                        Conflicts = conflicts.ToArray()
                    };
                }

                // Create the interview
                var interview = new ScheduledInterview
                {
                    CandidateId = request.CandidateId,
                    InterviewerId = request.InterviewerId,
                    InterviewDate = request.InterviewDate,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Status = InterviewStatus.Scheduled,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ScheduledInterviews.Add(interview);
                await _context.SaveChangesAsync();

                // Send confirmation emails
                await _emailService.SendInterviewScheduledEmailAsync(
                    candidate.User.Email,
                    candidate.User.Name,
                    interviewer.User.Name,
                    request.InterviewDate,
                    request.StartTime);

                await _emailService.SendInterviewScheduledEmailAsync(
                    interviewer.User.Email,
                    interviewer.User.Name,
                    candidate.User.Name,
                    request.InterviewDate,
                    request.StartTime);

                // Log the action
                await _logService.LogActionAsync(
                    "Interview Scheduled",
                    $"Interview scheduled for candidate {candidate.User.Name} with interviewer {interviewer.User.Name}",
                    AppLogLevel.Info);

                return new SchedulingResultDto
                {
                    Success = true,
                    Message = "Interview scheduled successfully",
                    Interview = await MapToInterviewDtoAsync(interview)
                };
            }
            catch (Exception ex)
            {
                await _logService.LogActionAsync(
                    "Schedule Interview Error",
                    $"Error scheduling interview: {ex.Message}",
                    AppLogLevel.Error);

                return new SchedulingResultDto
                {
                    Success = false,
                    Message = $"Error scheduling interview: {ex.Message}"
                };
            }
        }

        public async Task<SchedulingResultDto> AIScheduleInterviewAsync(AIScheduleRequestDto request)
        {
            try
            {
                // Get all available slots for the interviewer in the date range
                var availableSlots = await FindBestAvailableSlotsAsync(
                    request.InterviewerId,
                    request.PreferredStartDate,
                    request.PreferredEndDate,
                    request.DurationMinutes);

                if (!availableSlots.Any())
                {
                    return new SchedulingResultDto
                    {
                        Success = false,
                        Message = "No available slots found in the specified date range"
                    };
                }

                // AI logic: Score and rank slots based on preferences
                var scoredSlots = availableSlots.Select(slot => new
                {
                    Slot = slot,
                    Score = CalculateSlotScore(slot, request.PreferredTimeOfDay)
                }).OrderByDescending(s => s.Score).ToList();

                var bestSlot = scoredSlots.First().Slot;

                // Calculate end time based on duration
                var endTime = bestSlot.StartTime.Add(TimeSpan.FromMinutes(request.DurationMinutes));

                // Schedule using the best slot
                var scheduleRequest = new ScheduleRequestDto
                {
                    CandidateId = request.CandidateId,
                    InterviewerId = request.InterviewerId,
                    InterviewDate = bestSlot.AvailableDate,
                    StartTime = bestSlot.StartTime,
                    EndTime = endTime
                };

                return await ScheduleInterviewAsync(scheduleRequest);
            }
            catch (Exception ex)
            {
                await _logService.LogActionAsync(
                    "AI Schedule Error",
                    $"Error in AI scheduling: {ex.Message}",
                    AppLogLevel.Error);

                return new SchedulingResultDto
                {
                    Success = false,
                    Message = $"Error in AI scheduling: {ex.Message}"
                };
            }
        }

        public async Task<SchedulingResultDto> RescheduleInterviewAsync(RescheduleRequestDto request)
        {
            try
            {
                var interview = await _context.ScheduledInterviews
                    .Include(i => i.Candidate).ThenInclude(c => c.User)
                    .Include(i => i.Interviewer).ThenInclude(i => i.User)
                    .FirstOrDefaultAsync(i => i.InterviewId == request.InterviewId);

                if (interview == null)
                {
                    return new SchedulingResultDto
                    {
                        Success = false,
                        Message = "Interview not found"
                    };
                }

                // Check availability for new time
                var isAvailable = await CheckInterviewerAvailabilityAsync(
                    interview.InterviewerId,
                    request.NewInterviewDate,
                    request.NewStartTime,
                    request.NewEndTime);

                if (!isAvailable)
                {
                    return new SchedulingResultDto
                    {
                        Success = false,
                        Message = "Interviewer is not available at the new time"
                    };
                }

                // Check for conflicts
                var conflicts = await DetectPotentialConflictsAsync(
                    interview.CandidateId,
                    interview.InterviewerId,
                    request.NewInterviewDate,
                    request.NewStartTime,
                    request.NewEndTime,
                    request.InterviewId); // Exclude current interview

                if (conflicts.Any())
                {
                    return new SchedulingResultDto
                    {
                        Success = false,
                        Message = "Scheduling conflicts detected for the new time",
                        Conflicts = conflicts.ToArray()
                    };
                }

                // Update interview
                interview.InterviewDate = request.NewInterviewDate;
                interview.StartTime = request.NewStartTime;
                interview.EndTime = request.NewEndTime;
                interview.Status = InterviewStatus.Rescheduled;
                interview.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send notification emails
                await _emailService.SendInterviewRescheduledEmailAsync(
                    interview.Candidate.User.Email,
                    interview.Candidate.User.Name,
                    interview.Interviewer.User.Name,
                    request.NewInterviewDate,
                    request.NewStartTime,
                    request.Reason);

                await _emailService.SendInterviewRescheduledEmailAsync(
                    interview.Interviewer.User.Email,
                    interview.Interviewer.User.Name,
                    interview.Candidate.User.Name,
                    request.NewInterviewDate,
                    request.NewStartTime,
                    request.Reason);

                await _logService.LogActionAsync(
                    "Interview Rescheduled",
                    $"Interview {request.InterviewId} rescheduled. Reason: {request.Reason}",
                    AppLogLevel.Info);

                return new SchedulingResultDto
                {
                    Success = true,
                    Message = "Interview rescheduled successfully",
                    Interview = await MapToInterviewDtoAsync(interview)
                };
            }
            catch (Exception ex)
            {
                await _logService.LogActionAsync(
                    "Reschedule Error",
                    $"Error rescheduling interview: {ex.Message}",
                    AppLogLevel.Error);

                return new SchedulingResultDto
                {
                    Success = false,
                    Message = $"Error rescheduling interview: {ex.Message}"
                };
            }
        }

        public async Task<bool> CancelInterviewAsync(int interviewId, string reason)
        {
            try
            {
                var interview = await _context.ScheduledInterviews
                    .Include(i => i.Candidate).ThenInclude(c => c.User)
                    .Include(i => i.Interviewer).ThenInclude(i => i.User)
                    .FirstOrDefaultAsync(i => i.InterviewId == interviewId);

                if (interview == null)
                    return false;

                interview.Status = InterviewStatus.Cancelled;
                interview.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Send cancellation emails
                await _emailService.SendInterviewCancelledEmailAsync(
                    interview.Candidate.User.Email,
                    interview.Candidate.User.Name,
                    interview.Interviewer.User.Name,
                    interview.InterviewDate,
                    interview.StartTime,
                    reason);

                await _emailService.SendInterviewCancelledEmailAsync(
                    interview.Interviewer.User.Email,
                    interview.Interviewer.User.Name,
                    interview.Candidate.User.Name,
                    interview.InterviewDate,
                    interview.StartTime,
                    reason);

                await _logService.LogActionAsync(
                    "Interview Cancelled",
                    $"Interview {interviewId} cancelled. Reason: {reason}",
                    AppLogLevel.Info);

                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogActionAsync(
                    "Cancel Interview Error",
                    $"Error cancelling interview: {ex.Message}",
                    AppLogLevel.Error);
                return false;
            }
        }

        #endregion

        #region Conflict Detection

        public async Task<List<ConflictDto>> DetectConflictsAsync(int interviewId)
        {
            var interview = await _context.ScheduledInterviews
                .FirstOrDefaultAsync(i => i.InterviewId == interviewId);

            if (interview == null)
                return new List<ConflictDto>();

            var conflictReasons = await DetectPotentialConflictsAsync(
                interview.CandidateId,
                interview.InterviewerId,
                interview.InterviewDate,
                interview.StartTime,
                interview.EndTime,
                interviewId);

            var conflicts = new List<ConflictDto>();

            foreach (var reason in conflictReasons)
            {
                var conflict = new Conflict
                {
                    InterviewId = interviewId,
                    ConflictReason = reason,
                    DetectedAt = DateTime.UtcNow,
                    IsResolved = false
                };

                _context.Conflicts.Add(conflict);
                await _context.SaveChangesAsync();

                conflicts.Add(new ConflictDto
                {
                    ConflictId = conflict.ConflictId,
                    InterviewId = conflict.InterviewId,
                    ConflictReason = conflict.ConflictReason,
                    DetectedAt = conflict.DetectedAt,
                    IsResolved = conflict.IsResolved
                });
            }

            return conflicts;
        }

        public async Task<List<ConflictDto>> GetAllConflictsAsync()
        {
            return await _context.Conflicts
                .Select(c => new ConflictDto
                {
                    ConflictId = c.ConflictId,
                    InterviewId = c.InterviewId,
                    ConflictReason = c.ConflictReason,
                    DetectedAt = c.DetectedAt,
                    IsResolved = c.IsResolved,
                    ResolvedAt = c.ResolvedAt,
                    ResolutionNotes = c.ResolutionNotes
                })
                .ToListAsync();
        }

        public async Task<List<ConflictDto>> GetUnresolvedConflictsAsync()
        {
            return await _context.Conflicts
                .Where(c => !c.IsResolved)
                .Select(c => new ConflictDto
                {
                    ConflictId = c.ConflictId,
                    InterviewId = c.InterviewId,
                    ConflictReason = c.ConflictReason,
                    DetectedAt = c.DetectedAt,
                    IsResolved = c.IsResolved
                })
                .ToListAsync();
        }

        public async Task<bool> ResolveConflictAsync(ResolveConflictDto request)
        {
            try
            {
                var conflict = await _context.Conflicts
                    .FirstOrDefaultAsync(c => c.ConflictId == request.ConflictId);

                if (conflict == null)
                    return false;

                conflict.IsResolved = true;
                conflict.ResolvedAt = DateTime.UtcNow;
                conflict.ResolutionNotes = request.ResolutionNotes;

                await _context.SaveChangesAsync();

                await _logService.LogActionAsync(
                    "Conflict Resolved",
                    $"Conflict {request.ConflictId} resolved: {request.ResolutionNotes}",
                    AppLogLevel.Info);

                return true;
            }
            catch (Exception ex)
            {
                await _logService.LogActionAsync(
                    "Resolve Conflict Error",
                    $"Error resolving conflict: {ex.Message}",
                    AppLogLevel.Error);
                return false;
            }
        }

        #endregion

        #region Availability Management

        public async Task<List<AvailabilityDto>> GetAvailableSlotsAsync(
            int interviewerId,
            DateTime startDate,
            DateTime endDate)
        {
            var availabilities = await _context.Availabilities
                .Include(a => a.Interviewer).ThenInclude(i => i.User)
                .Where(a => a.InterviewerId == interviewerId &&
                           a.AvailableDate >= startDate &&
                           a.AvailableDate <= endDate)
                .ToListAsync();

            // Get scheduled interviews for this interviewer in the date range
            var scheduledInterviews = await _context.ScheduledInterviews
                .Where(i => i.InterviewerId == interviewerId &&
                           i.InterviewDate >= startDate &&
                           i.InterviewDate <= endDate &&
                           i.Status == InterviewStatus.Scheduled)
                .ToListAsync();

            var result = new List<AvailabilityDto>();

            foreach (var availability in availabilities)
            {
                // Check if this slot is already booked
                var isBooked = scheduledInterviews.Any(si =>
                    si.InterviewDate == availability.AvailableDate &&
                    si.StartTime < availability.EndTime &&
                    si.EndTime > availability.StartTime);

                result.Add(new AvailabilityDto
                {
                    AvailabilityId = availability.AvailabilityId,
                    InterviewerId = availability.InterviewerId,
                    InterviewerName = availability.Interviewer.User.Name,
                    AvailableDate = availability.AvailableDate,
                    StartTime = availability.StartTime,
                    EndTime = availability.EndTime,
                    IsBooked = isBooked
                });
            }

            return result.OrderBy(a => a.AvailableDate).ThenBy(a => a.StartTime).ToList();
        }

        public async Task<List<AvailabilityDto>> FindBestAvailableSlotsAsync(
            int interviewerId,
            DateTime startDate,
            DateTime endDate,
            int durationMinutes)
        {
            var allSlots = await GetAvailableSlotsAsync(interviewerId, startDate, endDate);
            var availableSlots = allSlots.Where(s => !s.IsBooked).ToList();

            var result = new List<AvailabilityDto>();

            foreach (var slot in availableSlots)
            {
                var slotDuration = (slot.EndTime - slot.StartTime).TotalMinutes;

                // Check if the slot is large enough
                if (slotDuration >= durationMinutes)
                {
                    result.Add(slot);
                }
            }

            return result.OrderBy(s => s.AvailableDate).ThenBy(s => s.StartTime).ToList();
        }

        #endregion

        #region Interview Queries

        public async Task<List<InterviewDto>> GetInterviewsByDateRangeAsync(
            DateTime startDate,
            DateTime endDate)
        {
            var interviews = await _context.ScheduledInterviews
                .Include(i => i.Candidate).ThenInclude(c => c.User)
                .Include(i => i.Interviewer).ThenInclude(i => i.User)
                .Where(i => i.InterviewDate >= startDate && i.InterviewDate <= endDate)
                .ToListAsync();

            return interviews.Select(i => MapToInterviewDto(i)).ToList();
        }

        public async Task<List<InterviewDto>> GetInterviewsByCandidateAsync(int candidateId)
        {
            var interviews = await _context.ScheduledInterviews
                .Include(i => i.Candidate).ThenInclude(c => c.User)
                .Include(i => i.Interviewer).ThenInclude(i => i.User)
                .Where(i => i.CandidateId == candidateId)
                .ToListAsync();

            return interviews.Select(i => MapToInterviewDto(i)).ToList();
        }

        public async Task<List<InterviewDto>> GetInterviewsByInterviewerAsync(int interviewerId)
        {
            var interviews = await _context.ScheduledInterviews
                .Include(i => i.Candidate).ThenInclude(c => c.User)
                .Include(i => i.Interviewer).ThenInclude(i => i.User)
                .Where(i => i.InterviewerId == interviewerId)
                .ToListAsync();

            return interviews.Select(i => MapToInterviewDto(i)).ToList();
        }

        public async Task<InterviewDto> GetInterviewByIdAsync(int interviewId)
        {
            var interview = await _context.ScheduledInterviews
                .Include(i => i.Candidate).ThenInclude(c => c.User)
                .Include(i => i.Interviewer).ThenInclude(i => i.User)
                .FirstOrDefaultAsync(i => i.InterviewId == interviewId);

            return interview == null ? null : MapToInterviewDto(interview);
        }

        #endregion

        #region Private Helper Methods

        private async Task<bool> CheckInterviewerAvailabilityAsync(
            int interviewerId,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            var availability = await _context.Availabilities
                .FirstOrDefaultAsync(a =>
                    a.InterviewerId == interviewerId &&
                    a.AvailableDate == date &&
                    a.StartTime <= startTime &&
                    a.EndTime >= endTime);

            // Around line 622 — after the FirstOrDefaultAsync call
// ADD this check before the null return:   
                if (availability == null)
{
    // If interviewer has NO availability records at all, allow scheduling
    var hasAnyAvailability = await _context.Availabilities
        .AnyAsync(a => a.InterviewerId == interviewerId);
    if (!hasAnyAvailability) return true;   // ← ADD THIS
    return false;
}
            
                return false;

            // Check if there's already a scheduled interview at this time
            var existingInterview = await _context.ScheduledInterviews
                .AnyAsync(i =>
                    i.InterviewerId == interviewerId &&
                    i.InterviewDate == date &&
                    i.Status == InterviewStatus.Scheduled &&
                    i.StartTime < endTime &&
                    i.EndTime > startTime);

            return !existingInterview;
        }

        private async Task<List<string>> DetectPotentialConflictsAsync(
            int candidateId,
            int interviewerId,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime,
            int? excludeInterviewId = null)
        {
            var conflicts = new List<string>();

            // Check interviewer conflicts
            var interviewerConflicts = await _context.ScheduledInterviews
                .Where(i => i.InterviewerId == interviewerId &&
                           i.InterviewDate == date &&
                           i.Status == InterviewStatus.Scheduled &&
                           i.StartTime < endTime &&
                           i.EndTime > startTime &&
                           (!excludeInterviewId.HasValue || i.InterviewId != excludeInterviewId.Value))
                .ToListAsync();

            if (interviewerConflicts.Any())
            {
                conflicts.Add($"Interviewer has {interviewerConflicts.Count} conflicting interview(s) at this time");
            }

            // Check candidate conflicts
            var candidateConflicts = await _context.ScheduledInterviews
                .Where(i => i.CandidateId == candidateId &&
                           i.InterviewDate == date &&
                           i.Status == InterviewStatus.Scheduled &&
                           i.StartTime < endTime &&
                           i.EndTime > startTime &&
                           (!excludeInterviewId.HasValue || i.InterviewId != excludeInterviewId.Value))
                .ToListAsync();

            if (candidateConflicts.Any())
            {
                conflicts.Add($"Candidate has {candidateConflicts.Count} conflicting interview(s) at this time");
            }

            return conflicts;
        }

        private int CalculateSlotScore(AvailabilityDto slot, string preferredTimeOfDay)
        {
            int score = 100; // Base score

            // Prefer closer dates
            var daysFromNow = (slot.AvailableDate - DateTime.Today).Days;
            score -= Math.Min(daysFromNow, 30); // Max 30 point deduction

            // Prefer specific time of day if specified
            if (!string.IsNullOrEmpty(preferredTimeOfDay))
            {
                var hour = slot.StartTime.Hours;

                switch (preferredTimeOfDay.ToLower())
                {
                    case "morning":
                        if (hour >= 8 && hour < 12)
                            score += 20;
                        break;
                    case "afternoon":
                        if (hour >= 12 && hour < 17)
                            score += 20;
                        break;
                    case "evening":
                        if (hour >= 17 && hour < 20)
                            score += 20;
                        break;
                }
            }

            // Prefer weekdays over weekends
            if (slot.AvailableDate.DayOfWeek != DayOfWeek.Saturday &&
                slot.AvailableDate.DayOfWeek != DayOfWeek.Sunday)
            {
                score += 10;
            }

            return score;
        }

        private InterviewDto MapToInterviewDto(ScheduledInterview interview)
        {
            return new InterviewDto
            {
                InterviewId = interview.InterviewId,
                CandidateId = interview.CandidateId,
                CandidateName = interview.Candidate?.User?.Name,
                CandidateEmail = interview.Candidate?.User?.Email,
                InterviewerId = interview.InterviewerId,
                InterviewerName = interview.Interviewer?.User?.Name,
                InterviewerEmail = interview.Interviewer?.User?.Email,
                InterviewDate = interview.InterviewDate,
                StartTime = interview.StartTime,
                EndTime = interview.EndTime,
                Status = interview.Status,
                CreatedAt = interview.CreatedAt
            };
        }

        private async Task<InterviewDto> MapToInterviewDtoAsync(ScheduledInterview interview)
        {
            if (interview.Candidate == null || interview.Interviewer == null)
            {
                interview = await _context.ScheduledInterviews
                    .Include(i => i.Candidate).ThenInclude(c => c.User)
                    .Include(i => i.Interviewer).ThenInclude(i => i.User)
                    .FirstOrDefaultAsync(i => i.InterviewId == interview.InterviewId);
            }

            return MapToInterviewDto(interview);
        }

        #endregion
    }
}

