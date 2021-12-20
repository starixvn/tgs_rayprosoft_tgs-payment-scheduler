namespace Tgs.Payment.Scheduler.MessageQueue
{
	public class ScheduleMessage
	{
		public string Subject { get; set; } = default!;
		public Guid FormId { get; init; }
		public DateTimeOffset ExpireTime { get; init; }
	}
}