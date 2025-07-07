using CPEMUS.Motorola.M68000.EA;

namespace CPEMUS.Motorola.M68000
{
    public partial class M68K
    {
        // Add.
        private int Add(ushort opcode)
        {
            return AddSub(opcode, (uint srcOperand, uint destOperand, OperandSize operandSize) =>
            {
                long result = (long)srcOperand + destOperand;

                _flagsHelper.AlterN((uint)result, operandSize);
                _flagsHelper.AlterZ((uint)result, operandSize);
                _flagsHelper.AlterV(destOperand, srcOperand, result, operandSize);
                _flagsHelper.AlterC(result, operandSize);
                _regs.X = _regs.C;

                return result;
            });
        }

        // Subtract.
        private int Sub(ushort opcode)
        {
            return AddSub(opcode, (uint srcOperand, uint destOperand, OperandSize operandSize) =>
            {
                long result = destOperand - srcOperand;

                _flagsHelper.AlterN((uint)result, operandSize);
                _flagsHelper.AlterZ((uint)result, operandSize);
                _flagsHelper.AlterVSub(destOperand, srcOperand, result, operandSize);
                _flagsHelper.AlterCSub(destOperand, srcOperand, operandSize);
                _regs.X = _regs.C;

                return result;
            });
        }

        private int AddSub(ushort opcode, Func<uint, uint, OperandSize, long> getResult)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var dataRegIdx = (uint)((opcode >> 9) & 0x7);
            var dataReg = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, operandSize);

            var eaProps = _eaHelper.Get(opcode, operandSize);
            var storeDirection = (StoreDirection)((opcode >> 8) & 0x1);

            if (storeDirection == StoreDirection.Source)
            {
                long result = getResult(eaProps.Operand, dataReg, operandSize);
                _memHelper.Write((uint)result, dataRegIdx, StoreLocation.DataRegister, operandSize);
            }
            else
            {
                long result = getResult(dataReg, eaProps.Operand, operandSize);
                _memHelper.Write((uint)result, eaProps.Address, eaProps.Location, operandSize);
            }

            return eaProps.InstructionSize;
        }

        // Add Address.
        private int Adda(ushort opcode)
        {
            return AddaSuba(opcode, (uint srcOperand, uint destOperand) =>
            {
                return (long)srcOperand + destOperand;
            });
        }

        // Subtract Address.
        private int Suba(ushort opcode)
        {
            return AddaSuba(opcode, (uint srcOperand, uint destOperand) =>
            {
                return destOperand - (int)srcOperand;
            });
        }

        private int AddaSuba(ushort opcode, Func<uint, uint, long> getResult)
        {
            OperandSize operandSize;
            switch ((opcode >> 6) & 0x7)
            {
                case 0x3:
                    operandSize = OperandSize.Word;
                    break;
                case 0x7:
                    operandSize = OperandSize.Long;
                    break;
                default:
                    throw new InvalidOperationException("The given operation size is unknown.");
            }

            var eaProps = _eaHelper.Get(opcode, operandSize, signExtended: true);

            var addrRegIdx = (uint)((opcode >> 9) & 0x7);
            // The entire destination address register is used regardless of the operation size.
            var addrReg = _memHelper.Read(addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            long result = getResult(eaProps.Operand, addrReg);

            // Storing.
            _memHelper.Write((uint)result, addrRegIdx, StoreLocation.AddressRegister, OperandSize.Long);

            return eaProps.InstructionSize;
        }

        // Add Immediate.
        private int Addi(ushort opcode)
        {
            return AddiSubi(opcode, (uint srcOperand, uint destOperand, OperandSize operandSize) =>
            {
                long result = (long)srcOperand + destOperand;

                _flagsHelper.AlterN((uint)result, operandSize);
                _flagsHelper.AlterZ((uint)result, operandSize);
                _flagsHelper.AlterV(srcOperand, destOperand, result, operandSize);
                _flagsHelper.AlterC(result, operandSize);
                _regs.X = _regs.C;

                return result;
            });
        }

        // Subtract Immediate.
        private int Subi(ushort opcode)
        {
            return AddiSubi(opcode, (uint srcOperand, uint destOperand, OperandSize operandSize) =>
            {
                long result = destOperand - srcOperand;

                _flagsHelper.AlterN((uint)result, operandSize);
                _flagsHelper.AlterZ((uint)result, operandSize);
                _flagsHelper.AlterVSub(destOperand, srcOperand, result, operandSize);
                _flagsHelper.AlterCSub(destOperand, srcOperand, operandSize);
                _regs.X = _regs.C;

                return result;
            });
        }

        private int AddiSubi(ushort opcode, Func<uint, uint, OperandSize, long> getResult)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var immediateOperand = _memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, operandSize);

            var opcodeSize = INSTR_DEFAULT_SIZE + (operandSize == OperandSize.Byte ? 2 : (int)operandSize);
            var eaProps = _eaHelper.Get(opcode, operandSize, opcodeSize);

            long result = getResult(immediateOperand, eaProps.Operand, operandSize);

            // Storing.
            _memHelper.Write((uint)result, eaProps.Address, eaProps.Location, operandSize);

            return eaProps.InstructionSize;
        }

        // Add Quick.
        private int Addq(ushort opcode)
        {
            return AddqSubq(opcode, (uint srcOperand, uint destOperand, OperandSize operandSize, bool isAllowedToChangeFlags) =>
            {
                long result = srcOperand + destOperand;

                if (isAllowedToChangeFlags)
                {
                    _flagsHelper.AlterN((uint)result, operandSize);
                    _flagsHelper.AlterZ((uint)result, operandSize);
                    _flagsHelper.AlterV((uint)srcOperand, destOperand, result, operandSize);
                    _flagsHelper.AlterC(result, operandSize);
                    _regs.X = _regs.C;
                }

                return result;
            });
        }

        // Subtract Quick.
        private int Subq(ushort opcode)
        {
            return AddqSubq(opcode, (uint srcOperand, uint destOperand, OperandSize operandSize, bool isAllowedToChangeFlags) =>
            {
                long result = destOperand - srcOperand;

                if (isAllowedToChangeFlags)
                {
                    _flagsHelper.AlterN((uint)result, operandSize);
                    _flagsHelper.AlterZ((uint)result, operandSize);
                    _flagsHelper.AlterVSub(destOperand, srcOperand, result, operandSize);
                    _flagsHelper.AlterCSub(destOperand, srcOperand, operandSize);
                    _regs.X = _regs.C;
                }

                return result;
            });
        }

        private int AddqSubq(ushort opcode, Func<uint, uint, OperandSize, bool, long> getResult)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var dataField = (opcode >> 9) & 0x7;
            var immediateOperand = dataField == 0 ? 8 : dataField;

            var eaProps = _eaHelper.Get(opcode, operandSize);
            bool isDestinationAddressRegister = eaProps.Location == StoreLocation.AddressRegister;
            if (isDestinationAddressRegister)
            {
                // The entire destination address register is used regardless of the operation size.
                //operandSize = OperandSize.Long;
                eaProps = _eaHelper.Get(opcode, OperandSize.Long);
            }

            long result = getResult((uint)immediateOperand, eaProps.Operand, operandSize, !isDestinationAddressRegister);

            // Storing.
            _memHelper.Write((uint)result, eaProps.Address, eaProps.Location, operandSize);

            return eaProps.InstructionSize;
        }

        // Add Extended.
        private int Addx(ushort opcode)
        {
            return AddxSubx(opcode, (uint srcOperand, uint destOperand, OperandSize operandSize) =>
            {
                uint extendValue = (uint)(_regs.X ? 1 : 0);
                long result = (long)srcOperand + destOperand + extendValue;

                _flagsHelper.AlterN((uint)result, operandSize);
                _flagsHelper.AlterZ((uint)result, operandSize, doNotChangeIfZero: true);
                _flagsHelper.AlterV(destOperand, srcOperand, result, operandSize);
                _flagsHelper.AlterC(result, operandSize);
                _regs.X = _regs.C;

                return result;
            });
        }

        // Subtract Extended.
        private int Subx(ushort opcode)
        {
            return AddxSubx(opcode, (uint srcOperand, uint destOperand, OperandSize operandSize) =>
            {
                uint extendValue = (uint)(_regs.X ? 1 : 0);
                long result = destOperand - srcOperand - extendValue;

                _flagsHelper.AlterN((uint)result, operandSize);
                _flagsHelper.AlterZ((uint)result, operandSize, doNotChangeIfZero: true);
                _flagsHelper.AlterVSub(destOperand, srcOperand, result, operandSize);
                _flagsHelper.AlterCSub(destOperand, srcOperand + extendValue, operandSize);
                _regs.X = _regs.C;

                return result;
            });
        }

        private int AddxSubx(ushort opcode, Func<uint, uint, OperandSize, long> getResult)
        {
            var eaMode = ((opcode >> 3) & 0x1) == 0 ? EAMode.DataDirect : EAMode.PredecIndirect;
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var srcRegIdx = (uint)(opcode & 0x7);
            var srcRegProps = _eaHelper.Get(eaMode, (int)srcRegIdx, operandSize);

            var destRegIdx = (uint)((opcode >> 9) & 0x7);
            var destRegProps = _eaHelper.Get(eaMode, (int)destRegIdx, operandSize);

            long result = getResult(srcRegProps.Operand, destRegProps.Operand, operandSize);

            _memHelper.Write((uint)result, destRegProps.Address, destRegProps.Location, operandSize);

            return destRegProps.InstructionSize;
        }

        private int Mulu(ushort opcode)
        {
            var destRegIdx = (uint)((opcode >> 9) & 0x7);
            uint destReg = _memHelper.Read(destRegIdx, StoreLocation.DataRegister, OperandSize.Word);
            var eaProps = _eaHelper.Get(opcode, OperandSize.Word);

            uint result = destReg * eaProps.Operand;

            _memHelper.Write(result, destRegIdx, StoreLocation.DataRegister, OperandSize.Long);

            _flagsHelper.AlterZ(result, OperandSize.Long);
            _flagsHelper.AlterN(result, OperandSize.Long);
            _regs.V = false;
            _regs.C = false;

            return eaProps.InstructionSize;
        }

        private int Muls(ushort opcode)
        {
            var destRegIdx = (uint)((opcode >> 9) & 0x7);
            int destReg = (int)_memHelper.Read(destRegIdx, StoreLocation.DataRegister, OperandSize.Word, signExtended: true);
            var eaProps = _eaHelper.Get(opcode, OperandSize.Word, signExtended: true);

            int result = destReg * (short)eaProps.Operand;

            _memHelper.Write((uint)result, destRegIdx, StoreLocation.DataRegister, OperandSize.Long);

            _flagsHelper.AlterZ((uint)result, OperandSize.Long);
            _flagsHelper.AlterN((uint)result, OperandSize.Long);
            _regs.V = false;
            _regs.C = false;

            return eaProps.InstructionSize;
        }

        // Clear an Operand.
        private int Clr(ushort opcode)
        {
            _regs.N = false;
            _regs.Z = true;
            _regs.V = false;
            _regs.C = false;

            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var eaProps = _eaHelper.Get(opcode, operandSize);

            _memHelper.Write(0, eaProps.Address, eaProps.Location, operandSize);

            return eaProps.InstructionSize;
        }

        private void Compare(uint dest, uint src, OperandSize operandSize)
        {
            long result = (long)dest - src;

            _flagsHelper.AlterN((uint)result, operandSize);
            _flagsHelper.AlterZ((uint)result, operandSize);
            _flagsHelper.AlterVCmp(dest, src, (int)result, operandSize);
            _flagsHelper.AlterC(result, operandSize);
        }

        // Compare.
        private int Cmp(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var dataRegIdx = (uint)((opcode >> 9) & 0x7);
            var dataReg = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, operandSize);

            var eaProps = _eaHelper.Get(opcode, operandSize);

            Compare(dataReg, eaProps.Operand, operandSize);

            return eaProps.InstructionSize;
        }

        // Compare Address.
        private int Cmpa(ushort opcode)
        {
            OperandSize operandSize;
            switch ((opcode >> 6) & 0x7)
            {
                case 0x3:
                    operandSize = OperandSize.Word;
                    break;
                case 0x7:
                    operandSize = OperandSize.Long;
                    break;
                default:
                    throw new InvalidOperationException("The given operation size is unknown.");

            }

            var addrRegIdx = (uint)((opcode >> 9) & 0x7);
            // The entire destination address register is used regardless of the operation size.
            var addrReg = _memHelper.Read(addrRegIdx, StoreLocation.AddressRegister, OperandSize.Word, signExtended: true);

            var eaProps = _eaHelper.Get(opcode, operandSize, signExtended: true);

            Compare(addrReg, eaProps.Operand, operandSize);

            return eaProps.InstructionSize;
        }

        // Compare Immediate.
        private int Cmpi(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);

            var immediateOperand = (int)_memHelper.Read(_regs.PC + INSTR_DEFAULT_SIZE, StoreLocation.ImmediateData, operandSize);

            var opcodeSize = INSTR_DEFAULT_SIZE + (operandSize == OperandSize.Byte ? 2 : (int)operandSize);
            var eaProps = _eaHelper.Get(opcode, operandSize, opcodeSize);

            Compare(eaProps.Operand, (uint)immediateOperand, operandSize);

            return eaProps.InstructionSize;
        }


        // Compare Memory.
        private int Cmpm(ushort opcode)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var eaMode = EAMode.PostincIndirect;

            var srcOperandIdx = opcode & 0x7;
            var srcOperand = _eaHelper.Get(eaMode, srcOperandIdx, operandSize);

            var destOperandIdx = (opcode >> 9) & 0x7;
            var destOperand = _eaHelper.Get(eaMode, destOperandIdx, operandSize);

            Compare(destOperand.Operand, srcOperand.Operand, operandSize);

            return INSTR_DEFAULT_SIZE;
        }

        // Signed Divide.
        private int Divs(ushort opcode)
        {
            uint dataRegIdx = (uint)((opcode >> 9) & 0x7);
            int dividend = (int)_memHelper.Read(dataRegIdx, StoreLocation.DataRegister, OperandSize.Long, signExtended: true);

            var eaProps = _eaHelper.Get(opcode, OperandSize.Word, signExtended: true);
            int divisor = (int)eaProps.Operand;
            if (divisor == 0)
            {
                throw new InvalidOperationException("Divisor cannot be zero");
            }

            int quotient = dividend / divisor;
            int remainder = dividend % divisor;
            uint result = ((uint)remainder << 16) | ((uint)quotient & 0xFFFF);

            _regs.V = quotient > 0x8000 || quotient < -0x8000;
            _regs.C = false;

            if (!_regs.V)
            {
                _flagsHelper.AlterN((uint)quotient, OperandSize.Word);
                _flagsHelper.AlterZ((uint)quotient, OperandSize.Word);

                _memHelper.Write(result, dataRegIdx, StoreLocation.DataRegister, OperandSize.Long);
            }

            return eaProps.InstructionSize;
        }

        // Unsigned Divide.
        private int Divu(ushort opcode)
        {
            uint dataRegIdx = (uint)((opcode >> 9) & 0x7);
            uint dividend = _memHelper.Read(dataRegIdx, StoreLocation.DataRegister, OperandSize.Long);

            var eaProps = _eaHelper.Get(opcode, OperandSize.Word);
            uint divisor = eaProps.Operand;
            if (divisor == 0)
            {
                throw new InvalidOperationException("Divisor cannot be zero");
            }

            uint quotient = dividend / divisor;
            uint remainder = dividend % divisor;
            uint result = (remainder << 16) | (quotient & 0xFFFF);

            _regs.V = quotient > 0xFFFF;
            _regs.C = false;

            if (!_regs.V)
            {
                _flagsHelper.AlterN(quotient, OperandSize.Word);
                _flagsHelper.AlterZ(quotient, OperandSize.Word);

                _memHelper.Write(result, dataRegIdx, StoreLocation.DataRegister, OperandSize.Long);
            }

            return eaProps.InstructionSize;
        }

        // Sign-Extend.
        private int Ext(ushort opcode)
        {
            OperandSize regSize;
            OperandSize extSize;

            var mode = (opcode >> 6) & 0x7;
            switch (mode)
            {
                case 2:
                    regSize = OperandSize.Byte;
                    extSize = OperandSize.Word;
                    break;
                case 3:
                    regSize = OperandSize.Word;
                    extSize = OperandSize.Long;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var regIdx = (uint)(opcode & 0x7);
            var result = _memHelper.Read(regIdx, StoreLocation.DataRegister, regSize, signExtended: true);
            _memHelper.Write(result, regIdx, StoreLocation.DataRegister, extSize);

            _flagsHelper.AlterN(result, regSize);
            _flagsHelper.AlterZ(result, regSize);
            _regs.C = _regs.V = false;

            return INSTR_DEFAULT_SIZE;
        }

        // Negate, Negate with Extend.
        private int Neg(ushort opcode, bool includeExtend)
        {
            var operandSize = (OperandSize)Math.Pow(2, (opcode >> 6) & 0x3);
            var eaProps = _eaHelper.Get(opcode, operandSize);

            var extendFlagValue = includeExtend
                ? (_regs.X ? 1 : 0)
                : 0;
            uint result = 0 - eaProps.Operand - (uint)extendFlagValue;

            _memHelper.Write(result, eaProps.Address, eaProps.Location, operandSize);

            _flagsHelper.AlterN(result, operandSize);
            var isDestOperandNeg = ((eaProps.Operand >> ((int)operandSize*8 - 1)) & 0x1) == 1;
            var isResultNeg = ((result >> ((int)operandSize*8 - 1)) & 0x1) == 1;
            _regs.V = isDestOperandNeg && isResultNeg;
            if (includeExtend)
            {
                _flagsHelper.AlterZ(result, operandSize, doNotChangeIfZero: true);
                _regs.X = _regs.C = 0 < (eaProps.Operand + extendFlagValue);
            }
            else
            {
                _flagsHelper.AlterZ(result, operandSize);
                _regs.X = _regs.C = result != 0;
            }

            return eaProps.InstructionSize;
        }
    }
}
