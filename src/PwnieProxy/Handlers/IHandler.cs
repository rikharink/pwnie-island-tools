namespace PwnieProxy.Handlers
{
    public interface IHandler
    {
        void QueueForOther(byte[] data);
        byte[] Handle(byte[] data);
    }
}