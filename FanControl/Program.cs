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
            if (!Vlv0100.IsSupported())
            {
                String message = "";
                message += "Current device is not supported.\n";
                message += "FirmwareVersion: " + Vlv0100.GetFirmwareVersion().ToString("X") + "\n";
                message += "BoardID: " + Vlv0100.GetBoardID().ToString("X") + "\n";
                message += "PDCS: " + Vlv0100.GetPDCS().ToString("X") + "\n";

                String title = "Steam Deck Fan Control v" + Application.ProductVersion.ToString();
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FanControlForm());
        }
    }
}
