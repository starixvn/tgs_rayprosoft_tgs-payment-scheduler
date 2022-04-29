using Hangfire;
using Hangfire.Console;
using Hangfire.Redis;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.Payment.Scheduler;
using Tgs.Payment.Scheduler.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.AddJsonFile("/config/appsettings.json", true);

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP");
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME");
			fluentOptions.UseMessagePack();
		});
	});

builder.Services.AddControllers();

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
	_ = config
		.UseRedisStorage(connectionString, options)
		.UseConsole();
});

builder.Services.AddHangfireServer();

builder.Services
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig => natsConfig
		.ConfigQueueOptions((options, sp) =>
		{
			var configuration = sp.GetRequiredService<IConfiguration>();

			options.Url = configuration.GetConnectionString("NatsMQ");
		})
		.AddReplyHandler<SchedulerHandler>("tgs.payment.countdown.schedule", "tgs.payment.countdown.schedule"));

builder.Services.AddPaymentClient();

builder.Services.AddScheduler();

builder.Services
	.AddHealthChecks()
	.AddRedis(builder.Configuration.GetConnectionString("RedisConfigurationString"), "Redis Default");

builder.Services
	.AddOptions<OtlpExporterOptions>()
	.Configure<IConfiguration>((options, conifg) =>
	{
		var nodeIp = conifg.GetValue<string>("NODE_IP");

		if (!string.IsNullOrWhiteSpace(nodeIp))
			options.Endpoint = new Uri($"http://{nodeIp}:4317");

		conifg.GetSection("Opentelemetry:Tracing").Bind(options);
	})
	.Services
	.AddOpenTelemetryTracing(builder => builder
		.SetResourceBuilder(ResourceBuilder.CreateDefault()
		.AddEnvironmentVariableDetector())
		.AddSource("Tgs.*")
		.AddAspNetCoreInstrumentation()
		.AddHttpClientInstrumentation()
		.AddProfileViewExporter()
		.AddOtlpExporter())
	.Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
		httpContext => !httpContext.Request.Path.HasValue
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/healthz")
			&& !httpContext.Request.Path.StartsWithSegments("/metrics")
			&& !new[] { "html", "ico", "png", "jpg" }.Any(
				ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));

var app = builder.Build();

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

app.MapProfileViewer();
app.MapHealthChecks("/healthz");

app.Run();
