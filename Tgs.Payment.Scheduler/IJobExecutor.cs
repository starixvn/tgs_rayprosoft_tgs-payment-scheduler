using System.ComponentModel;

namespace Tgs.Payment.Scheduler
{
	public interface IJobExecutor
	{
		[DisplayName("{0}")]
		public void Execute(string subject, Guid formId, DateTimeOffset expireTime);
	}
}