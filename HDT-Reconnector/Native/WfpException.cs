using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDT_Reconnector.Native
{
    internal class WfpException : Exception
    {
        public WfpException(string message) : base(message) { }
    }
}
