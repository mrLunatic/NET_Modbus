using System;
using System.Collections.Generic;

using System.IO;
using System.IO.Ports;
using System.Threading;

using Modbus;
using Modbus.Common;
using Modbus.RTU;

namespace Modbus.RTU
{
    public class RTU_Server: BaseModbusServer, IModbusDevice
    {
        SerialPort Port;
        Thread Listener;
        List<byte> Buffer;

        public bool Broadcast { get { return false; } }
        public int Address { get; private set; }
        public int ByteTimeout { get; set; }


        public RTU_Server(string PortName, int BaudRate = 9600, int Address = 1, Parity Parity = Parity.None, int DataBits = 8, StopBits StopBits = StopBits.One, int Timeout = 2500)
        {
            Port = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
            this.Address = Address;
            Listener = new Thread(this.Listen);

            Port.Open();

            Listener.Start();
        }

        public override void Stop()
        {
            Port.Close();
            Port.Dispose();
            Listener.Join();
        }
        void Listen()
        {
            var Device = new RTU_Device(Address);

            
            Buffer = new List<byte>();

            try
            {
                while (true)
                {
                    var ADU = new RTU_ADU(Device);

                    Port.DiscardInBuffer();
                    Buffer.Clear();

                    #region Recieve first byte
                    Port.ReadTimeout = SerialPort.InfiniteTimeout;

                    Buffer.Add((byte)Port.ReadByte());
                    //ByteRecievedEvent(Buffer.Last());

                    #endregion

                    #region Recieve All bytes
                    Port.ReadTimeout = ByteTimeout;


                    try
                    {
                        while(true)
                            Buffer.Add((byte)Port.ReadByte());
                            //ByteRecievedEvent(Buffer.Last());
                        
                    }
                    catch (TimeoutException) { }
                    #endregion

                    #region Handle Req
                    if ((ADU.CheckRequest(Buffer.ToArray())) == Status.OK)
                    {
                        ADU.PDU.Handle(this);
                        var Response = ADU.BuildResponse();
                        Port.Write(Response, 0, Response.Length);
                    }
                    #endregion

                }
            }
            catch (IOException)
            { }

          }
        

    }
}
