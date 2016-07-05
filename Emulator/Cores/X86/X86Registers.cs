/********************************************************
*
* Warning!
* This file was generated automatically.
* Please do not edit. Changes should be made in the
* appropriate *.tt file.
*
*/
using System;
using System.Collections.Generic;
using Emul8.Peripherals.CPU.Registers;
using Emul8.Utilities.Binding;

namespace Emul8.Peripherals.CPU
{
    public partial class X86
    {
        public override void SetRegisterUnsafe(int register, uint value)
        {
            SetRegisterValue32(register, value);
        }

        public override uint GetRegisterUnsafe(int register)
        {
            return GetRegisterValue32(register);
        }
        
        public override int[] GetRegisters()
        {
            return new int[] {
                0,
            };
        }

        [Register]
        public UInt32 EIP
        {
            get
            {
                return GetRegisterValue32((int)X86Registers.EIP);
            }
            set
            {
                SetRegisterValue32((int)X86Registers.EIP, value);
            }
        }

        [Register]
        public override UInt32 PC
        {
            get
            {
                return GetRegisterValue32((int)X86Registers.PC);
            }
            set
            {
                SetRegisterValue32((int)X86Registers.PC, value);
            }
        }


        protected override void InitializeRegisters()
        {
        }

        [Import(Name = "tlib_set_register_value_32")]
        protected ActionInt32UInt32 SetRegisterValue32;
        [Import(Name = "tlib_get_register_value_32")]
        protected FuncUInt32Int32 GetRegisterValue32;

    }

    public enum X86Registers
    {
        EIP = 0,
        PC = 0,
    }
}
