using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus;

namespace Modbus.Common
{
    public enum ExceptionType
    {
        Timeout = 0x00,
        IllegalFunction = 0x01,
        IllegalDataAddress = 0x02,
        IllegalDataValue = 0x03,
        ServerDeviceFailure = 0x04,
        Acknowledge = 0x05,
        ServerDeviceBusy = 0x06,
        MemoryParityError = 0x08,
        GatewayPathUnavaliable = 0x0A,
        GatewayTargertDeviceFailedToResponse = 0x0B,
    }

    public class ModbusException : ApplicationException
    {
        public int DeviceAddress { get; private set; }
        public FunctionCode Code { get; private set; }
        public ExceptionType Type { get; private set; }
        public string Comment { get; private set; }

        public override string HelpLink
        {
            get
            {
                return "http://www.modbus.org/";
            }
        }
        public override string Message
        {
            get
            {
                string tmp = "\nMODBUS Exception occured:" +
                              "\nType: " + Type.ToString() +
                              "\nDevice ID: " + DeviceAddress.ToString() +
                              "\nFunction: " + Code.ToString();

                if (Comment != null)
                    tmp += "\nComment: " + Comment;

                return tmp;

            }
        }

        internal ModbusException(int DeviceAddress, FunctionCode FunctionCode, ExceptionType Type, string Comment = null)
        {
            this.Type = Type;  
            this.DeviceAddress = DeviceAddress;
            this.Code = FunctionCode;
            this.Comment = Comment;
        }
    }
}
