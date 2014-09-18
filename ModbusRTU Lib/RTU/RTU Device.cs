using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus.RTU
{
    public class RTU_Device : IModbusDevice
    {
        public int Address { get; private set; }
        public bool Broadcast { get; private set; }

        public RTU_Device(int Address)
        {
            if ((Address > 247) || (Address < 0))
                throw new ArgumentOutOfRangeException("Device Address", "Device Address must be in rande 0 - 247");

            this.Address = Address;
            this.Broadcast = (Address == 0);
        }
    }
}
