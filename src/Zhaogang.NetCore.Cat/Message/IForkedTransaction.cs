using System;
using System.Collections.Generic;
using System.Text;

namespace Zhaogang.NetCore.Cat.Message
{
    public interface IForkedTransaction : ITransaction
    {
        void Fork();

        String ForkedMessageId {get;}
    }
}
