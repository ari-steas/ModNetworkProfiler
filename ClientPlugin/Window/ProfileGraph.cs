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
        private Dictionary<long, Color> typeColorMap = new Dictionary<long, Color>();

        public ProfileGraph(Chart chart)
        {
            Chart = chart;
            ChartArea area = Chart.ChartAreas[0];
            area.AxisX.IntervalType = DateTimeIntervalType.Number;
            area.AxisX.Maximum = 0;
            area.BackColor = Color.FromArgb(38,38,38);

            area.AxisX.LineColor = Color.White;
            area.AxisX.TitleForeColor = Color.White;
            area.AxisX.InterlacedColor = Color.White;
            area.AxisY.LineColor = Color.White;
            area.AxisY.TitleForeColor = Color.White;
            area.AxisY.InterlacedColor = Color.White;
        }

        public void Update(ProfilingTracker tracker, List<ushort> trackedTypes)
        {
            ChartArea area = Chart.ChartAreas[0];
            if (area.AxisX.Minimum > -tracker.LoggedInterval / TimeSpan.TicksPerSecond)
                area.AxisX.Minimum = -tracker.LoggedInterval / TimeSpan.TicksPerSecond;

            Chart.Series.Clear();
            long time = DateTime.Now.Ticks;

            HashSet<string> usedLegends = new HashSet<string>();
            foreach (var networkId in trackedTypes)
            {
                string name = tracker.GetNetworkIdName(networkId);
                if (!usedLegends.Add(name) || tracker.IncomingMessagesTick[networkId].Count == 0)
                    continue;

                Series series = new Series(name);

                if (!typeColorMap.ContainsKey(networkId))
                    typeColorMap[networkId] = Color.FromArgb(_random.Next(256), _random.Next(256), _random.Next(256));

                series.Color = typeColorMap[networkId];
                series.ChartType = SeriesChartType.Column;
                series.IsValueShownAsLabel = true;
                series.LabelForeColor = Color.White;

                //series["PixelPointWidth"] = "40";
                //series.BorderWidth = 2;

                foreach (var data in tracker.GetAllPacketsDown(networkId))
                {
                    var point = series.Points[series.Points.AddXY((decimal)(data.Timestamp - time)/TimeSpan.TicksPerSecond, data.Size)];
                    //point.SetCustomProperty("LabelStyle", "Bottom");
                }

                //for (int i = (int) -(tracker.LoggedInterval / TimeSpan.TicksPerSecond); i <= 0; i++)
                //{
                //    series.Points.AddXY(i, 0);
                //}

                Chart.Series.Add(series);
            }

            area.AxisX.Maximum = 0;
            area.AxisX.Minimum = -tracker.LoggedInterval / TimeSpan.TicksPerSecond;
            area.AxisX.Interval = 1;
            Chart.Legends[0].Docking = Docking.Bottom;
        }
    }
}
