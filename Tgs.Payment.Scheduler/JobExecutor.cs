using Tgs.Payment.Scheduler.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Scheduler
{
	public class JobExecutor : IJobExecutor
	{
		private readonly ICommandService<ExecuteCommand> _command;

		public JobExecutor(ICommandService<ExecuteCommand> command)
		{
			_command = command ?? throw new ArgumentNullException(nameof(command));
		}

		public void Execute(string subject, Guid formId, DateTime expireTime)
			=> _command.ExecuteAsync(new ExecuteCommand(subject, formId, expireTime));
	}
}