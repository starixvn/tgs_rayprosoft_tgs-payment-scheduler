using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services => services
	.AddMessageQueue()
	.AddNatsMessageQueue(config => config
		.ConfigQueueOptions((options, ap) =>
		{
			options.Url = "nats://localhost:4222";
		})));

var app = builder.Build();

try
{
	await app.StartAsync();
	var logger = app.Services.GetRequiredService<ILogger<Program>>();
	var sender = app.Services.GetRequiredService<IMessageSender>();

	var request = new ScheduleRequest
	{
		Subject = "tgs.payment.tgs",
		FormId = Guid.NewGuid(),
		ScheduleTime = Timestamp.FromDateTime(DateTime.Now.AddSeconds(10))
	};

	logger.LogInformation("Send to Scheduler");
	await sender.SendAsync("tgs.payment.countdown.schedule", request.ToByteArray());
}
finally
{
	await app.StopAsync();
}
