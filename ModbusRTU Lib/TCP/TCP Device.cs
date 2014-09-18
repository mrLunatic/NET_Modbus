using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using Modbus;

namespace Modbus.TCP
{
    class TCP_Device :IModbusDevice
    {
        public int Address { get; private set; }
        public bool Broadcast { get; private set; }
        public Socket DeviceLink { get; private set; }

        public TCP_Device(string Link)
        {
            this.Address = 0;
            this.Broadcast = false;

            var HostEntry = Dns.GetHostEntry(Link);
            var Endpoint = new IPEndPoint(HostEntry.AddressList[0], 503);

            DeviceLink = new Socket(Endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public TCP_Device(string Link, int Address)
            : this(Link)
        {
            if ((Address > 247) || (Address < 0))
                throw new ArgumentOutOfRangeException("Device Address", "Device Address must be in rande 0 - 247");

            this.Address = Address;
            this.Broadcast = false;
        }
    }
}
