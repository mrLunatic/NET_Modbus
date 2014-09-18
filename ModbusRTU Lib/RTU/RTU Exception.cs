using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus.RTU
{
    public class RTU_Exception : Exception
    {
        public RTU_Exception() { }
        public RTU_Exception(string message) : base(message) { }
        public RTU_Exception(string message, Exception inner) : base(message, inner) { }
    }
}
