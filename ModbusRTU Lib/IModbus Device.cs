using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus
{
    public interface IModbusDevice
    {
        int Address { get; }
        bool Broadcast { get; }
    }
}
