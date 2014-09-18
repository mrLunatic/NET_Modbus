using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus;
using Modbus.Common;


namespace Modbus.Common
{

    public enum Status
    {
        OK,
        Exception,
        Error
    }
    public class ADUException: ApplicationException
        {
            public ADUException() : base() {}
            public ADUException(string message) : base(message) { }
        }

    public interface IApplicationDataUnit
    {
        IProtocolDataUnit PDU { get; }
        IModbusDevice Device { get; }

        int ReqSize { get; }
        int RespSize { get; }

        byte[] BuildRequest();
        Status CheckResponse(byte[] Response);

        Status CheckRequest(byte[] Request);
        byte[] BuildResponse();
    }
}
