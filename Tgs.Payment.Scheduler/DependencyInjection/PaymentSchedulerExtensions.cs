using System.Reflection;
using Tgs.Payment.Scheduler.MessageQueue;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class PaymentSchedulerExtensions
	{
		public static IServiceCollection AddScheduler(this IServiceCollection services)
		{
			return services
				.RegisterAllServiceUtilityTypes(
					Assembly.GetExecutingAssembly(),
					ServiceLifetime.Transient);
		}
	}
}