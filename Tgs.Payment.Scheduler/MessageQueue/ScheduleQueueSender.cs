using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;

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

		public ValueTask SendScheduleResultAsync(string subject, Guid formId, DateTime scheduleTime)
		{
			var request = new ScheduleRequest
			{
				Subject = subject,
				FormId = formId,
				ScheduleTime = Timestamp.FromDateTime(scheduleTime)
			};

			_logger.Log(LogLevel.Information,
				new EventId(1000, nameof(ScheduleQueueSender)),
				new
				{
					subject,
					formId,
					scheduleTime
				},
				null,
				(state, ex) => JsonSerializer.Serialize(state));

			return _messageSender.PublishAsync(subject, request.ToByteArray());
		}
	}
}