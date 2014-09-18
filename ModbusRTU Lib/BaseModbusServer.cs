using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus;
using Modbus.Common;

namespace Modbus
{
    public class BaseModbusServer : IModbusServerManager, IModbusServer
    {

        Dictionary<int, bool> Coils = new Dictionary<int,bool>();
        Dictionary<int, bool> DiscreteInputs = new Dictionary<int, bool>();
        Dictionary<int, int> InputRegisters = new Dictionary<int,int>();
        Dictionary<int, int> HoldingRegisters = new Dictionary<int,int>();

        public Dictionary<int, bool> CoilsList
        {
            get
            {
                return new Dictionary<int, bool>(Coils);
            }
        }
        public Dictionary<int, bool> DiscreteInputsList
        {
            get
            {
                return new Dictionary<int, bool>(DiscreteInputs);
            }
        }
        public Dictionary<int, int> InputRegistersList
        {
            get
            {
                return new Dictionary<int, int>(InputRegisters);
            }
        }
        public Dictionary<int, int> HoldingRegistersList
        {
            get
            {
                return new Dictionary<int, int>(HoldingRegisters);
            }
        }


        public event EventHandler<CoilUpdateEventArgs> CoilUpdateEvent;
        public event EventHandler<RegisterUpdateEventArgs> RegisteUpdateEvent;

        public class CoilUpdateEventArgs : EventArgs
        {
            public readonly int Address;
            public readonly bool Value;

            public CoilUpdateEventArgs(int Address, bool Value)
            {
                this.Address = Address;
                this.Value = Value;
            }
        }
        public class RegisterUpdateEventArgs : EventArgs
        {
            public readonly int Address;
            public readonly int Value;

            public RegisterUpdateEventArgs(int Address, int Value)
            {
                this.Address = Address;
                this.Value = Value;
            }
        }

        #region Modbus Server Manager
        public virtual void AddCoil(int Address, bool Value = false)
        {
                Coils.Add(Address, Value);
        }
        public virtual void AddDiscreteInput(int Address, bool Value = false)
        {
                DiscreteInputs.Add(Address, Value);
        }
        public virtual void AddInputRegister(int Address, int Value = 0)
        {
                InputRegisters.Add(Address, Value);
        }
        public virtual void AddHoldingRegister(int Address, int Value = 0)
        {
                HoldingRegisters.Add(Address, Value);
        }

        public virtual void SetCoil(int Address, bool Value)
        {
                Coils[Address] = Value;
        }
        public virtual void SetDiscreteInput(int Address, bool Value)
        {
                DiscreteInputs[Address] = Value;
        }
        public virtual void SetInputRegister(int Address, int Value)
        {
                InputRegisters[Address] = Value;
        }
        public virtual void SetHoldingRegister(int Address, int Value)
        {
                HoldingRegisters[Address] = Value;
        }


        public virtual bool GetCoil(int Address)
        {
            return Coils[Address];
        }
        public virtual bool GetDiscreteInput(int Address)
        {
            return DiscreteInputs[Address];
        }
        public virtual int GetInputRegister(int Address)
        {
            return InputRegisters[Address];
        }
        public virtual int GetHoldingRegister(int Address)
        {
            return HoldingRegisters[Address];
        }

        public virtual void DeleteCoil(int Address)
        {
                Coils.Remove(Address);
        }
        public virtual void DeleteDiscreteInput(int Address)
        {
                DiscreteInputs.Remove(Address);
        }
        public virtual void DeleteInputRegister(int Address)
        {
                InputRegisters.Remove(Address);
        }
        public virtual void DeleteHoldingRegister(int Address)
        {
                HoldingRegisters.Remove(Address);
        }
        #endregion

        #region Modbus Server
        public virtual void Start() {}
        public virtual void Stop() {}

        public virtual void WriteSingleCoil(int Address, bool Value)
        {
            if (Coils.Count == 0)
                throw new ModbusException(0, FunctionCode.WriteSingleCoil, ExceptionType.IllegalFunction);

            if (!Coils.ContainsKey(Address))
                throw new ModbusException(0, FunctionCode.WriteSingleCoil, ExceptionType.IllegalDataAddress);

                Coils[Address] = Value;
        }
        public virtual void WriteMultipleCoils(int Address, bool[] Value)
        {
            if (Coils.Count == 0)
                throw new ModbusException(0, FunctionCode.WriteMultipleCoils, ExceptionType.IllegalFunction);

            for (int i = 0; i < Value.Length; i++ )
            {
                if (!Coils.ContainsKey(Address))
                    throw new ModbusException(0, FunctionCode.WriteMultipleCoils, ExceptionType.IllegalDataAddress);
                Coils[Address + i] = Value[i];
            }
        }

        public virtual void WriteSingleRegister(int Address, int Value)
        {
            if (HoldingRegisters.Count == 0)
                throw new ModbusException(0, FunctionCode.WriteSingleRegister, ExceptionType.IllegalFunction);

            if (!HoldingRegisters.ContainsKey(Address))
                throw new ModbusException(0, FunctionCode.WriteSingleRegister, ExceptionType.IllegalDataAddress);
            HoldingRegisters[Address] = Value;
        }
        public virtual void WriteMultipleRegisters(int Address, int[] Value)
        {
            if (HoldingRegisters.Count == 0)
                throw new ModbusException(0, FunctionCode.WriteMultipleRegisters, ExceptionType.IllegalFunction);

            for (int i = 0; i < Value.Length; i++)
            {
                if (!HoldingRegisters.ContainsKey(Address))
                    throw new ModbusException(0, FunctionCode.WriteMultipleRegisters, ExceptionType.IllegalDataAddress);
                HoldingRegisters[Address + i] = Value[i];
            }
        }

        public virtual bool[] ReadCoils(int Address, int Quantity)
        {
            if (Coils.Count == 0)
                throw new ModbusException(0, FunctionCode.ReadCoils, ExceptionType.IllegalFunction);

            var output = new bool[Quantity];

            for (int i=0; i < Quantity; i ++)
            {
                if (!Coils.ContainsKey(Address))
                    throw new ModbusException(0, FunctionCode.ReadCoils, ExceptionType.IllegalDataAddress);
                output[i] = Coils[Address + i];
            }

            return output;
        }
        public virtual bool[] ReadDiscreteInputs(int Address, int Quantity)
        {
            if (DiscreteInputs.Count == 0)
                throw new ModbusException(0, FunctionCode.ReadDiscreteInputs, ExceptionType.IllegalFunction);

            var output = new bool[Quantity];

            for (int i = 0; i < Quantity; i++)
            {
                if (!DiscreteInputs.ContainsKey(Address))
                    throw new ModbusException(0, FunctionCode.ReadDiscreteInputs, ExceptionType.IllegalDataAddress);
                output[i] = DiscreteInputs[Address + i];
            }

            return output;
        }
        public virtual int[] ReadInputRegisters(int Address, int Quantity)
        {
            if (InputRegisters.Count == 0)
                throw new ModbusException(0, FunctionCode.ReadInputRegisters, ExceptionType.IllegalFunction);

            var output = new int[Quantity];

            for (int i = 0; i < Quantity; i++)
            {
                if (!InputRegisters.ContainsKey(Address))
                    throw new ModbusException(0, FunctionCode.ReadInputRegisters, ExceptionType.IllegalDataAddress, "Input Register #" + Address.ToString() + " not found");

                output[i] = InputRegisters[Address];
                Address++;
            }

            return output;
        }
        public virtual int[] ReadHoldingRegisters(int Address, int Quantity)
        {
            if (HoldingRegisters.Count == 0)
                throw new ModbusException(0, FunctionCode.ReadHoldingRegisters, ExceptionType.IllegalFunction);

            var output = new int[Quantity];

            for (int i = 0; i < Quantity; i++)
            {
                if (!HoldingRegisters.ContainsKey(Address))
                    throw new ModbusException(0, FunctionCode.ReadHoldingRegisters, ExceptionType.IllegalDataAddress, "Input Register #" + Address.ToString() + " not found");

                output[i] = HoldingRegisters[Address];
                Address++;
            }

            return output;
        }
        #endregion
    }



}
