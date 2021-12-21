using Tgs.MessageQueue;
using Tgs.Payment.Protos;
using Tgs.Payment.Scheduler.Services;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Scheduler.MessageQueue
{
	public class SchedulerHandler : IMessageHandler
	{
		private readonly ICommandService<ScheduleCommand> _commandService;
		private readonly ILogger<SchedulerHandler> _logger;

		public SchedulerHandler(
			ICommandService<ScheduleCommand> commandService,
			ILogger<SchedulerHandler> logger)
		{
			_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async ValueTask HandleAsync(byte[] data, CancellationToken cancellationToken = default)
		{
			var request = ScheduleRequest.Parser.ParseFrom(data);

			_logger.LogInformation(
				$"Scheduler Handler Request Subject:{request.Subject}, Form Id:{request.FormId.ToGuid()}, Schedule Time:{request.ScheduleTime.ToDateTime():yyyy/MM/dd HH:mm:ss}");

			var command = new ScheduleCommand(request.Subject, request.FormId, request.ScheduleTime.ToDateTime());

			await _commandService.ExecuteAsync(command).ConfigureAwait(false);
		}
	}
}