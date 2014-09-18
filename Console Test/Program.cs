using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Modbus;
using Modbus.RTU;
using Modbus.Common;


namespace Console_Test
{
    class Program
    {

        class myDevice :IDisposable
        {
            RTU_Client Client;
            RTU_Device Device;
            Timer Timer;
            int Value;

            public myDevice()
            {
                Client = new RTU_Client("COM10", 19200);
                Client.MaxCount = 10;
                Device = new RTU_Device(1);

                Value = 0;
                Timer = new Timer(25);
                Timer.Elapsed += Timer_Elapsed;
                Timer.Enabled = true;
            }

            public void WriteRandom()
            {
                var Gen = new Random();
                try
                {
                    Client.WriteMultipleRegisters(Device, 1, new int[] { (3<<Value) });
                    Console.WriteLine("Query OK!");
                }
                catch (ModbusException ex)
                {
                    Console.WriteLine(ex.Message);
                }

                if (Value < 15)
                    Value++;

                else Value = 0;
            }

            void Timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                WriteRandom();
            }

            public void Dispose()
            {
                Timer.Enabled = false;
                Timer.Dispose();
                Client.Dispose();
            }

        }



        static void Main(string[] args)
        {
            var Server = new RTU_Server("COM9", 19200, 1);
            Server.AddHoldingRegister(1);


            var Dev = new myDevice();
            Console.ReadLine();
            Dev.Dispose();

            Console.ReadLine();
            Server.Stop();
        }


    }
}
