using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zhaogang.NetCore.Cat.Message.Internals
{
    class NullMetric : AbstractMessage, IMetric
    {
        public NullMetric() : base(null, null)
        {
        }
    }
}
