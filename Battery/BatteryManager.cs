using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Battery
{
    public class BatteryManager
    {
        /// <summary>
        ///     Lenth of timeout string in system
        /// </summary>
        private const int TimeLen = 10;

        public int PrevTimeout { get; set; }
        public string State { get; set; }
        public string Percentage { get; set; }
        public string TimeLeft { get; set; }
        public string PrevState { get; set; }

        public bool OnLaunch { get; set; }


        /// <summary>
        ///     Init object of this class
        /// </summary>
        public void Init()
        {
            OnLaunch = true;
            PrevTimeout = GetCurrentTimeout();
            UpdateState();
        }

        /// <summary>
        ///     Get screen timeout from system
        /// </summary>
        /// <returns></returns>
        public int GetCurrentTimeout()
        {
            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c powercfg /q";
            p.Start();
            var powercfgOut = p.StandardOutput.ReadToEnd();
            var reg = new Regex("VIDEOIDLE.*\\n.*\\n.*\\n.*\\n.*\\n.*\\n.*");
            var videoidle = reg.Match(powercfgOut).Value;
            var batteryIdle = videoidle.Substring(videoidle.Length - 1 - TimeLen).TrimEnd();
            return Convert.ToInt32(batteryIdle, 16) / 60;
        }

        /// <summary>
        ///     Update this object state
        /// </summary>
        public void UpdateState()
        {
            State = SystemInformation.PowerStatus.PowerLineStatus.ToString();
            if (OnLaunch)
            {
                PrevState = State;
                OnLaunch = false;
            }
            Percentage = SystemInformation.PowerStatus.BatteryLifePercent * 100 + @"%";
            if (State == "Offline")
            {
                var temp = SystemInformation.PowerStatus.BatteryLifeRemaining;
                if (temp != -1)
                {
                    var span = new TimeSpan(0, 0, temp);
                    TimeLeft = span.ToString("g");
                }
                else TimeLeft = @"Calculating....";
            }
            else
            {
                TimeLeft = @"Device is connected to AC.";
            }
        }

        /// <summary>
        ///     Set screen timeout for battery
        /// </summary>
        /// <param name="value">Timeout in minutes</param>
        public void SetTimeout(int value)
        {
            var p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.Arguments = "/c powercfg /x -monitor-timeout-dc " + value;
            p.Start();
        }
    }
}