using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

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
