using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus;

namespace Modbus.Common
{
    public enum FunctionCode
    {
        ReadCoils = 0x01,
        ReadDiscreteInputs = 0x02,
        ReadHoldingRegisters = 0x03,
        ReadInputRegisters = 0x04,
        WriteSingleCoil = 0x05,
        WriteSingleRegister = 0x06,
        ReadExceptionStatus = 0x07,
        WriteMultipleCoils = 0x0F,
        WriteMultipleRegisters = 0x10,
    }   
    public enum ErrorCode
    {
        ReadCoils = 0x81,
        ReadDiscreteInputs = 0x82,
        ReadHoldingRegisters = 0x83,
        ReadInputRegisters = 0x84,
        WriteSingleCoil = 0x85,
        WriteSingleRegister = 0x86,
        ReadExceptionStatus = 0x87,
        WriteMultipleCoils = 0x8F,
        WriteMultipleRegisters = 0x90

    }

    public interface IProtocolDataUnit
    {
        int StartingAddress { get; }
        int Quantity { get; }
        byte ExceptionCode { get; }

        int ReqSize { get;}
        int RespSize { get; }
        byte Func { get; }
        byte ErrorFunc { get; }

        byte[] BuildRequest();
        Status CheckResponse(byte[] Response);

        Status CheckRequest(byte[] Response);
        byte[] BuildResponse();

        void Handle(IModbusServer Server);
    }

    public class PDUException: ApplicationException
    {
        public PDUException() : base() { }
        public PDUException(string message) : base(message) { }
    }
}
