using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus.Common
{
    public class ReadHoldingRegistersPDU : IProtocolDataUnit
    {
        static byte ByteCount(int Quantity)
        {
            return (byte)(Quantity * 2);
        }

        public int StartingAddress
        {
            get;
            protected set;
        }
        public int Quantity
        {
            get;
            protected set;
        }
        public byte ExceptionCode
        {
            get;
            protected set;
        }

        public int[] Value
        {
            get;
            private set;
        }

        public byte Func
        {
            get { return 0x03; }
        }
        public byte ErrorFunc
        {
            get { return 0x83; }
        }
        public int ReqSize
        {
            get { return 5; }
        }
        public int RespSize
        {
            get { return 2 + ByteCount(Quantity); }
        }

        public ReadHoldingRegistersPDU(int StartingAddress, int Quantity)
        {
            if ((StartingAddress < 0) || (StartingAddress > 0xFFFF))
                throw new ArgumentOutOfRangeException("StartingAddress", "Starting Address must be in range 0x0000 to 0xFFFF");

            if ((Quantity < 1) || (Quantity > 125))
                throw new ArgumentOutOfRangeException("ReadHoldingRegisters: Quantity", "Quantity must be in range 1 - 125");

            this.StartingAddress = StartingAddress;

            this.Quantity = Quantity; 
            Value = new int[Quantity];
        }
        public byte[] BuildRequest()
        {
            return new byte[5]
                {
                    Func,
                    (byte) ( (StartingAddress >> 8) & 0xFF ),
                    (byte) ( (StartingAddress) & 0xFF ),
                    (byte) ( (Quantity >> 8) &0xFF  ),
                    (byte) ( (Quantity) & 0xFF )

                };

        }
        public Status CheckResponse(byte[] Response)
        {

            if ( (Response.Length == RespSize) && (Response[0] == Func) && ((Response[1] >> 1) == Quantity) )
            {
                for (int i = 0; i < Quantity; i++)
                    Value[i] = (Response[2 * i + 2] << 8) + Response[2 * i + 3];

                return Status.OK;
            }
            else if ( (Response.Length == 2) && (Response[0] == ErrorFunc) )
            {
                ExceptionCode = Response[1];
                return Status.Exception;
            }
            else
                return Status.Error;

        }

        public ReadHoldingRegistersPDU() { }
        public Status CheckRequest(byte[] Request)
        {
            if (Request.Length != ReqSize)
                return Status.Error;

            StartingAddress = (Request[1] << 8) + Request[2];
            Quantity = (Request[3] << 8) + Request[4];

            Value = new int[Quantity];
            return Status.OK;
        }
        public void Handle(IModbusServer Server)
        {
            try
            {
                Value = Server.ReadHoldingRegisters(StartingAddress, Quantity);
            }
            catch (ModbusException ex)
            {
                ExceptionCode = (byte)ex.Code;
            }
        }
        public byte[] BuildResponse()
        {
            byte[] tmp;
            if (ExceptionCode == 0)
            {
                tmp = new byte[RespSize];
                tmp[0] = Func;
                tmp[1] = ByteCount(Quantity);

                for (int i=0; i<Quantity; i++)
                {
                    tmp[2 + (i * 2)] = (byte)(Value[i] >> 8);
                    tmp[2 + (i * 2 + 1)] = (byte)(Value[i] & 0xFF);
                }
            }
            else
            {
                tmp = new byte[2];
                tmp[0] = ErrorFunc;
                tmp[1] = ExceptionCode;
            }
            return tmp;
        }
    }
}
