
namespace Industrial.Protocols.HART
{
    using System;

    public class Address
    {
        private byte _manufacturerIdentificationCode;
        private byte _manufacturerDeviceTypeCode;
        private byte[] _deviceId;

        public const int Length = 5;

        public Address()
        {
            this.DeviceId = new byte[3];
        }

        public byte ManufacturerIdentificationCode
        {
            get { return _manufacturerIdentificationCode; }

            set { _manufacturerIdentificationCode = value; }
        }

        public byte ManufacturerDeviceTypeCode
        {
            get { return _manufacturerDeviceTypeCode; }

            set { _manufacturerDeviceTypeCode = value; }
        }

        public byte[] DeviceId
        {
            get { return _deviceId; }

            set
            {
                if (value.Length != 3) throw new ArgumentException();

                _deviceId = value;
            }
        }

        public byte[] GetBytes(params object[] objects)
        {
            byte[] res = new byte[Length];
            res[0] = (byte)((ManufacturerIdentificationCode | 0x80) & 0xBF);
            res[1] = ManufacturerDeviceTypeCode;
            res[2] = DeviceId[0];
            res[3] = DeviceId[1];
            res[4] = DeviceId[2];
            return res;
        }

        public void FromBytes(byte[] buffer, int offset, params object[] objects)
        {
            ManufacturerIdentificationCode = (byte)(buffer[offset] & 0x3F);
            ManufacturerDeviceTypeCode = buffer[offset + 1];
            byte[] buf = new byte[3];
            buf[0] = buffer[offset + 2];
            buf[1] = buffer[offset + 3];
            buf[2] = buffer[offset + 4];
            DeviceId = buf;
        }

        public override string ToString()
        {
            return GetBytes().ToHex("-");
        }
    }
}
