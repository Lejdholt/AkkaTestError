using Akka.Actor;

namespace AkkaTestError
{
    public interface IProcessFactory
    {
        IActorRef Create(IActorRefFactory context, SupervisorStrategy strategy);
    }
}