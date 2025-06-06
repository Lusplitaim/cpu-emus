namespace CPEMUS.Motorola.M68000.Extensions
{
    public static class ByteArrayExtensions
    {
        public static int ReadByte(this byte[] arr, int idx)
        {
            return arr[idx];
        }

        public static int ReadSignExtByte(this byte[] arr, int idx)
        {
            return (sbyte)arr[idx];
        }

        public static int ReadWord(this byte[] arr, int idx)
        {
            return (arr[idx] << 8) | arr[idx + 1];
        }

        public static int ReadSignExtWord(this byte[] arr, int idx)
        {
            return (short)((arr[idx] << 8) | arr[idx + 1]);
        }

        public static int ReadLong(this byte[] arr, int idx)
        {
            return (arr[idx] << 24) | (arr[idx + 1] << 16) | (arr[idx + 2] << 8) | arr[idx + 3];
        }

        public static int Read(this byte[] arr, int idx, OperandSize operandSize)
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

        public static void WriteByte(this byte[] arr, int idx, int value)
        {
            arr[idx] = (byte)value;
        }

        public static void WriteWord(this byte[] arr, int idx, int value)
        {
            arr[idx] = (byte)(value >> 8);
            arr[idx + 1] = (byte)value;
        }

        public static void WriteLong(this byte[] arr, int idx, int value)
        {
            arr[idx] = (byte)(value >> 24);
            arr[idx + 1] = (byte)(value >> 16);
            arr[idx + 2] = (byte)(value >> 8);
            arr[idx + 3] = (byte)value;
        }

        public static void Write(this byte[] arr, int idx, int value, OperandSize operandSize)
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
