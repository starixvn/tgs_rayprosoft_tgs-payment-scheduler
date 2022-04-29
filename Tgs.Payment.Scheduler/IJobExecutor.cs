using System.ComponentModel;

namespace Tgs.Payment.Scheduler;

public interface IJobExecutor
{
	[DisplayName("{0}")]
	public Task ExecuteAsync(string subject, Guid formId, DateTime expireTime);
}
