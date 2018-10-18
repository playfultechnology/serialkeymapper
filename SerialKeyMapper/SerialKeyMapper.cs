using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;

namespace Playful.Technology
{
    /// <summary>
    ///  Listens for a line of delimited data on the specified serial port (e.g. as sent from
    ///  the Arduino Serial.println() method), and converts this to a sequence of keystrokes 
    ///  which are sent to the currently active application
    /// </summary>
    public partial class SerialKeyMapper : Form
    {
        // List of available baud rates at which serial port can be connected
        private int[] bauds = {9600, 14400, 19200, 38400, 57600, 115200};
        // Is the application currently listening to the serial port
        private bool isRunning = false;
        // The port in use
        private SerialPort port = null;

        public SerialKeyMapper()
        {
            InitializeComponent();

            // Populate combo boxes and other form elements with appropriate values
            Populate();

            // Initialise the serial port with some common values
            port = new SerialPort();
            port.ReadTimeout = 500;
            port.WriteTimeout = 500;

            // Arduino println sends a string terminated with \r\n 
            port.NewLine = "\r\n";

            // Attach event handlers
            this.chkOnOff.CheckedChanged += new EventHandler(ChkOnOff_CheckedChanged_Handler);
            this.FormClosing += new FormClosingEventHandler(FormClosing_Handler);
            this.bgWorker.DoWork += new DoWorkEventHandler(BackgroundWork_Handler);
            this.bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(StartStop);
        }

        // Called when the application is closed
        void FormClosing_Handler(object sender, FormClosingEventArgs e)
        {
            if (port.IsOpen) {
                port.Close();
            }
            chkOnOff.Checked = false;
        }

        // Called when the option for enabling/disabling the application is toggled
        void ChkOnOff_CheckedChanged_Handler(object sender, EventArgs e) {
            if (isRunning) {
                isRunning = false;
            }
            else {
                StartStop(null, null);
            }

            ToggleFormControlsActive(!chkOnOff.Checked);
        }

        // Sets whether form controls are enabled or not
        private void ToggleFormControlsActive(bool enable) {
            cboBaudRate.Enabled = cboPort.Enabled = enable;
        }

        // Populates the various form controls with options
        private void Populate() {
            // COM Port combo
            cboPort.Items.Clear();
            string[] portList = SerialPort.GetPortNames();
            foreach (string port in portList) {
                // Some guides suggest that to address COM ports greater than "COM9", you must use NT address format "\\.\COM10", "\\.\COM11" etc.
                // This could be achieved using the following, but doesn't actually appear to be necessary:
                // string portNTformat = string.Concat(@"\\.\", port);
                string portNTformat = port; 
                cboPort.Items.Add(portNTformat);
            }

            // Baud rate combo
            cboBaudRate.DataSource = bauds;
            cboBaudRate.SelectedIndex = 0;
        }

        private void StartStop(object sender, RunWorkerCompletedEventArgs args) {
            ToggleFormControlsActive(!chkOnOff.Checked);

            // If port is already open, close it
            if (port.IsOpen) {
                port.Close();
            }
           
            // If we've just started listening
            if (chkOnOff.Checked)  {
                if (cboPort.SelectedItem != null) {
                    try {
                        port = new SerialPort((string)cboPort.SelectedItem, (int)cboBaudRate.SelectedItem);
                        port.Open();
                        isRunning = true;
                        bgWorker.RunWorkerAsync();
                    }
                    catch(Exception e) {
                        isRunning = false;
                        chkOnOff.Checked = false;
                        ToggleFormControlsActive(true);
                    }
                }
            }
        }

        // Background handler actually does the work of listening to the serial
        // port and converting to keystrokes
        private void BackgroundWork_Handler(object sender, DoWorkEventArgs args) {
            while (isRunning) {
                try {
                    var line = port.ReadLine();
                    if (chkStripWhitespace.Checked) {
                        // Strip any whitespace, newline, carriage returns etc.
                        line = Regex.Replace(line, @"\s+", string.Empty);
                    }
                    SendKeys.SendWait(line);
                }
                catch (TimeoutException) { }
            }
        }

    }
}
