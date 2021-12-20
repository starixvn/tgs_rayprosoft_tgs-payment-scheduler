namespace Tgs.Payment.Scheduler.MessageQueue
{
	public interface IScheduleQueueSender
	{
		ValueTask SendScheduleResultAsync(string subject, Guid formId, DateTimeOffset expireTime);
	}
}