using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NationalInstruments.Visa;
using Ivi.Visa;
using System.Threading;

using System.Windows.Forms.DataVisualization.Charting;  //chart用



namespace GPIBCONTROLWindowsFormsAppTEST1
{
    public partial class Form1 : Form
    {
        private MessageBasedSession mbSession;      //VISAセッションをするためのもの
        private System.Windows.Forms.Timer timer;　//TimerかつThreading（UIに出さないよう）とかぶらないため

        //Chart
        private Series series;
        private int displayRange = 20;  // 表示範囲のデータ数

        public Form1()
        {
            InitializeComponent();

            // タイマーの初期化
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // 1秒ごとにイベントを発生させる
            timer.Tick += timer1_Tick;

            // Chartコントロールの初期化
            series = new Series("Data");
            series.ChartType = SeriesChartType.Line; // 点として表示
            chart1.Series.Add(series);

            // チャートの軸の設定
            chart1.ChartAreas[0].AxisX.Title = "Time (seconds)";
            chart1.ChartAreas[0].AxisY.Title = "Voltage";

            // X軸の設定
            chart1.ChartAreas[0].AxisX.Minimum = 0;  // X軸の初期値
            chart1.ChartAreas[0].AxisX.Maximum = displayRange - 1 ;  // 初期表示範囲を設定 

            chart1.ChartAreas[0].AxisX.Interval = 5;  // X軸の目盛りの間隔            
            chart1.ChartAreas[0].AxisX.IsMarginVisible = false;     //X軸の余白をなくす

        }
        
        //機器との通信、個別にコマンドでトークする用
        private void button1_Click(object sender, EventArgs e)
        {
            using (var rmSession = new ResourceManager())
            {
                try
                {
                    mbSession = (MessageBasedSession)rmSession.Open("GPIB0::2::INSTR");
                    mbSession.RawIO.Write(textBox1.Text);
                    textBox2.Text = mbSession.RawIO.ReadString();
                    mbSession.Dispose();
                }
                catch
                {
                    MessageBox.Show("通信エラー");
                }
            }
        }

        //電圧の測定
        private void button2_Click(object sender, EventArgs e)  //電圧計測開始ボタン
        {
            timer1.Start();        
        }

        private void button3_Click(object sender, EventArgs e)  //停止ボタン
        {
            timer1.Enabled = false;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            using (var rmSession = new ResourceManager())   //変数の宣言
             try
            {
                    //DMMから電圧を得る
                mbSession = (MessageBasedSession)rmSession.Open("GPIB0::3::INSTR");  //特定の機器への挨拶
                mbSession.RawIO.Write("MEAS:VOLT:DC?");     //DC(直流）電圧測定のWrite
               // mbSession.RawIO.Write("MEAS:VOLT:AC?");       //AC(交流）電圧測定のWrite
                Thread.Sleep(TimeSpan.FromSeconds(1));   //Read,Write間隔
                string voltageData = mbSession.RawIO.ReadString();
                mbSession.Dispose();

                    // グラフにデータを追加
                double voltageValue = double.Parse(voltageData);
                series.Points.AddY(voltageValue);

                //    // UIの更新
                UpdateChart(voltageValue);
             }
             catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                timer1.Stop();
            }
        }

        //Chartを右から左へ流れるように更新する
        private void UpdateChart(double voltageValue)
        {
            int lastPointIndex = series.Points.Count - 1;
            //int displayRange = 20;  // 表示範囲のデータ数

            // データポイント数が表示範囲を超えている場合、表示範囲を動的に更新
            if (lastPointIndex > displayRange)
            {
                chart1.ChartAreas[0].AxisX.Minimum = lastPointIndex - displayRange;
                chart1.ChartAreas[0].AxisX.Maximum = lastPointIndex;
            }
        }
    }
}
