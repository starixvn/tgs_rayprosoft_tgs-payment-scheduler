using Hangfire.Dashboard;

namespace Tgs.Payment.Scheduler
{
	public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
	{
		public bool Authorize(DashboardContext context) => true;
	}
}