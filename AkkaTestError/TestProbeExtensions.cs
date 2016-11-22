using Akka.TestKit;

namespace AkkaTestError
{
    public static class TestProbeExtensions
    {
        public static void IgnoreAllMessagesBut<T>(this TestProbe probe)
        {
            probe.IgnoreMessages(o => !(o is T));
        }
    }
}
