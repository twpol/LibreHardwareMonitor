// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.
// Partial Copyright (C) Michael Möller <mmoeller@openhardwaremonitor.org> and Contributors.
// All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace LibreHardwareMonitor.Utilities
{
    public class Logger
    {
        private const string FileNameFormat = "LibreHardwareMonitorLog-{0:yyyy-MM-dd}.json";

        private readonly IComputer _computer;

        private DateTime _day = DateTime.MinValue;
        private string _fileName;
        private List<ISensor> _sensors = new List<ISensor>();
        private DateTime _lastLoggedTime = DateTime.MinValue;

        public Logger(IComputer computer)
        {
            _computer = computer;
            _computer.HardwareAdded += HardwareAdded;
            _computer.HardwareRemoved += HardwareRemoved;
        }

        private void HardwareRemoved(IHardware hardware)
        {
            hardware.SensorAdded -= SensorAdded;
            hardware.SensorRemoved -= SensorRemoved;

            foreach (ISensor sensor in hardware.Sensors)
                SensorRemoved(sensor);

            foreach (IHardware subHardware in hardware.SubHardware)
                HardwareRemoved(subHardware);
        }

        private void HardwareAdded(IHardware hardware)
        {
            foreach (ISensor sensor in hardware.Sensors)
                SensorAdded(sensor);

            hardware.SensorAdded += SensorAdded;
            hardware.SensorRemoved += SensorRemoved;

            foreach (IHardware subHardware in hardware.SubHardware)
                HardwareAdded(subHardware);
        }

        private void SensorAdded(ISensor sensor)
        {
            _sensors.Add(sensor);
        }

        private void SensorRemoved(ISensor sensor)
        {
            _sensors.Remove(sensor);
        }

        private static string GetFileName(DateTime date)
        {
            return AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + string.Format(FileNameFormat, date);
        }

        private bool OpenExistingLogFile()
        {
            return File.Exists(_fileName);
        }

        private void CreateNewLogFile()
        {
            using (StreamWriter writer = new StreamWriter(_fileName, false))
            {
            }
        }

        public TimeSpan LoggingInterval { get; set; }

        public void Log()
        {
            DateTime now = DateTime.Now;

            if (_lastLoggedTime + LoggingInterval - new TimeSpan(5000000) > now)
                return;

            if (_day != now.Date || !File.Exists(_fileName))
            {
                _day = now.Date;
                _fileName = GetFileName(_day);

                if (!OpenExistingLogFile())
                    CreateNewLogFile();
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(_fileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
                {
                    writer.Write($"{{\"time\":\"{DateTimeOffset.UtcNow.ToString("u")}\"");
                    foreach (var sensor in _sensors)
                    {
                        writer.Write($",\"{sensor.Identifier}\":");
                        if (sensor != null && sensor.Value.HasValue)
                        {
                            writer.Write(sensor.Value.Value.ToString("R", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            writer.Write("null");
                        }
                    }
                    writer.WriteLine("}");
                }
            }
            catch (IOException) { }

            _lastLoggedTime = now;
        }
    }
}
