using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus.Common
{
    public class WriteSingleRegisterPDU : IProtocolDataUnit
    {
        public int StartingAddress { get; protected set; }
        public int Quantity { get; protected set; }
        public byte ExceptionCode { get; protected set; }

        public int Value { get; private set; }

        public byte Func
        {
            get { return 0x06; }
        }
        public byte ErrorFunc
        {
            get { return 0x86; }
        }
        public int ReqSize
        {
            get { return 5; }
        }
        public int RespSize
        {
            get { return 5; }
        }

        public WriteSingleRegisterPDU(int StartingAddress, int Value)
        {
            if ((StartingAddress < 0) || (StartingAddress > 0xFFFF))
                throw new ArgumentOutOfRangeException("StartingAddress", "Starting Address must be in range 0x0000 to 0xFFFF");

            this.StartingAddress = StartingAddress;

            this.Quantity = 1; 

            this.Value = Value;
        }
        public byte[] BuildRequest()
        {
            return new byte[5]
                {
                    Func,
                    (byte) ((StartingAddress >> 8) & 0xFF),
                    (byte) (StartingAddress & 0xFF),
                    (byte) ((Value >> 8) & 0xFF),
                    (byte) (Value & 0xFF)
                };
        }
        public Status CheckResponse(byte[] Response)
        {
            if ( (Response.Length == RespSize ) && (Response[0] == Func) && ((Response[1] << 8) + Response[2] == StartingAddress) && ((Response[3] << 8) + Response[4] == Value) )
                return Status.OK;

            else if ( (Response.Length == 2 ) && (Response[0] == ErrorFunc) )
            {
                ExceptionCode = Response[1];
                return Status.Exception;
            }
            else
                return Status.Error;
        }

        public WriteSingleRegisterPDU() { }
        public Status CheckRequest(byte[] Request)
        {
            if (Request.Length != ReqSize)
                return Status.Error;

            StartingAddress = (Request[1] << 8) + Request[2];
            Value = ((Request[3] << 8) + Request[4]);

            return Status.OK;
        }
        public void Handle(IModbusServer Server)
        {
            try
            {
                Server.WriteSingleRegister(StartingAddress, Value);
            }
            catch (ModbusException ex)
            {
                ExceptionCode = (byte)ex.Code;
            };
        }
        public byte[] BuildResponse()
        {

            if (ExceptionCode == 0)
                return new byte[]
                {
                    Func,
                    (byte) ( (StartingAddress >> 8) & 0xFF ),
                    (byte) ( (StartingAddress) & 0xFF ),
                    (byte) ((Value >> 8) & 0xFF),
                    (byte) (Value & 0xFF),
                };
            else
                return new byte[]
                {
                    ErrorFunc,
                    ExceptionCode,
                };
        }
    }
}
