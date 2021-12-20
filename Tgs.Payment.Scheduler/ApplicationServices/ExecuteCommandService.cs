using Tgs.Payment.Scheduler.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Scheduler.ApplicationServices
{
	public class ExecuteCommandService : ICommandService<ExecuteCommand>
	{
		private readonly IScheduleQueueSender _sender;

		public ExecuteCommandService(IScheduleQueueSender sender)
		{
			_sender = sender ?? throw new ArgumentNullException(nameof(sender));
		}

		public ValueTask ExecuteAsync(ExecuteCommand command, CancellationToken cancellationToken = default)
			=> _sender.SendScheduleResultAsync(command.Subject, command.FormId, command.ExpireTime);
	}
}