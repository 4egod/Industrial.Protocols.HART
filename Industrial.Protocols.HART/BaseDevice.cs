
namespace Industrial.Protocols.HART
{
    using Industrial.IO;
    using System.Threading;

    public abstract class BaseDevice
    {
        public BaseDevice(IPort port, byte pollingAddress)
        {
            Port = port;
            PollingAddress = pollingAddress;
        }

        protected IPort Port { get; set; }

        public byte PollingAddress { get; private set; }

        public abstract byte PreamblesCount { get; }

        public void ExecuteCommand(BaseCommand command)
        {
            byte[] cmd = command.GetBytes();

            Port.Flush();
            //for (int i = 0; i < PreamblesCount; i++)
            //{
            //    Port.Write(cmd, i, 1);
            //    Thread.Sleep(10);
            //}

            //Port.Write(cmd, PreamblesCount - 1, cmd.Length - PreamblesCount + 1);
            Port.Write(cmd, 0, cmd.Length);
            Thread.Sleep(10);

            byte[] buf = new byte[255];

            for (int i = 0; i < buf.Length; i++)
            {
                Port.Read(buf, i, 1);

                if (buf[i] == 255)
                {
                    continue;
                }

                if (buf[i] == BaseCommand.SlaveToMasterStartDelimiterShortFrame)
                {
                    buf[0] = buf[i];
                    Port.Read(buf, 1, 3);
                    Port.Read(buf, 4, buf[3] + 1);
                    break;
                }

                if (buf[i] == BaseCommand.SlaveToMasterStartDelimiterLongFrame)
                {
                    buf[0] = buf[i];
                    Port.Read(buf, 1, 7);
                    Port.Read(buf, 8, buf[7] + 1);
                    break;
                }

                throw new InvalidResponseException();
            }


            command.FromBytes(buf);
        }
    }
}
