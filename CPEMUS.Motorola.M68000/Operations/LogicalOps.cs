﻿using CPEMUS.Motorola.M68000.EA;

namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        // Exclusive-OR Logical.
        private M68KExecResult Eor(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var dataRegIdx = (uint)((opcode >> 9) & 0x7);
            var dataReg = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, operandSize);

            var eaProps = _eaHelper.Get(opcode, operandSize);

            var result = dataReg ^ eaProps.Operand;

            _flagsHelper.AlterN(result, operandSize);
            _flagsHelper.AlterZ(result, operandSize);
            _regs.V = false;
            _regs.C = false;

            _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);

            int clockPeriods = eaProps.ClockPeriods;
            if (eaProps.Mode == EAMode.DataDirect && operandSize == OperandSize.Long)
            {
                clockPeriods += 4;
            }

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods
            };
        }

        // Exclusive-OR Immediate.
        private M68KExecResult Eori(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var immediateOperand = _memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, operandSize);

            var opcodeSize = INSTR_DEFAULT_SIZE + (operandSize == OperandSize.Byte ? 2 : (int)operandSize);
            var eaProps = _eaHelper.Get(opcode, operandSize, opcodeSize);

            var result = eaProps.Operand ^ immediateOperand;

            _flagsHelper.AlterN(result, operandSize);
            _flagsHelper.AlterZ(result, operandSize);
            _regs.V = false;
            _regs.C = false;

            _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);

            int clockPeriods = eaProps.ClockPeriods;
            if (eaProps.Mode == EAMode.DataDirect && operandSize == OperandSize.Long)
            {
                clockPeriods += 4;
            }

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods
            };
        }

        // Exclusive-OR Immediate to CCR.
        private M68KExecResult EoriToCcr(ushort opcode)
        {
            uint src = _memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, OperandSize.Byte);
            var ccr = _regs.CCR;

            var result = (byte)(src ^ ccr);

            _regs.CCR = result;

            int clockPeriods = 8;

            return new()
            {
                InstructionSize = INSTR_DEFAULT_SIZE + 2,
                ClockPeriods = clockPeriods
            };
        }

        // Logical Complement.
        private M68KExecResult Not(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var eaProps = _eaHelper.Get(opcode, operandSize);

            var result = ~eaProps.Operand;
            _memHelper.Write(~eaProps.Operand, eaProps.Address, eaProps.Location, operandSize);

            _flagsHelper.AlterN(result, operandSize);
            _flagsHelper.AlterZ(result, operandSize);
            _regs.V = _regs.C = false;

            int clockPeriods = eaProps.ClockPeriods;
            if (operandSize == OperandSize.Long && (eaProps.Mode == EAMode.DataDirect || eaProps.Mode == EAMode.AddressDirect))
            {
                clockPeriods += 2;
            }

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods
            };
        }

        // Inclusive-OR Logical.
        private M68KExecResult Or(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var eaProps = _eaHelper.Get(opcode, operandSize);
            var dataRegIdx = (uint)((opcode >> 9) & 0x7);
            var dataReg = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, operandSize);

            var result = eaProps.Operand | dataReg;

            int clockPeriods = eaProps.ClockPeriods;

            var writeToDataReg = ((opcode >> 8) & 0x1) == 0;
            if (writeToDataReg)
            {
                _memHelper.Write(result, dataRegIdx, StoreLocation.DataRegister, operandSize);
                if (operandSize == OperandSize.Long)
                {
                    clockPeriods += eaProps.Mode == EAMode.DataDirect || eaProps.Mode == EAMode.ImmediateData ? 4 : 2;
                }
            }
            else
            {
                _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);
            }

            _flagsHelper.AlterN(result, operandSize);
            _flagsHelper.AlterZ(result, operandSize);
            _regs.V = _regs.C = false;

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods
            };
        }

        // Inclusive-OR.
        private M68KExecResult Ori(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var opcodeSize = Math.Max((int)operandSize, (int)OperandSize.Word) + INSTR_DEFAULT_SIZE;
            var immediateData = _memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, operandSize);
            var eaProps = _eaHelper.Get(opcode, operandSize, opcodeSize);

            var result = eaProps.Operand | immediateData;

            _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);

            _flagsHelper.AlterN(result, operandSize);
            _flagsHelper.AlterZ(result, operandSize);
            _regs.V = _regs.C = false;

            int clockPeriods = eaProps.ClockPeriods;
            if (eaProps.Mode == EAMode.DataDirect && operandSize == OperandSize.Long)
            {
                clockPeriods += 4;
            }

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods
            };
        }

        // Inclusive-OR Immediate to CCR.
        private M68KExecResult OriToCcr(ushort opcode)
        {
            var immediateData = _memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, OperandSize.Byte);

            var result = _regs.CCR | immediateData;

            _regs.CCR = (byte)result;

            int clockPeriods = 8;

            return new()
            {
                InstructionSize = INSTR_DEFAULT_SIZE + 2,
                ClockPeriods = clockPeriods
            };
        }


        private M68KExecResult And(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            uint srcIdx = (uint)((opcode >> 9) & 0x7);
            uint src = _memHelper.Read(srcIdx, StoreLocation.DataRegister, operandSize);

            var eaProps = _eaHelper.Get(opcode, operandSize);
            uint dest = eaProps.Operand;

            uint result = src & dest;

            // Flags.
            _regs.N = (result >> ((int)operandSize * 8 - 1)) == 1;
            _regs.Z = result == 0;
            _regs.V = false;
            _regs.C = false;

            int clockPeriods = 0;

            // Storing.
            int storeDirection = (opcode >> 8) & 0x1;
            if ((StoreDirection)storeDirection == StoreDirection.Source)
            {
                _memHelper.Write(result, srcIdx, StoreLocation.DataRegister, operandSize);
                if (operandSize == OperandSize.Long)
                {
                    clockPeriods += eaProps.Mode == EAMode.DataDirect || eaProps.Mode == EAMode.ImmediateData ? 4 : 2;
                }
            }
            else
            {
                _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);
            }

            clockPeriods += eaProps.ClockPeriods;

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods
            };
        }

        private M68KExecResult Andi(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var immediateDataSize = operandSize == OperandSize.Long
                ? (int)OperandSize.Long
                : (int)OperandSize.Word;
            var pc = _regs.PC;

            uint src = _memHelper.Read(pc + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, operandSize);

            var eaProps = _eaHelper.Get(opcode, operandSize, INSTR_DEFAULT_SIZE + immediateDataSize);
            uint dest = eaProps.Operand;

            uint result = src & dest;

            // Flags.
            _regs.N = (result >> ((int)operandSize * 8 - 1)) == 1;
            _regs.Z = result == 0;
            _regs.V = false;
            _regs.C = false;

            // Storing.
            _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);

            int clockPeriods = eaProps.ClockPeriods;
            if (eaProps.Mode == EAMode.DataDirect && operandSize == OperandSize.Long)
            {
                clockPeriods += 2;
            }

            return new()
            {
                InstructionSize = eaProps.InstructionSize,
                ClockPeriods = clockPeriods
            };
        }
    }
}
