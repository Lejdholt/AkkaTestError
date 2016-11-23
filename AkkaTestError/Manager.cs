using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;

namespace AkkaTestError
{
    public class Manager : ReceiveActor
    {
        private readonly IProcessFactory processFactory;

        public Manager(IProcessFactory processFactory)
        {
            this.processFactory = processFactory;

            Context.System.EventStream.Subscribe(Self, typeof(DomainEvent));
            logger = Context.GetLogger();
            Become(Ready);
        }

        private readonly Dictionary<Guid, IActorRef> processes = new Dictionary<Guid, IActorRef>();
        private ILoggingAdapter logger;

        private void Ready()
        {
            Receive<StartProcessCommand>(cmd =>
            {
                logger.Info($"Creating process with Id: {cmd.Id}.");
                if (processes.ContainsKey(cmd.Id))
                {
                    logger.Error($"Process exists with Id: {cmd.Id}.");
                    return;
                }

                var process = processFactory.Create(Context, Akka.Actor.SupervisorStrategy.StoppingStrategy);
                Context.Watch(process);
                processes.Add(cmd.Id, process);
                logger.Info($"Created process with Id: {cmd.Id}.");
                process.Forward(cmd);
            });

            Receive<DomainEvent>(evnt =>
            {
                if (!processes.ContainsKey(evnt.Id))
                {
                    logger.Error($"Could not delagate event to process with Id: {evnt.Id}.");
                    return;
                }
                logger.Info($"Delegating event to process with Id: {evnt.Id}.");
                processes[evnt.Id].Tell(evnt);
            });

            Receive<Terminated>(msg =>
            {
                logger.Info("Removing process.");

                Guid key = (from actorRef in processes
                    where Equals(actorRef.Value, msg.ActorRef)
                    select actorRef.Key).DefaultIfEmpty(Guid.Empty).FirstOrDefault();

                if (key == Guid.Empty)
                {
                    logger.Warning("Could not remove process.");
                    return;
                }

                logger.Info($"Removing process with Id: {key}.");
                processes.Remove(key);
            });
        }
    }
}