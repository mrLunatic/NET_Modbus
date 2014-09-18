using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus.Common
{
    public class WriteMultipleRegistersPDU : IProtocolDataUnit
    {
        static int ByteCount(int ValueLenght)
        {
            return 2 * ValueLenght;
        }

        public int StartingAddress { get; protected set; }
        public int Quantity { get; protected set; }
        public byte ExceptionCode { get; protected set; }

        public int[] Value { get; private set; }

        public byte Func
        {
            get { return 0x10; ; }
        }
        public byte ErrorFunc
        {
            get { return 0x90; }
        }
        public int ReqSize
        {
            get { return 6 + ByteCount(Value.Length); }
        }
        public  int RespSize
        {
            get { return 5; }
        }

        public WriteMultipleRegistersPDU(int StartingAddress, int[] Value)
        {

            if ((StartingAddress < 0) || (StartingAddress > 0xFFFF))
                throw new ArgumentOutOfRangeException("StartingAddress", "Starting Address must be in range 0x0000 to 0xFFFF");

            if ((Value.Length < 1) || (Value.Length > 123))
                throw new ArgumentOutOfRangeException("WriteMultipleRegistersPDU: Quantity", "Register Quantity must be in range 1 - 123");


            this.StartingAddress = StartingAddress;

            this.Quantity = Value.Length; 
            this.Value = Value;
        }
        public byte[] BuildRequest()
        {
            if (ExceptionCode == 0)
            {
                byte[] tmp = new byte[ReqSize];

                tmp[0] = Func;
                tmp[1] = ((byte)((StartingAddress >> 8) & 0xFF));
                tmp[2] = ((byte)((StartingAddress & 0xFF)));
                tmp[3] = ((byte)((Quantity >> 8) & 0xFF));
                tmp[4] = ((byte)((Quantity & 0xFF)));
                tmp[5] = ((byte)((Quantity << 1)));

                for (int i = 0; i < Value.Length; i++)
                {
                    tmp[2 * i + 6] = (byte)((Value[i] >> 8) & 0xFF);
                    tmp[2 * i + 7] = (byte)((Value[i] & 0xFF));
                }

                return tmp;
            }
            else
                return new byte[]
                {
                    Func,
                    ExceptionCode
                };

        }
        public Status CheckResponse(byte[] Response)
        {

            if ((Response.Length == RespSize) && (Response[0] == Func) && (((Response[1] << 8) + Response[2]) == StartingAddress) && (((Response[3] << 8) + Response[4]) == Quantity))
                return Status.OK;

            else if ( (Response.Length == 2) && (Response[0] == ErrorFunc) )
            {
                ExceptionCode = Response[1];
                return Status.Exception;
            }
            else
                return Status.Error;

        }

        public WriteMultipleRegistersPDU() { }
        public Status CheckRequest(byte[] Request)
        {
            if (Request.Length < 6)
                return Status.Error;

            StartingAddress = (Request[1] << 8) + Request[2];
            Quantity = (Request[3] << 8) + Request[4];
            Value = new int[Quantity];

            if (Request.Length != ReqSize)
                return Status.Error;

            for (int i = 0; i < Quantity; i++)
                Value[i] = (Request[2 * i + 6] << 8) + Request[2 * i + 7];

            return Status.OK;
        }
        public void Handle(IModbusServer Server)
        {
            try
            {
                Server.WriteMultipleRegisters(StartingAddress, Value);
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
                    (byte)((StartingAddress & 0xFF)),
                    (byte)((Quantity >> 8) & 0xFF),
                    (byte)(Quantity  & 0xFF),
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
