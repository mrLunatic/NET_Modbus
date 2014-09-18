using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus;
using Modbus.Common;
using Modbus.RTU;


namespace Server_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var Server = new RTU_Server("COM9", 19200, 1);
            Server.AddHoldingRegister(1);
            Console.ReadLine();
            Server.Stop();

        }
    }
}
