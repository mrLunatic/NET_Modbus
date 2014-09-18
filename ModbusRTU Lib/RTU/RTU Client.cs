using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Modbus;
using Modbus.Common;
using Modbus.RTU;



namespace Modbus.RTU
{
    public class RTU_Client : IModbusClient, IDisposable
    {
        SerialPort Port;
        public int ByteTimeout {get; set;}
        public int Timeout { get; set; }
        public int MaxCount { get; set; }

        List<byte> Buffer;

        public delegate void ReqSendHandler(byte[] Req);
        public event ReqSendHandler ReqSendEvent;
        public delegate void ByteRecievedHandler(byte Byte);
        public event ByteRecievedHandler ByteRecievedEvent;


        public RTU_Client(string PortName, int BaudRate = 9600, Parity Parity = Parity.None, int DataBits = 8, StopBits StopBits = StopBits.One, int Timeout = 2500)
        {
            try
            {
                Port = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
            }
            catch (IOException ex)
            {
                throw new RTU_Exception("Ошибка настроек коммуникационного порта. " + ex.Message, ex);
            }
            ByteTimeout = 20;
            MaxCount = 5;
            this.Timeout = Timeout;

            this.Buffer = new List<byte>(256);


            ReqSendEvent += delegate { };
            ByteRecievedEvent += delegate { };

            try
            {
                Port.Open();
            }
            catch (Exception ex)
            {
                throw new RTU_Exception("Ошибка открытия коммуникационного порта. " + ex.Message, ex);
            }

        }
        public void Dispose()
        {
            lock (Port)
            {
                if (Port.IsOpen)
                    Port.Close();
            }
            Port.Dispose();
        }

        #region IModbusClient Members
        public bool[] ReadCoils(IModbusDevice Device, int StartAddr, int Quantity)
        {
            if (!(Device is RTU_Device))
                throw new ArgumentException("Wrong Type", "Device");

            var PDU = new ReadCoilsPDU(StartAddr, Quantity);
            var ADU = new RTU_ADU((Device as RTU_Device), PDU);

            Query(ADU);

            return PDU.Value;
        }
        public bool[] ReadDiscreteInputs(IModbusDevice Device, int StartAddr, int Quantity)
        {
            if (!(Device is RTU_Device))
                throw new ArgumentException("Wrong Type", "Device");

            var PDU = new ReadDiscreteInputsPDU(StartAddr, Quantity);
            var ADU = new RTU_ADU((Device as RTU_Device), PDU);

            Query(ADU);
            return PDU.Value;
        }
        public int[] ReadHoldingRegisters(IModbusDevice Device, int StartAddr, int Quantity)
        {
            if (!(Device is RTU_Device))
                throw new ArgumentException("Wrong Type", "Device");

            var PDU = new ReadHoldingRegistersPDU(StartAddr, Quantity);
            var ADU = new RTU_ADU((Device as RTU_Device), PDU);
            Query(ADU);
            return PDU.Value;
        }
        public int[] ReadInputRegisters(IModbusDevice Device, int StartAddr, int Quantity)
        {
            var PDU = new ReadInputRegistersPDU(StartAddr, Quantity);
            var ADU = new RTU_ADU((Device as RTU_Device), PDU);
            Query(ADU);
            return PDU.Value;
        }

        public void WriteSingleCoil(IModbusDevice Device, int StartAddr, bool Value)
        {
            if (!(Device is RTU_Device))
                throw new ArgumentException("Wrong Type", "Device");

            var PDU = new WriteSingleCoilPDU(StartAddr, Value);
            var ADU = new RTU_ADU((Device as RTU_Device), PDU);
            Query(ADU);
        }
        public void WriteSingleRegister(IModbusDevice Device, int StartAddr, int Value)
        {
            if (!(Device is RTU_Device))
                throw new ArgumentException("Wrong Type", "Device");

            var PDU = new WriteSingleRegisterPDU(StartAddr, Value);
            var ADU = new RTU_ADU((Device as RTU_Device), PDU);

            Query(ADU);
        }
        public void WriteMultipleCoils(IModbusDevice Device, int StartAddr, bool[] Value)
        {
            if (!(Device is RTU_Device))
                throw new ArgumentException("Wrong Type", "Device");

            var PDU = new WriteMultipleCoilsPDU(StartAddr, Value);
            var ADU = new RTU_ADU((Device as RTU_Device), PDU);

            Query(ADU);
        }
        public void WriteMultipleRegisters(IModbusDevice Device, int StartAddr, int[] Value)
        {
            if (!(Device is RTU_Device))
                throw new ArgumentException("Wrong Type", "Device");

            var PDU = new WriteMultipleRegistersPDU(StartAddr, Value);
            var ADU = new RTU_ADU((Device as RTU_Device), PDU);

            Query(ADU) ;

       
        }
        #endregion

        private Status Query(RTU_ADU ADU)
        {
            lock (Port)
            {
                var Request = ADU.BuildRequest();
                var Count = 0;

                try
                {

                    do
                    {

                        Port.DiscardInBuffer();
                        Buffer.Clear();

                        Port.Write(Request, 0, ADU.ReqSize);
                        ReqSendEvent(ADU.BuildRequest());

                        if (!ADU.Device.Broadcast)
                        {
                            Port.ReadTimeout = Timeout;

                            #region Wait for first byte
                            try
                            {
                                Buffer.Add((byte)Port.ReadByte());
                                ByteRecievedEvent(Buffer.Last());
                            }
                            catch (TimeoutException)
                            {
                                Count++;
                                continue;
                            }
                            #endregion

                            Port.ReadTimeout = ByteTimeout;

                            #region Recieve All bytes
                            try
                            {
                                for (int i = 0; i < ADU.RespSize - 1; i++)
                                {
                                    Buffer.Add((byte)Port.ReadByte());
                                    ByteRecievedEvent(Buffer.Last());
                                }
                            }
                            catch (TimeoutException) { }
                            #endregion

                            switch (ADU.CheckResponse(Buffer.ToArray()))
                            {
                                case Status.OK: Thread.Sleep(4); return Status.OK;
                                case Status.Exception: throw new ModbusException(ADU.Device.Address, FunctionCode.WriteMultipleRegisters, (ExceptionType)ADU.PDU.ExceptionCode);
                                case Status.Error: Thread.Sleep(4); Count++; break;
                            }
                        }
                        else
                            return Status.OK;

                    } while (Count < MaxCount);
                }
                catch (IOException ex) { throw new RTU_Exception("Ошибка порта", ex); }
                throw new ModbusException(ADU.Device.Address, FunctionCode.WriteMultipleRegisters, (ExceptionType)ADU.PDU.ExceptionCode);
            }

        }

        public override string ToString()
        {
            return "Modbus RTU:" +
                   "\nPort: " + Port.PortName +
                   "\nBaudRate: " + Port.BaudRate.ToString() +
                   "\nParity: " + Port.Parity.ToString() +
                   "\nData Bits: " + Port.DataBits.ToString() +
                   "\nStop Bits: " + Port.StopBits.ToString() +
                   "\nTimeout: " + Timeout.ToString();
        }

    }
}
