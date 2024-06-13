using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace ClientPlugin.Window
{
    internal class ProfileGraph
    {
        public Chart Chart;
        private Random _random = new Random();

        public ProfileGraph(Chart chart)
        {
            Chart = chart;
        }

        public void Update(ProfilingTracker tracker, List<ushort> trackedTypes)
        {
            ChartArea area = Chart.ChartAreas[0];
            area.AxisX.IntervalType = DateTimeIntervalType.Number;
            area.AxisX.Maximum = 0;
            if (area.AxisX.Minimum > tracker.LoggedInterval / TimeSpan.TicksPerSecond)
                area.AxisX.Minimum = tracker.LoggedInterval / TimeSpan.TicksPerSecond;

            Chart.Series.Clear();
            long time = DateTime.Now.Ticks;

            HashSet<string> usedLegends = new HashSet<string>();
            foreach (var networkId in trackedTypes)
            {
                string name = tracker.GetNetworkIdName(networkId);
                if (!usedLegends.Add(name) || tracker.IncomingMessagesTick[networkId].Count == 0)
                    continue;

                Series series = new Series(name);

                series.Color = Color.FromArgb(_random.Next(256), _random.Next(256), _random.Next(256));
                //series.Legend = name;
                series.ChartType = SeriesChartType.Spline;
                foreach (var data in tracker.GetAllPacketsDown(networkId))
                {
                    series.Points.AddXY((decimal)(data.Timestamp - time)/TimeSpan.TicksPerSecond, data.Size);
                }

                for (int i = (int) area.AxisX.Minimum; i <= 0; i++)
                {
                    series.Points.AddXY(0, 0);
                }

                Chart.Series.Add(series);
            }
        }
    }
}
