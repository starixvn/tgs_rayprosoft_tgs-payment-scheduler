namespace Tgs.Payment.Scheduler.Services
{
	public record EmitScheduleCommand(string Subject, Guid FormId, DateTimeOffset ExpireTime);
}