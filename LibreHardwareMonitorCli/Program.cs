using System;
using System.Threading;
using LibreHardwareMonitor.Hardware;

namespace LibreHardwareMonitorCli
{
    class Program
    {
        const long LOG_PERIOD = 60000;

        static void Main(string[] args)
        {
            var computer = new Computer()
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true,
            };

            var updater = new Updater();
            var logger = new JsonLogger();

            computer.Open();
            computer.Accept(updater);

            while (true)
            {
                var wait = LOG_PERIOD - DateTimeOffset.Now.ToUnixTimeMilliseconds() % LOG_PERIOD;
                Thread.Sleep((int)wait);
                computer.Accept(updater);
                logger.Log(computer);
            }
        }
    }
}
