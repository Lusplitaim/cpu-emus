using System.Collections;

namespace CPEMUS.Motorola.M68000.ConsoleTests
{
    internal class MemTest : IList<byte>
    {
        private readonly IList<byte> _mem;
        private readonly Dictionary<int, int> _memMapper;
        public MemTest(List<List<uint>> ram)
        {
            _mem = new byte[ram.Count];

            _memMapper = new();
            ram.Sort((e1, e2) =>
            {
                // Comparing addresses.
                if (e1[0] < e2[0])
                {
                    return -1;
                }
                else if (e1[0] == e2[0])
                {
                    return 0;
                }

                return 1;
            });

            for (int i = 0; i < ram.Count; i++)
            {
                var ramAddress = ram[i][0];
                _memMapper.Add((int)ramAddress, i);
                var ramValue = (byte)ram[i][1];
                _mem[i] = ramValue;
            }
        }

        public byte this[int index]
        {
            get
            {
                if (!_memMapper.TryGetValue(index, out int addr))
                {
                    throw new IndexOutOfRangeException();
                }
                return _mem[addr];
            }
            set
            {
                if (!_memMapper.TryGetValue(index, out int addr))
                {
                    throw new IndexOutOfRangeException();
                }
                _mem[addr] = value;
            }
        }

        public int Count => _mem.Count;

        public bool IsReadOnly => _mem.IsReadOnly;

        public IEnumerator<byte> GetEnumerator()
        {
            foreach (var addr in _memMapper)
            {
                yield return _mem[addr.Value];
            }
        }

        #region Not necessary.
        public void Add(byte item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(byte item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(byte[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(byte item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, byte item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(byte item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
