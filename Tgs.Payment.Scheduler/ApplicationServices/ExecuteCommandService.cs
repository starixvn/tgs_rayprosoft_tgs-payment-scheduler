using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.Scheduler.ApplicationServices;

public class ExecuteCommandService(
	IMessageSender sender,
	ILogger<ExecuteCommandService> logger) : ICommandService<ExecuteCommand>
{
	public ValueTask ExecuteAsync(ExecuteCommand command, CancellationToken cancellationToken = default)
	{
		var request = new ScheduleRequest
		{
			Subject = command.Subject,
			FormId = command.FormId,
			ScheduleTime = Timestamp.FromDateTime(command.ExpireTime)
		};

		logger.Log(LogLevel.Information,
			new EventId(1000, nameof(ExecuteCommandService)),
			new
			{
				command.Subject,
				command.FormId,
				command.ExpireTime
			},
			null,
			(state, ex) => JsonSerializer.Serialize(state));

		return sender.PublishAsync(command.Subject, request.ToByteArray(), cancellationToken: cancellationToken);
	}
}