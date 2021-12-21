using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NUnit.Framework;
using Tgs.MessageQueue;
using Tgs.Payment.Protos;
using Tgs.Payment.Scheduler.MessageQueue;
using Tgs.Payment.Scheduler.Services;
using Tgs.ServiceUtility;

namespace Tgs.Payment.Scheduler.UnitTests
{
	[TestFixture]
	public class SchedulerUnitTests
	{
		[Test]
		public async Task Test_Create_Background_Job_Should_Success()
		{
			//arrange
			var client = Substitute.For<IBackgroundJobClient>();
			var sut = new ScheduleCommandService(client);

			//act
			var command = new ScheduleCommand(
				"tgs.payment.umpay",
				Guid.NewGuid(),
				DateTime.Now.AddSeconds(20));

			await sut.ExecuteAsync(command);

			//assert
			_ = client
				.Received(1)
				.Create(Arg.Any<Job>(), Arg.Any<IState>());
		}

		[Test]
		public async Task Test_Receive_ScheduleRequest_From_Third_Party_Pay()
		{
			//arrange
			var fakeCommandService = Substitute.For<ICommandService<ScheduleCommand>>();
			var fakeLogger = new NullLogger<SchedulerHandler>();
			var sut = new SchedulerHandler(fakeCommandService, fakeLogger);

			//act
			var request = new ScheduleRequest
			{
				Subject = "tgs.payment.umpay",
				FormId = Guid.NewGuid(),
				ScheduleTime = Timestamp.FromDateTime(DateTime.Now.AddSeconds(30))
			};

			await sut
				.HandleAsync(request.ToByteArray())
				.ConfigureAwait(false);

			//assert
			_ = fakeCommandService
				.Received(1)
				.ExecuteAsync(Arg.Any<ScheduleCommand>());
		}

		[Test]
		public async Task Test_Emit_Schedule_To_Third_Party_Pay()
		{
			//arrange
			var fakeMessageSender = Substitute.For<IMessageSender>();

			var fakeLogger = new NullLogger<EmitScheduleCommandService>();

			var sut = new EmitScheduleCommandService(fakeMessageSender, fakeLogger);

			var command = new EmitScheduleCommand(
				"tgs.payment.umpay",
				Guid.NewGuid(),
				DateTime.Now.AddSeconds(30));

			//act
			await sut
				.ExecuteAsync(command)
				.ConfigureAwait(false);

			//assert
			_ = fakeMessageSender
				.Received(1)
				.SendAsync(Arg.Any<string>(), Arg.Any<byte[]>());
		}
	}
}