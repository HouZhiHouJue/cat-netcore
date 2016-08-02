namespace Zhaogang.NetCore.Cat.Message.Spi.IO
{
    public interface IMessageSender
    {
        bool HasSendingMessage { get; }

        void Initialize();

        void Send(IMessageTree tree);

        void Shutdown();
    }
}