
namespace Industrial.Protocols.HART
{
    using System;

    public class PollingAddress
    {
        private byte _pollingAddress;

        public const int Length = 1;

        public PollingAddress()
        {
        }

        public PollingAddress(byte address)
        {
            if (address > 15 || address < 0) throw new ArgumentOutOfRangeException();

            _pollingAddress = address;
        }

        public byte Value { get { return _pollingAddress; } }

        public byte[] GetBytes(params object[] objects)
        {
            //TODO CHECK THIS! This is for HyperFlow
            return new byte[] { _pollingAddress };
            //return new byte[1] { (byte)((_pollingAddress | 0x80) & 0x8F) };
        }

        public void FromBytes(byte[] buffer, int offset, params object[] objects)
        {
            _pollingAddress = (byte)(buffer[offset] & 0x7F);
        }
    }
}
