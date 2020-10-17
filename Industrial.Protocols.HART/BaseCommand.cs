using System;
using Mx;

namespace Industrial.Protocols.HART
{
    public abstract class BaseCommand
    {
        public const byte MasterToSlaveStartDelimiterShortFrame = 0x02;

        public const byte SlaveToMasterStartDelimiterShortFrame = 0x06;

        public const byte MasterToSlaveStartDelimiterLongFrame = 0x82;

        public const byte SlaveToMasterStartDelimiterLongFrame = 0x86;

        public BaseCommand(byte preamblesCount, FrameType frameType)
        {
            PreamblesCount = preamblesCount;
            FrameType = frameType;
        }

        public abstract byte CommandIndex { get; }

        public byte PreamblesCount { get; private set; }

        public FrameType FrameType { get; private set; }

        /// <summary>
        /// Address for short frame
        /// </summary>
        public PollingAddress PollingAddress { get; protected set; }

        /// <summary>
        /// Address for Long frame (HART protocol v5)
        /// </summary>
        public Address Address { get; protected set; }

        public byte[] ResponseStatus { get; private set; }

        public byte[] Data { get; set; }

        public byte DataLength
        {
            get
            {
                if (Data == null)
                {
                    return 0;
                }
                else
                {
                    return (byte)Data.Length;
                }
            }
        }

        public byte Checksum { get; private set; }

        public byte[] GetBytes(params object[] objects)
        {
            int length = PreamblesCount + DataLength + 4;
            byte[] result;

            switch (FrameType)
            {
                case FrameType.ShortFrame:
                    {
                        length += 1;
                        result = new byte[length];
                        result[PreamblesCount] = MasterToSlaveStartDelimiterShortFrame;
                        result[PreamblesCount + 1] = PollingAddress.GetBytes()[0];
                        result[PreamblesCount + 2] = CommandIndex;
                        result[PreamblesCount + 3] = DataLength;
                        if (DataLength > 0)
                        {
                            Data.CopyTo(result, PreamblesCount + 4);
                        }
                        break;
                    }

                case FrameType.LongFrame:
                    {
                        length += Address.Length;
                        result = new byte[length];
                        result[PreamblesCount] = MasterToSlaveStartDelimiterLongFrame;
                        Address.GetBytes().CopyTo(result, PreamblesCount + 1);
                        result[PreamblesCount + 6] = CommandIndex;
                        result[PreamblesCount + 7] = DataLength;
                        if (DataLength > 0)
                        {
                            Data.CopyTo(result, PreamblesCount + 8);
                        }
                        break;
                    }

                default: throw new ArgumentException();
            }

            for (int i = 0; i < PreamblesCount; ++i)
            {
                result[i] = 255;
            }

            result[length - 1] = CrcXor.Calculate(result, PreamblesCount, length - PreamblesCount - 1);

            return result;
        }

        public void FromBytes(byte[] buffer, int offset = 0, params object[] objects)
        {
            byte[] result;

            switch (FrameType)
            {
                case FrameType.ShortFrame:
                    {
                        Data = null;

                        if (buffer[0] != SlaveToMasterStartDelimiterShortFrame ||
                            buffer[1] != PollingAddress.GetBytes()[0] ||
                            buffer[2] != CommandIndex)
                        {
                            throw new InvalidResponseException();
                        }

                        if (buffer[buffer[3] + 4] != CrcXor.Calculate(buffer, 0, buffer[3] + 4))
                        {
                            throw new InvalidChecksumException();
                        }

                        result = new byte[buffer[3] + 5];
                        Array.Copy(buffer, 0, result, 0, result.Length);

                        ResponseStatus = new byte[2];
                        Array.Copy(result, 4, ResponseStatus, 0, ResponseStatus.Length);

                        Data = new byte[result[3] - 2];
                        Array.Copy(result, 6, Data, 0, Data.Length);
                        break;
                    }

                case FrameType.LongFrame:
                    {
                        Data = null;

                        byte[] buf = Address.GetBytes();
                        if (buffer[0] != SlaveToMasterStartDelimiterLongFrame ||
                            buffer[1] != buf[0] ||
                            buffer[2] != buf[1] ||
                            buffer[3] != buf[2] ||
                            buffer[4] != buf[3] ||
                            buffer[5] != buf[4] ||
                            buffer[6] != CommandIndex)
                        {
                            throw new InvalidResponseException();
                        }

                        if (buffer[buffer[7] + 8] != CrcXor.Calculate(buffer, 0, buffer[7] + 8))
                        {
                            throw new InvalidChecksumException();
                        }

                        result = new byte[buffer[7] + 9];
                        Array.Copy(buffer, 0, result, 0, result.Length);

                        ResponseStatus = new byte[2];
                        Array.Copy(result, 8, ResponseStatus, 0, ResponseStatus.Length);

                        Data = new byte[result[7] - 2];
                        Array.Copy(result, 10, Data, 0, Data.Length);
                        break;
                    }

                default: return;
            }
        }
    }
}
