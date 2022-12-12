using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using CommonHelpers;

namespace FanControl
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Instance.WithSentry(() =>
            {
                ApplicationConfiguration.Initialize();
                Application.Run(new FanControlForm());
            });
        }
    }
}
