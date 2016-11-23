using System;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.Xunit2;
using NSubstitute;
using Xunit;

namespace AkkaTestError.Xunit
{
    public class ManagerTests : TestKit
    {
        private readonly TestProbe process;
        private readonly IActorRef manager;
        private readonly Guid processId;
      

        public ManagerTests() 
        {
            IProcessFactory factory = Substitute.For<IProcessFactory>();

            process = CreateTestProbe();

            processId = Guid.NewGuid();

            factory.Create(Arg.Any<IActorRefFactory>(), Arg.Any<SupervisorStrategy>()).Returns(process);

            manager = Sys.ActorOf(Props.Create(() => new Manager(factory)));
        }

        [Fact]
        public void GivenStartProcessCommand_WhenNewProcess_ShouldForwardCommand()
        {
            manager.Tell(new StartProcessCommand(processId));

            process.ExpectMsg<StartProcessCommand>();
        }

        [Fact]
        public void GivenNewProcess_WhenStartProcessCommand_ShouldLogStartCreatingProcess()
        {
            EventFilter.Info($"Creating process with Id: {processId}.")
                .ExpectOne(() => manager.Tell(new StartProcessCommand(processId)));
        }

        [Fact]
        public void GivenNewProcess_WhenStartProcessCommand_ShouldLogFinnishCreatingProcess()
        {
            EventFilter.Info($"Created process with Id: {processId}.")
                .ExpectOne(() => manager.Tell(new StartProcessCommand(processId)));
        }

        [Fact]
        public void GivenProcessExists_WhenStartProcessCommand_ShouldLogError()
        {
            manager.Tell(new StartProcessCommand(processId));

            EventFilter.Error($"Process exists with Id: {processId}.")
                .ExpectOne(() => manager.Tell(new StartProcessCommand(processId)));
        }

        [Fact]
        public void GivenProcessExists_WhenDomainEvent_ShouldLogDelegatingToProcess()
        {
            var evnt = Substitute.For<DomainEvent>(processId);

            manager.Tell(new StartProcessCommand(processId));

            EventFilter.Info($"Delegating event to process with Id: {processId}.")
                .ExpectOne(() => manager.Tell(evnt));
        }

        [Fact]
        public void GivenProcessExists_WhenDomainEvent_ShouldDelegateEventToProcess()
        {
            var evnt = Substitute.For<DomainEvent>(processId);

            process.IgnoreAllMessagesBut<DomainEvent>();

            manager.Tell(new StartProcessCommand(processId));
            manager.Tell(evnt);

            process.ExpectMsg<DomainEvent>();
        }


        [Fact]
        public void GivenProcessExists_WhenPublishDomainEvent_ShouldDelegateEventToProcess()
        {
            var evnt = Substitute.For<DomainEvent>(processId);

            process.IgnoreAllMessagesBut<DomainEvent>();

            manager.Tell(new StartProcessCommand(processId));

            Sys.EventStream.Publish(evnt);

            process.ExpectMsg<DomainEvent>();
        }

        [Fact]
        public void GivenProcessDoesNotExist_WhenDomainEvent_ShouldLogError()
        {
            var evnt = Substitute.For<DomainEvent>(processId);

            EventFilter.Error($"Could not delagate event to process with Id: {processId}.")
                .ExpectOne(() => manager.Tell(evnt));
        }


        [Fact]
        public void GivenAnyTime_WhenProcessTerminates_ShouldLogStartRemovingProcess()
        {
            manager.Tell(new StartProcessCommand(processId));
            
            EventFilter.Info("Removing process.")
                .ExpectOne(() => Sys.Stop(process));
        }


        [Fact]
        public void GivenProcessExist_WhenProcessTerminates_ShouldLogRemovingProcess()
        {
            manager.Tell(new StartProcessCommand(processId));

            EventFilter.Info($"Removing process with Id: {processId}.")
                .ExpectOne(() => Sys.Stop(process));
        }


    }
}