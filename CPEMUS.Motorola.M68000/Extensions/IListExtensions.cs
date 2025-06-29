namespace CPEMUS.Motorola.M68000.Extensions
{
    public static class IListExtensions
    {
        public static uint ReadByte(this IList<byte> arr, uint idx)
        {
            return arr[(int)idx];
        }

        public static uint ReadWord(this IList<byte> arr, uint idx)
        {
            return (uint)((arr[(int)idx] << 8) | arr[(int)(idx + 1)]);
        }

        public static uint ReadLong(this IList<byte> arr, uint idx)
        {
            return (uint)((arr[(int)idx] << 24) | (arr[(int)(idx + 1)] << 16)
                | (arr[(int)(idx + 2)] << 8) | arr[(int)(idx + 3)]);
        }

        public static uint Read(this IList<byte> arr, uint idx, OperandSize operandSize)
        {
            switch (operandSize)
            {
                case OperandSize.Byte:
                    return ReadByte(arr, idx);
                case OperandSize.Word:
                    return ReadWord(arr, idx);
                case OperandSize.Long:
                    return ReadLong(arr, idx);
                default:
                    throw new InvalidOperationException("Operand size type is unknown");
            }
        }

        public static void WriteByte(this IList<byte> arr, uint idx, uint value)
        {
            arr[(int)idx] = (byte)value;
        }

        public static void WriteWord(this IList<byte> arr, uint idx, uint value)
        {
            arr[(int)idx] = (byte)(value >> 8);
            arr[(int)(idx + 1)] = (byte)value;
        }

        public static void WriteLong(this IList<byte> arr, uint idx, uint value)
        {
            arr[(int)idx] = (byte)(value >> 24);
            arr[(int)(idx + 1)] = (byte)(value >> 16);
            arr[(int)(idx + 2)] = (byte)(value >> 8);
            arr[(int)(idx + 3)] = (byte)value;
        }

        public static void Write(this IList<byte> arr, uint idx, uint value, OperandSize operandSize)
        {
            switch (operandSize)
            {
                case OperandSize.Byte:
                    WriteByte(arr, idx, value);
                    break;
                case OperandSize.Word:
                    WriteWord(arr, idx, value);
                    break;
                case OperandSize.Long:
                    WriteLong(arr, idx, value);
                    break;
            }
        }
    }
}
