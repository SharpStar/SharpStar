using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpStar.Lib.Networking
{
    public interface IWriteable
    {
        void WriteTo(IStarboundStream stream);
    }
}
