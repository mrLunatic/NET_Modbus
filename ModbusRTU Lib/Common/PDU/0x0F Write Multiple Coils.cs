using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus.Common
{
    public class WriteMultipleCoilsPDU : IProtocolDataUnit
    {
        static int ByteCount(int ValueLenght)
        {
            int tmp = ValueLenght / 8;
            if ((ValueLenght % 8) != 0)
                tmp++;
            return tmp;
        }

        public int StartingAddress { get; protected set; }
        public int Quantity { get; protected set; }
        public byte ExceptionCode { get; protected set; }

        public bool[] Value { get; private set; }

        public byte Func
        {
            get { return 0x0F; }
        }
        public byte ErrorFunc
        {
            get { return 0x8F; }
        }
        public int ReqSize
        {
            get { return 6 + ByteCount(Quantity); }
        }
        public int RespSize
        {
            get { return 5; }
        }

        public WriteMultipleCoilsPDU(int StartingAddress, bool[] Value)
        {
            if ((StartingAddress < 0) || (StartingAddress > 0xFFFF))
                throw new ArgumentOutOfRangeException("StartingAddress", "Starting Address must be in range 0x0000 to 0xFFFF");

            if ((Value.Length < 1) || (Value.Length > 0x07B0))
                throw new ArgumentOutOfRangeException("WriteMultipleCoils: Quantity", "Coil Quantity must be in range 1 - 1969");

            this.StartingAddress = StartingAddress;
            this.Quantity = Value.Length;
            this.Value = Value;
        }
        public byte[] BuildRequest()
        {
            byte[] tmp = new byte[ReqSize];

            tmp[0] = Func;
            tmp[1] = (byte)((StartingAddress >> 8) & 0xFF);
            tmp[2] = (byte)((StartingAddress) & 0xFF);
            tmp[3] = (byte)((Quantity >> 8) & 0xFF);
            tmp[4] = (byte)((Quantity) & 0xFF);
            tmp[5] = (byte)ByteCount(Quantity);

            for (int i = 0; i < Quantity; i++)
            {
                if (Value[i])
                    tmp[i / 8 + 6] += (byte)(1 << (i % 8));
            }

            return tmp;

        }
        public Status CheckResponse(byte[] Response)
        {
            if ((Response.Length == RespSize) && (Response[0] == Func) && ((Response[1] << 8) + Response[2] != StartingAddress) && ((Response[3] << 8) + Response[4] != Quantity))
                return Status.OK;

            else if ((Response.Length == 2) && (Response[0] == ErrorFunc))
            {
                ExceptionCode = Response[1];
                return Status.Exception;
            }
            else return Status.Error;
        }

        public WriteMultipleCoilsPDU() { }
        public Status CheckRequest(byte[] Request)
        {
            if (Request.Length < 6)
                return Status.Error;

            StartingAddress = (Request[1] << 8) + Request[2];
            Quantity = (Request[3] << 8) + Request[4];
            Value = new bool[Quantity];

            if (Request.Length != ReqSize)
                return Status.Error;

            for (int i = 0; i < Quantity; i++)
                Value[i] = ( (Request[i / 8 + 6] & (1 << (i % 8))) != 0 );

            return Status.OK;
        }
        public void Handle(IModbusServer Server)
        {
            try
            {
                Server.WriteMultipleCoils(StartingAddress, Value);
            }
            catch (ModbusException ex)
            {
                ExceptionCode = (byte)ex.Code;
            }
        }
        public byte[] BuildResponse()
        {
            if (ExceptionCode == 0)
            {
                return new byte[]
                {
                    Func,
                    (byte)((StartingAddress >> 8) & 0xFF),
                    (byte)((StartingAddress) & 0xFF),
                    (byte)((Quantity >> 8) & 0xFF),
                    (byte)((Quantity) & 0xFF),
                };
            }
            else
            {
                return new byte[]
                {
                    ErrorFunc,
                    ExceptionCode,
                };
            }
        }
    }

}
