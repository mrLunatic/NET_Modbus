using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus
{
    public interface IModbusServer
    {
        void Start();
        void Stop();

        void WriteSingleCoil(int Address, bool Value);
        void WriteMultipleCoils(int Address, bool[] Value);

        void WriteSingleRegister(int Address, int Value);
        void WriteMultipleRegisters(int Address, int[] Value);

        bool[] ReadCoils(int Address, int Quantity);
        bool[] ReadDiscreteInputs(int Address, int Quantity);
        int[] ReadInputRegisters(int Address, int Quantity);
        int[] ReadHoldingRegisters(int Address, int Quantity);
    }
}
