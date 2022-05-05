using Hangfire;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Scheduler.Services;

public class ScheduleCommandService : ICommandService<ScheduleCommand>
{
	private readonly IBackgroundJobClient _client;

	public ScheduleCommandService(IBackgroundJobClient client)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
	}

	public ValueTask ExecuteAsync(ScheduleCommand command, CancellationToken cancellationToken = default)
	{
		var timeSpan = command.ScheduleTime - DateTime.Now;

		_ = _client.Schedule<JobExecutor>(
			excutor => excutor.ExecuteAsync(command.Subject, command.FormId, command.ScheduleTime),
			timeSpan);

		return ValueTask.CompletedTask;
	}
}
