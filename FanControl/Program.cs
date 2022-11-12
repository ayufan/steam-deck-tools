using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;

namespace FanControl
{ 
    internal class Program
    {
        static void Main(string[] args)
        {
            Application.Run(new FanControlForm());
        }
    }
}
