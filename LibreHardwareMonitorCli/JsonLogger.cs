using System;
using System.Collections.Generic;
using System.Globalization;
using LibreHardwareMonitor.Hardware;

namespace LibreHardwareMonitorCli
{
    class JsonLogger : IVisitor
    {
        Dictionary<Identifier, string> LogItems = new Dictionary<Identifier, string>();

        public void Log(IComputer computer)
        {
            LogItems.Clear();
            computer.Accept(this);
            foreach (var item in LogItems)
            {
                Console.WriteLine($"{{{item.Value}}}");
            }
        }
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            LogItems[hardware.Identifier] = $"\"time\":\"{DateTimeOffset.UtcNow.ToString("u")}\",\"hardware.type\":\"{hardware.HardwareType}\",\"hardware.id\":\"{hardware.Identifier}\",\"hardware.name\":\"{hardware.Name}\"";
            hardware.Traverse(this);
        }

        public void VisitSensor(ISensor sensor)
        {
            var value = sensor.Value.HasValue ? sensor.Value.Value.ToString("R", CultureInfo.InvariantCulture) : null;
            LogItems[sensor.Hardware.Identifier] += $",\"sensor.{sensor.SensorType}.{sensor.Name}\":{value}";
        }

        public void VisitParameter(IParameter parameter)
        {
        }
    }
}
