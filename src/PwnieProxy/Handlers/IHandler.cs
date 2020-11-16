namespace PwnieProxy.Handlers
{
    public interface IHandler
    {
        InterceptionStream? Other { get; set; }
        void QueueForOther(byte[] data);
        byte[] Handle(byte[] data);
    }
}