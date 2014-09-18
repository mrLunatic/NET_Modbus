using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace Modbus
{
    public interface IModbusClient :IDisposable
    {
        bool[] ReadCoils(IModbusDevice Device, int StartAddr, int Quantity);
        bool[] ReadDiscreteInputs(IModbusDevice Device, int StartAddr, int Quantity);
        int[] ReadHoldingRegisters(IModbusDevice Device, int StartAddr, int Quantity);
        int[] ReadInputRegisters(IModbusDevice Device, int StartAddr, int Quantity);

        void WriteSingleCoil(IModbusDevice Device, int StartAddr, bool Value);
        void WriteSingleRegister(IModbusDevice Device, int StartAddr, int Value);
        void WriteMultipleCoils(IModbusDevice Device, int StartAddr, bool[] Value);
        void WriteMultipleRegisters(IModbusDevice Device, int StartAddr, int[] Value);
    }
}

