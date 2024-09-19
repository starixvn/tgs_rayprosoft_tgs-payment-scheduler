using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
	.AddKeyedSingleton("Nats",
	(sp, _) => new NatsConnection(NatsOpts.Default with
	{
		Url = "nats://localhost:4222"
	}))
	.AddPhoenixToolkitsMessageQueue()
	.AddNatsMessageQueue(config => config
		.ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats")))
	.AddNatsGlobPatternExchange("*");

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
		ScheduleTime = Timestamp.FromDateTime(DateTime.UtcNow.AddSeconds(10))
	};

	logger.LogInformation("Send to Scheduler");
	await sender.SendAsync("tgs.payment.countdown.schedule", request.ToByteArray());
}
finally
{
	await app.StopAsync();
}
