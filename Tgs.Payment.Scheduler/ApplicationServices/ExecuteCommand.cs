namespace Tgs.Payment.Scheduler.ApplicationServices
{
	public record ExecuteCommand(string Subject, Guid FormId, DateTimeOffset ExpireTime);
}