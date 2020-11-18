namespace PwnieProxy.Handlers
{
    public interface IHandler
    {
        InterceptionStream? Other { get; set; }
        public void QueueForOther(byte[] data)
        {
            Other?.QueueMessage(data);
        }
        byte[] Handle(byte[] data);
    }
}