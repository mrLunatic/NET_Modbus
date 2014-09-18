using System;

namespace Modbus.Common
{
    public class ReadCoilsPDU : IProtocolDataUnit
    {
        static byte ByteCount(int Quantity)
        {
            int tmp = Quantity / 8;
            if ((Quantity % 8) != 0)
                tmp++;
            return (byte)tmp;         
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
        public bool[] Value
        {
            get;
            private set;
        }

        public byte Func
        {
            get
            {
                return 0x01;
            }
        }
        public byte ErrorFunc
        {
            get
            {
                return 0x81;
            }
        }
        public int ReqSize {
            get
            {
                return 5;
            }
        }
        public int RespSize
        {
            get
            {
                return 2 + ByteCount(Quantity);
            }
        }

        public ReadCoilsPDU(int StartingAddress, int Quantity)
        {
            if ( (StartingAddress < 0) || (StartingAddress > 0xFFFF) )
                throw new ArgumentOutOfRangeException("StartingAddress", "Starting Address must be in range 0x0000 to 0xFFFF");

            if ( (Quantity < 1) || (Quantity > 2000) )
                throw new ArgumentOutOfRangeException("ReadCoils: Quantity", "Quantity must be in range 1 - 2000");

            this.StartingAddress = StartingAddress;
            this.Quantity = Quantity; 

            Value = new bool[Quantity];
        }
        public byte[] BuildRequest()
        {
            return new byte[5]
            {
                Func,
                (byte) ( (StartingAddress >> 8) & 0xFF ),
                (byte) ( (StartingAddress) & 0xFF ),
                (byte) ( (Quantity >> 8) & 0xFF  ),
                (byte) ( (Quantity) & 0xFF )
            };
        }
        public Status CheckResponse(byte[] Response)
        {
            if ( (Response.Length == RespSize) && (Response[0] == Func) && (Response[1] == ByteCount(Quantity)))
            {
                for (int i = 0; i < Quantity; i++)
                    Value[i] = ((Response[(i / 8) + 2] >> (i % 8)) & 0x01) != 0;
                
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

        public ReadCoilsPDU() { }

        public Status CheckRequest(byte[] Request)
        {
            if (Request.Length != ReqSize)
                return Status.Error;

            StartingAddress = (Request[1] << 8) + Request[2];
            Quantity = (Request[3] << 8) + Request[4];

            Value = new bool[Quantity];

            return Status.OK;
        }
        public byte[] BuildResponse()
        {
            if (ExceptionCode == 0)
            {
                var tmp = new byte[RespSize];

                tmp[0] = Func;
                tmp[1] = ByteCount(Quantity);

                for (int i = 0; i < Quantity; i++ )
                {
                    if (Value[i] == true)
                        tmp[2 + i / 8] += (byte)(1 << (i % 8)); 
                }
                return tmp;
            }
            else
            {
                var tmp = new byte[2];
                tmp[0] = ErrorFunc;
                tmp[1] = ExceptionCode;
                return tmp;
            }
        }
        public void Handle(IModbusServer Server)
        {
            try
            {
                Value = Server.ReadCoils(StartingAddress, Quantity);
            }
            catch (ModbusException ex)
            {
                ExceptionCode = (byte)ex.Code;
            }
        }

    }
}
