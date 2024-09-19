using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Tgs.Payment.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.Scheduler.Services;

public class EmitScheduleCommandService(
	IMessageSender messageSender,
	ILogger<EmitScheduleCommandService> logger) : ICommandService<EmitScheduleCommand>
{
	public ValueTask ExecuteAsync(EmitScheduleCommand command, CancellationToken cancellationToken = default)
	{
		var request = new ScheduleRequest
		{
			Subject = command.Subject,
			FormId = command.FormId,
			ScheduleTime = Timestamp.FromDateTime(command.ExpireTime)
		};

		logger.LogInformation(
			"Emit Schedule Result To Subject:{command.Subject}, FormId:{command.FormId}, ExpireTime:{command.ExpireTime:yyyy/MM/dd HH:mm:ss}",
			command.Subject,
			command.FormId,
			command.ExpireTime);

		return messageSender.SendAsync(
			command.Subject,
			new ReadOnlyMemory<byte>(request.ToByteArray()),
			cancellationToken: cancellationToken);
	}
}