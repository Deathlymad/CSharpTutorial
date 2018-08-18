using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.

namespace Test
{
    class Program
    {
        const String POWER_MULTIPLIERS = ".kMGTPEZY";
        System.Text.RegularExpressions.Regex reactorPowerRegex = new System.Text.RegularExpressions.Regex(
                            "Max Output: (\\d+\\.?\\d*) (\\w?)W.*Current Output: (\\d+\\.?\\d*) (\\w?)W",
                            System.Text.RegularExpressions.RegexOptions.Singleline);

        const double maxUranPerSec = 0.083333333333;
        const double uraniumDensity = 19100; //kg/m3

        const double progressBarLength = 17.0;

        //TIME CONSTANTS
        const int yearInSec = 31557600;
        const int weekInSec = 604800;
        const int dayInSec = 86400;
        const int hourInSec = 3600;
        const int minuteInSec = 60;

        List<IMyTextPanel> reactorMonitors;
        List<String> defaultScreenArray;
        IMyReactor reactor;

        double currentOutput = 0.0f;
        double maxOutput = 0.0f;
        double uranLeft = 0.0f;
        double maxUranContained = 0.0f;
        double currentConsumption = 0.0f;

        static char rgb(byte r, byte g, byte b)
        {  // in 0..7 range
            return (char)(0xe100 + (r << 6) + (g << 3) + b);
        }

        char emptyBar = rgb(1, 1, 1);
        char highBar = rgb(0, 7, 0);
        char mediumBar = rgb(7, 7, 0);
        char lowBar = rgb(7, 0, 0);

        void displayOnScreens(List<String> arr)
        {
            String builder = "";
            for (int i = 0; i < arr.Count; i++)
            {
                builder = builder + arr[i];
                builder = builder + "\n";
            }
            for (int i = 0; i < reactorMonitors.Count; i++)
            {
                reactorMonitors[i].WritePublicText(builder);
            }
        }

        void init()
        {
            List<IMyReactor> reactors = new List<IMyReactor>();
            GridTerminalSystem.GetBlocksOfType<IMyReactor>(reactors);
            for (int i = 0; i < reactors.Count; i++)
            {
                if (reactors[i].CustomData.Contains("stationReactor"))
                {
                    reactor = reactors[i];
                    break;
                }
            }

            reactorMonitors.Clear(); //Reset List

            //Pull all Panels
            List<IMyTextPanel> panels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(panels);

            //Find Marked Panels
            for (int i = 0; i < panels.Count; i++)
            {
                if (panels[i].CustomData.Contains("reactorMonitor"))
                {
                    reactorMonitors.Add(panels[i]);
                }
            }

            //Configure LCDs
            for (int i = 0; i < reactorMonitors.Count; i++)
            {
                reactorMonitors[i].SetValue<Int64>("Font", 1147350002); //Set Font to Monospace
                reactorMonitors[i].SetValue<Color>("FontColor", new Color(255, 255, 255)); // set preset font colour
                reactorMonitors[i].SetValue<Single>("FontSize", (Single)1); // set font size
                reactorMonitors[i].SetValue<Color>("BackgroundColor", new Color(0, 0, 0)); // set BG 
                reactorMonitors[i].ShowPublicTextOnScreen();
            }
        }

        void updateReactorData()
        {
            System.Text.RegularExpressions.Match match = reactorPowerRegex.Match(reactor.DetailedInfo);

            double parsedDouble = 0.0f;
            if (match.Success)
            {
                if (Double.TryParse(match.Groups[1].Value, out parsedDouble))
                    maxOutput = parsedDouble * Math.Pow(1000.0, POWER_MULTIPLIERS.IndexOf(match.Groups[2].Value));

                if (Double.TryParse(match.Groups[3].Value, out parsedDouble))
                    currentOutput = parsedDouble * Math.Pow(1000.0, POWER_MULTIPLIERS.IndexOf(match.Groups[4].Value));
            }

            IMyInventory inv = reactor.GetInventory();
            maxUranContained = (double)inv.MaxVolume * uraniumDensity;
            IMyInventoryItem uranium = inv.GetItems()[0];
            uranLeft = (double)uranium.Amount;

            double outputRatio = currentOutput / maxOutput;
            currentConsumption = maxUranPerSec * outputRatio;
        }

        String getProgressBar(double ratio)
        {
            String res = "[";
            char charCode;
            if (ratio < 0.33)
                charCode = lowBar;
            else if (ratio < 0.66)
                charCode = mediumBar;
            else if (ratio >= 0.66)
                charCode = highBar;
            else
                charCode = emptyBar;

            for (int calculatedRatio = (int)Math.Round(ratio * progressBarLength); calculatedRatio > 0; calculatedRatio--)
                res = res + charCode;

            int strSize = res.Length;
            while (strSize < progressBarLength)
            {
                res = res + emptyBar;
                strSize++;
            }

            res = res + "]";

            return res;
        }

        String getTimeLeft()
        {
            int seconds = (int)(uranLeft * currentConsumption * 1000000); //Unsure why the multiplier is necessary. probably bad conversion
            int years = 0;
            int weeks = 0;
            int days = 0;
            int hours = 0;
            int minutes = 0;

            while (seconds > yearInSec)
            {
                seconds -= yearInSec;
                years++;
            }
            while (seconds > weekInSec)
            {
                seconds -= weekInSec;
                weeks++;
            }
            while (seconds > dayInSec)
            {
                seconds -= dayInSec;
                days++;
            }
            while (seconds > hourInSec)
            {
                seconds -= hourInSec;
                hours++;
            }
            while (seconds > minuteInSec)
            {
                seconds -= minuteInSec;
                minutes++;
            }

            String str = "";

            if (years > 0)
                str += years.ToString() + "y";
            if (years > 0 || weeks > 0)
                str += weeks.ToString() + "w";
            if (years > 0 || weeks > 0 || days > 0)
                str += days.ToString() + "d";
            if (years > 0 || weeks > 0 || days > 0 || hours > 0)
                str += hours.ToString() + "h";
            if (years > 0 || weeks > 0 || days > 0 || hours > 0 || minutes > 0)
                str += minutes.ToString() + "m";

            str += seconds.ToString() + "s";
            return str;
        }

        String convertToValue(double power, String type)
        {
            int ctr = 0;
            while (power > 800)
            {
                power /= 1000;
                ctr++;
            }
            power = Math.Round(power, 2);

            return power.ToString() + " " + POWER_MULTIPLIERS[ctr] + type;
        }

        String padStringToCenter(String str)
        {
            int len = str.Length;
            if (len > 26)
                return str;
            else
            {
                int buffer = (26 - len) / 2;
                String buf = "";
                for (; buffer > 0; buffer--)
                {
                    buf = buf + " ";
                }
                return buf + str + buf;
            }
        }

        public Program()
        {
            reactorMonitors = new List<IMyTextPanel>();
            defaultScreenArray = new List<String>();

            defaultScreenArray.Add("==========================");
            defaultScreenArray.Add("                          ");
            defaultScreenArray.Add("       <<REACTOR>>        ");
            defaultScreenArray.Add("                          ");
            defaultScreenArray.Add("        TIME LEFT         ");
            defaultScreenArray.Add("       xx:xx:xx:xx        ");
            defaultScreenArray.Add("                          ");
            defaultScreenArray.Add("");
            defaultScreenArray.Add(padStringToCenter("URANIUM STORED"));
            defaultScreenArray.Add("value");
            defaultScreenArray.Add("[########]");
            defaultScreenArray.Add("");
            defaultScreenArray.Add(padStringToCenter("ENERGY OUTPUT"));
            defaultScreenArray.Add("value/value");
            defaultScreenArray.Add("[###########]");
            defaultScreenArray.Add("");
            defaultScreenArray.Add("                          ");
            defaultScreenArray.Add("==========================");

            init();
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }


        public void Main(string argument, UpdateType updateSource)
        {
            if (UpdateType.Update100 != updateSource)
                init();


            updateReactorData();
            List<String> screenArray = defaultScreenArray;

            screenArray[5] = padStringToCenter(getTimeLeft());

            screenArray[9] = padStringToCenter(convertToValue(uranLeft, "g") + " / " + convertToValue(maxUranContained, "g"));
            screenArray[10] = getProgressBar(uranLeft / maxUranContained);

            screenArray[13] = padStringToCenter(convertToValue(currentOutput, "W") + " / " + convertToValue(maxOutput, "W"));
            screenArray[14] = getProgressBar(currentOutput / maxOutput);

            displayOnScreens(screenArray);
        }

    }
}
