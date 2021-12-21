using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Scheduler.Services
{
	public class EmitScheduleCommandService : ICommandService<EmitScheduleCommand>
	{
		private readonly IMessageSender _messageSender;
		private readonly ILogger<EmitScheduleCommandService> _logger;

		public EmitScheduleCommandService(
			IMessageSender messageSender,
			ILogger<EmitScheduleCommandService> logger)
		{
			_messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public ValueTask ExecuteAsync(EmitScheduleCommand command, CancellationToken cancellationToken = default)
		{
			var request = new ScheduleRequest
			{
				Subject = command.Subject,
				FormId = command.FormId,
				ScheduleTime = Timestamp.FromDateTime(command.ExpireTime)
			};

			_logger.LogInformation(
				$"Emit Schedule Result To Subject:{command.Subject}, FormId:{command.FormId}, ExpireTime:{command.ExpireTime:yyyy/MM/dd HH:mm:ss}");

			return _messageSender.SendAsync(command.Subject, request.ToByteArray());
		}
	}
}