using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static APA.Ultrasonic;

namespace APA
{
    public partial class ChartShowForm : Form
    {
        public ChartShowForm()
        {
            InitializeComponent();
            chart_uls.Series[0].ChartType = SeriesChartType.Column;
            chart_uls.Series[0].BorderWidth = 3;
            chart_uls.Series[0].BorderDashStyle = ChartDashStyle.Dash;
            chart_uls.Series[0].MarkerColor = Color.Red;
            //chart_uls.Series[0].le
            //chart_uls.Series[0].LegendText = "超声波数据显示";
            //chart_uls.Series[0].Points.AddXY(6, 3);
            //chart_uls.Series[0].Points.AddXY(8, 3);
            //chart_uls.Series[0].Points.AddXY(8, 8);
            //chart_uls.Series[0].Points.AddXY(6, 8);
            //chart_uls.Series[0].Points.AddXY(6, 3);
        }

        public void UpdateLabelValue(UInt16 v)
        {
            label1.Text = v.ToString();
        }

        public void VehicleUltrasonicDataShow(LIN_STP318_ReadData[] UPA_data, LIN_STP313_ReadData[] APA_data )
        {
            chart_uls.Series[0].Points.Clear();
            chart_uls.Series[0].Points.AddXY(1, UPA_data[0].TOF / 58.0);
            chart_uls.Series[0].Points.AddXY(2, UPA_data[1].TOF / 58.0);
            chart_uls.Series[0].Points.AddXY(3, UPA_data[2].TOF / 58.0);
            chart_uls.Series[0].Points.AddXY(4, UPA_data[3].TOF / 58.0);
        }
    }
}
