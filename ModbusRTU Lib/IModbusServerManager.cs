using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modbus
{
    interface IModbusServerManager
    {
        void AddCoil(int Address, bool Value = false);
        void AddDiscreteInput(int Address, bool Value = false);
        void AddInputRegister(int Address, int Value = 0);
        void AddHoldingRegister(int Address, int Value = 0);

        void DeleteCoil(int Address);
        void DeleteDiscreteInput(int Address);
        void DeleteInputRegister(int Address);
        void DeleteHoldingRegister(int Address);

        void SetCoil(int Address, bool Value);
        void SetDiscreteInput(int Address, bool Value);
        void SetInputRegister(int Address, int Value);
        void SetHoldingRegister(int Address, int Value);

        bool GetCoil(int Address);
        bool GetDiscreteInput(int Address);
        int GetInputRegister(int Address);
        int GetHoldingRegister(int Address);
    }
}
