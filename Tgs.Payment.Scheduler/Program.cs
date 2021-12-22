using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using Hangfire;
using Hangfire.Console;
using Hangfire.Redis;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Tgs.Payment.Scheduler;
using Tgs.Payment.Scheduler.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Host.ConfigureAppConfiguration(builder =>
{
	builder.AddJsonFile("/config/appsettings.json", true);
})
.ConfigureLogging((_, builder) =>
{
	builder.AddTgsLog();
});

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tgs.Payment.Scheduler", Version = "v1" });
});

builder.Services.AddHangfire((sp, config) =>
{
	var systemConfig = sp.GetRequiredService<IConfiguration>();

	var connectionString = builder.Configuration.GetConnectionString("RedisConfigurationString");

	var options = new RedisStorageOptions
	{
		Prefix = "hangfire:payment:"
	};
	config.UseRedisStorage(connectionString, options);

	config.UseConsole();
});

builder.Services.AddHangfireServer();

builder.Services.AddNatsMessageQueue(natsConfig =>
{
	natsConfig
		.ConfigQueueOptions((options, sp) =>
		{
			var configuration = sp.GetRequiredService<IConfiguration>();

			options.Url = configuration.GetConnectionString("NatsMQ");
		})
		.AddReplyHandler<SchedulerHandler>("tgs.payment.countdown.schedule", "tgs.payment.countdown.schedule");
});

builder.Services.AddPaymentClient();

builder.Services.AddScheduler();

builder.Services
	.AddHealthChecks()
	.AddRedis(builder.Configuration.GetConnectionString("RedisConfigurationString"), "Redis Default");

var app = builder.Build();

app.MapHealthChecks("/health",
	new HealthCheckOptions
	{
		ResultStatusCodes = new Dictionary<HealthStatus, int>
		{
							{ HealthStatus.Healthy, StatusCodes.Status200OK },
							{ HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable },
							{ HealthStatus.Degraded, StatusCodes.Status500InternalServerError }
		},
		ResponseWriter = async (
			context,
			report) =>
		{
			var hostName = Dns.GetHostName();
			var hostEntry = await Dns.GetHostEntryAsync(hostName);
			var hostIp = Array.Find(hostEntry.AddressList,
				x => x.AddressFamily == AddressFamily.InterNetwork);
			var response = new
			{
				Data = new
				{
					Environment.MachineName,
					HostName = hostName,
					HostIp = hostIp?.ToString() ?? string.Empty,
					Environment = builder.Environment.EnvironmentName,
					OsVersion = $"{Environment.OSVersion}",
					Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
					ProcessCount = Environment.ProcessorCount
				},
				Result = new
				{
					Status = report.Status.ToString(),
					TotalDuration = report.TotalDuration.TotalSeconds,
					DependencyServices = report.Entries.Select(srv => new
					{
						srv.Key,
						Duration = srv.Value.Duration.TotalSeconds,
						srv.Value.Description,
						srv.Value.Data,
						Exception = srv.Value.Exception?.Message,
						Status = srv.Value.Status.ToString(),
						Tags = srv.Value.Tags.Any() ? string.Join(',', srv.Value.Tags) : string.Empty
					}).ToArray()
				}
			};
			var json = JsonSerializer.Serialize(response,
				new JsonSerializerOptions
				{
					WriteIndented = true
				});
			context.Response.ContentType = "application/json";
			await context.Response.WriteAsync(json);
		}
	});

app.UseHangfireDashboard(
	"/hangfire",
	new DashboardOptions()
	{
		Authorization =
		new[]
		{
			new IgnoreHangfireAuthorizationFilter()
		}
	});
app.MapSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tgs.Payment.Scheduler.Scheduler v1"));
app.Run();