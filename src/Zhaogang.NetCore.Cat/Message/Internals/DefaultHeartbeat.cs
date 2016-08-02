using System;

namespace Zhaogang.NetCore.Cat.Message.Internals
{
    public class DefaultHeartbeat : AbstractMessage, IHeartbeat
    {
        public DefaultHeartbeat(String type, String name) : base(type, name)
        {
        }
    }
}