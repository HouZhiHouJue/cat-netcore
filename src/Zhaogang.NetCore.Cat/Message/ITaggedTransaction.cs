using System;
using System.Collections.Generic;
using System.Text;

namespace Zhaogang.NetCore.Cat.Message
{
    public interface ITaggedTransaction : ITransaction
    {
        void Bind(String tag, String childMessageId, String title);

        string ParentMessageId {get;}

        string RootMessageId {get;}

        string Tag {get;}

        void Start();
    }
}
