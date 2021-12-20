using Hangfire;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Scheduler.Services
{
	public class ScheduleCommandService : ICommandService<ScheduleCommand>
	{
		private readonly IBackgroundJobClient _client;

		public ScheduleCommandService(IBackgroundJobClient client)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
		}

		public ValueTask ExecuteAsync(ScheduleCommand command, CancellationToken cancellationToken = default)
		{
			var timeSpan = command.ExpireTime - DateTimeOffset.Now;

			_ = _client
				.Schedule<JobExecutor>(
					excutor => excutor.Execute(command.Subject, command.FormId, command.ExpireTime),
					timeSpan);

			return ValueTask.CompletedTask;
		}
	}
}