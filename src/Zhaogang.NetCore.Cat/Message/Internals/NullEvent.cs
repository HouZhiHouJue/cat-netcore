namespace Zhaogang.NetCore.Cat.Message.Internals
{
    public class NullEvent : AbstractMessage, IEvent
    {
        public NullEvent() : base(null, null)
        {
        }
    }
}