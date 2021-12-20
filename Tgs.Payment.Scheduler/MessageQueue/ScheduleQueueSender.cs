using System.Text;
using System.Text.Json;
using Tgs.MessageQueue;

namespace Tgs.Payment.Scheduler.MessageQueue
{
	public class ScheduleQueueSender : IScheduleQueueSender
	{
		private readonly IMessageSender _messageSender;
		private readonly ILogger<ScheduleQueueSender> _logger;

		public ScheduleQueueSender(
			IMessageSender messageSender,
			ILogger<ScheduleQueueSender> logger)
		{
			_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public ValueTask SendScheduleResultAsync(string subject,Guid formId, DateTimeOffset expireTime)
		{
			var body = new ScheduleMessage
			{
				Subject = subject,
				FormId = formId,
				ExpireTime = expireTime
			};

			_logger.Log(LogLevel.Information,
				new EventId(1000, nameof(ScheduleQueueSender)),
				new
				{
					formId
				},
				null,
				(state, ex) => JsonSerializer.Serialize(state));

			return _messageSender.PublishAsync(subject, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(body)));
		}
	}
}