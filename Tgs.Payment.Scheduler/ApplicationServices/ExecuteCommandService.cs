using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Scheduler.ApplicationServices
{
	public class ExecuteCommandService : ICommandService<ExecuteCommand>
	{
		private readonly IMessageSender _sender;
		private readonly ILogger<ExecuteCommandService> _logger;

		public ExecuteCommandService(
			IMessageSender sender,
			ILogger<ExecuteCommandService> logger)
		{
			_sender = sender ?? throw new ArgumentNullException(nameof(sender));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public ValueTask ExecuteAsync(ExecuteCommand command, CancellationToken cancellationToken = default)
		{
			var request = new ScheduleRequest
			{
				Subject = command.Subject,
				FormId = command.FormId,
				ScheduleTime = Timestamp.FromDateTime(command.ExpireTime)
			};

			_logger.Log(LogLevel.Information,
				new EventId(1000, nameof(ExecuteCommandService)),
				new
				{
					command.Subject,
					command.FormId,
					command.ExpireTime
				},
				null,
				(state, ex) => JsonSerializer.Serialize(state));

			return _sender.PublishAsync(command.Subject, request.ToByteArray());
		}
	}
}