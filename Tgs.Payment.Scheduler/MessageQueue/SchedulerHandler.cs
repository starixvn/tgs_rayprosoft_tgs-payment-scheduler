using Tgs.Payment.Protos;
using Tgs.Payment.Scheduler.Services;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.Payment.Scheduler.MessageQueue;

public class SchedulerHandler(
	ICommandService<ScheduleCommand> commandService,
	ILogger<SchedulerHandler> logger) : IMessageHandler<ReadOnlyMemory<byte>>
{
	public async ValueTask HandleAsync(
		string subject,
		ReadOnlyMemory<byte> data,
		IEnumerable<MessageHeaderValue>? headerValues,
		CancellationToken cancellationToken = default)
	{
		var request = ScheduleRequest.Parser.ParseFrom(data.Span);

		logger.LogInformation(
			"Scheduler Handler Request Subject:{request.Subject}, " +
				"Form Id:{request.FormId.ToGuid()}, " +
				"Schedule Time:{request.ScheduleTime.ToDateTime():yyyy/MM/dd HH:mm:ss}",
			request.Subject,
			request.FormId.ToGuid(),
			request.ScheduleTime.ToDateTime());

		var command = new ScheduleCommand(request.Subject, request.FormId, request.ScheduleTime.ToDateTime());

		await commandService.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
	}
}