namespace Tgs.Payment.Scheduler.Services;

public record ScheduleCommand(string Subject, Guid FormId, DateTime ScheduleTime);
