using System;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Serial4PunpNew
{
    public partial class Form1 : Form
    {
        const int serial_Num = 2049;            // 串口单次接收总字节数
        byte[] buffer   = new byte[serial_Num]; // 串口接收数据缓存
        int flag_chart  = 0;                    // 图表刷新标志
        int open_serial = 0;                    // 串口开关标志位
        int open_series = 0;                    // 连续识别模式标志位
        int received_num = 0;                   // 串口缓冲区有效字节数
        int received_num_buf  = 0;
        int received_num_buf2 = 0;
        int serial1_count = 0;                  // 串口接收事件进入次数

        // 钞票数量统计值初始化
        int money_new_100 = 0;
        int money_old_100 = 0;
        int money_50 = 0;
        int money_10 = 0;
        int money_20 = 0;
        int money_5 = 0;
        int money_0 = 0;
        int money = 0;
        int money_num = 0;

        // 窗口初始化
        public Form1()
        {
            InitializeComponent();
        }

        // 窗口默认数据加载
        private void Form1_Load(object sender, EventArgs e)
        {
            Serial_init();  // 串口初始化
            Chart_init();   // 图表初始化

            // 定义串口接收事件
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
        }

        // 串口接受事件
        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            serial1_count++;                                // 更新串口接收事件进入次数
            received_num = serialPort1.BytesToRead;         // 更新串口缓冲区有效字节数
            received_num_buf2 = received_num;

            // 只有当缓存区中可读数据量等于serial_Num且不处于连续识别模式时，才进行串口读操作(2049字节)
            if ((open_series == 0) && (received_num == serial_Num))
            {
                serialPort1.Read(buffer, 0, received_num);  // 从串口缓冲区中获取数据
                serialPort1.DiscardInBuffer();              // 清空串口缓冲区
                flag_chart = 1;                             // 图表刷新标志置位
            }
        }

        // 图表初始化
        public void Chart_init()
        {
            int i;

            // 时域图表初始化
            chart1.Series.Clear();                              // 清楚默认的Series
            Series time = new Series("时域")                    // 定义时序序列
            {
                BorderWidth = 2,                                // 设置线宽
                ChartType = SeriesChartType.Line                // 设置图表样式为折线图
            };                                                  // 定义新图表
            chart1.ChartAreas[0].AxisX.Minimum = 0;             // 限制X轴范围
            chart1.ChartAreas[0].AxisX.Maximum = 1023;
            chart1.ChartAreas[0].AxisY.Minimum = 0;             // 限制Y轴范围
            chart1.ChartAreas[0].AxisY.Maximum = 200;
            for (i = 0; i < serial_Num; i++)                    // 初始化时域数据
            {
                buffer[i] = 0;
            }
            for (i = 0; i < 1024; i++)                          // 获取数据点的集合
            {
                time.Points.AddXY(i, 0);
            }
            chart1.Series.Add(time);                            // 更新时域图表

            // 频域图表初始化
            chart2.Series.Clear();                              // 清楚默认的Series
            Series frequency = new Series("频域")               // 定义频域序列
            {
                BorderWidth = 2,                                // 设置线宽
                ChartType = SeriesChartType.Line                // 设置图表样式为折线图
            };
            chart2.ChartAreas[0].AxisX.Minimum = 0;             // 限制X轴范围
            chart2.ChartAreas[0].AxisX.Maximum = 511;
            chart2.ChartAreas[0].AxisY.Minimum = 0;             // 限制Y轴范围
            chart2.ChartAreas[0].AxisY.Maximum = 255;
            for (i = 0; i < 512; i++)                           // 获取数据点的集合
            {
                frequency.Points.AddXY(i, 0);
            }
            chart2.Series.Add(frequency);                       // 更新时域图表

            // 初始化统计数值
            label9.Text = "0";
        }

        // 串口初始化
        public void Serial_init()
        {
            // 查询可用COM口
            foreach (string s in SerialPort.GetPortNames())
            {
                PortName.Items.Add(s);
            }
            try
            {
                PortName.Text = PortName.Items[0].ToString();
            }
            catch
            {
                MessageBox.Show("找不到可用串口！", "ERROR");
            }
            //serialPort1.RtsEnable = true;

            // 其他缺省串口设置
            BaudRate.Text  = "921600";
            DataBits.Text  = "8";
            StopBits.Text  = "One";
            Parity.Text    = "None";
            reset.Enabled  = false;

            // 更新时间
            date.Text = DateTime.Now.ToString();
        }

        // 更新串口列表
        private void Refresh_Click(object sender, EventArgs e)
        {
            // 清楚串口列表
            PortName.Items.Clear();
            foreach (string s in SerialPort.GetPortNames())
            {
                // 查询可用COM口
                PortName.Items.Add(s);
            }

            try
            {
                // 列表置初值
                PortName.Text = PortName.Items[0].ToString();
            }
            catch
            {
                MessageBox.Show("找不到可用串口！", "错误");
            }
        }

        // 复位（清空串口缓冲区数据、初始化图表）
        private void Reset_Click(object sender, EventArgs e)
        {
            try
            {
                Chart_init();                   // 初始化图表
                serialPort1.DiscardInBuffer();  // 清空串口缓冲区数据
            }
            catch
            {
                MessageBox.Show("缓存为空！", "提示");
            }
        }

        // 定时器1，用于更新时间
        private void Timer1_Tick(object sender, EventArgs e)
        {
            date.Text = DateTime.Now.ToString();            // 更新时间
        }

        // 定时器2，用于刷新图表
        private void Timer2_Tick(object sender, EventArgs e)
        {
            int i = 0;

            // 图表刷新 + 价值统计
            this.Invoke((EventHandler)(delegate
            {
                if (flag_chart == 1)
                {
                    chart1.Series.Clear();                              // 移除chart1所有元素
                    chart2.Series.Clear();                              // 移除chart2所有元素
                    Series time = new Series("时域");                   // 定义时域序列
                    Series frequency = new Series("频域");              // 定义频域序列
                    time.BorderWidth = 1;                               // 设置线宽
                    frequency.BorderWidth = 1;
                    time.ChartType = SeriesChartType.Line;              // 设置图表样式为折线图
                    frequency.ChartType = SeriesChartType.Line;
                    for (i = 0; i < 1024; i++)                          // 获取数据点的集合
                    {
                        time.Points.AddXY(i, buffer[i]);
                    }
                    for (i = 0; i < 512; i++)                           // 获取数据点的集合
                    {
                        frequency.Points.AddXY(i, buffer[i + 1024]);
                    }
                    chart1.Series.Add(time);                            // 更新时域图表
                    chart2.Series.Add(frequency);                       // 更新频域图表
                    flag_chart = 0;

                    // 更新统计数值
                    label9.Text = buffer[2048].ToString();

                    // 以 TXT 文件保存数据
                    Save_txt();
                }
            }));

            // 只有间隔两秒串口缓冲区内可读字节数不同时，才进行串口读操作
            if ((open_series == 1) && (received_num_buf == received_num) && (received_num != 0))
            {
                serialPort1.Read(buffer, 0, received_num);  // 从串口缓冲区中获取数据
                serialPort1.DiscardInBuffer();              // 清空串口缓冲区

                for (i = 0; i < received_num; i++)
                {
                    // 连续识别模式更新统计值
                    if (buffer[i] == 101)       { money_new_100++; label17.Text = money_new_100.ToString(); }
                    else if (buffer[i] == 100)  { money_old_100++; label20.Text = money_old_100.ToString(); }
                    else if (buffer[i] == 50)   { money_50++; label21.Text = money_50.ToString(); }
                    else if (buffer[i] == 20)   { money_20++; label28.Text = money_20.ToString(); }
                    else if (buffer[i] == 10)   { money_10++; label22.Text = money_10.ToString(); }
                    else if (buffer[i] == 5)    { money_5++; label23.Text = money_5.ToString(); }
                    else if (buffer[i] == 0)    { money_0++; }
                    money = (money_new_100 + money_old_100) * 100 + money_50 * 50 + money_20 * 20 + money_10 * 10 + money_5 * 5;
                    money_num = money_new_100 + money_old_100 + money_50 + money_20 + money_10 + money_5 + money_0;
                    label18.Text = money.ToString();
                    label30.Text = money_num.ToString();
                    label32.Text = money_0.ToString();
                }
                received_num = 0;       // 串口接收字节数清零
            }
            received_num_buf = received_num;

            debug_count.Text = serial1_count.ToString();    // 显示串口事件触发次数
            debug_num.Text = received_num_buf2.ToString();  // 显示串口接收数据数量
        }

        // 串口开关按钮
        private void Open_Click(object sender, EventArgs e)
        {
            if (open_serial == 0)
            {
                try
                {
                    // 串口初始化
                    serialPort1.PortName = PortName.Text;                                   // 端口号
                    serialPort1.BaudRate = Convert.ToInt32(BaudRate.Text);                  // 波特率
                    serialPort1.DataBits = Convert.ToInt16(DataBits.Text);                  // 数据位
                    serialPort1.Parity   = (Parity)Enum.Parse(typeof(Parity), Parity.Text); // 校验位
                    serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), StopBits.Text);   // 停止位
                    serialPort1.ReceivedBytesThreshold = 1;       // 设置串口接收事件触发阈值

                    serialPort1.Open();                                    // 打开串口
                    serialPort1.DiscardInBuffer();                         // 清空串口缓冲区数据

                    // 禁止修改串口设置
                    PortName.Enabled = false;
                    BaudRate.Enabled = false;
                    DataBits.Enabled = false;
                    StopBits.Enabled = false;
                    Parity.Enabled   = false;
                    refresh.Enabled  = false;
                    reset.Enabled    = true;

                    open_serial = 1;
                    open.Text = "关闭";
                }
                catch { MessageBox.Show("串口设置错误！", "错误"); }
            }
            else
            {
                try
                {
                    // 关闭串口
                    serialPort1.Close();

                    // 使能串口设置
                    PortName.Enabled = true;
                    BaudRate.Enabled = true;
                    DataBits.Enabled = true;
                    StopBits.Enabled = true;
                    Parity.Enabled   = true;
                    refresh.Enabled  = true;
                    reset.Enabled    = false;

                    open_serial = 0;
                    open.Text = "打开";
                }
                catch { MessageBox.Show("串口无法关闭！", "错误"); }
            }
        }

        // 清空统计数值
        private void Clear_Click(object sender, EventArgs e)
        {
            buffer[2048] = 0;
            label9.Text = "0";
        }

        // 使用 TXT 文件保存串口数据
        public void Save_txt()
        {
            int i = 0;

            // 将数据写入文件中，文件以"yyyyMMddHHmm.txt"格式命名
            string name = "./Date/" + buffer[2048].ToString() + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";

            System.IO.Directory.CreateDirectory(System.IO.Path.Combine("./", "Date"));

            // 以文件流方式写入数据
            FileStream fs = new FileStream(name, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);

            // 以追加的方式不断写入数据
            for (i = 0; i < serial_Num; i++)
            {
                sw.WriteLine(buffer[i].ToString());
            }

            //关闭文件
            sw.Close();
            fs.Close();
        }

        // 打开连续识别模式
        private void Open2_Click(object sender, EventArgs e)
        {
            if (open_series == 0)
            {
                try
                {
                    serialPort1.DiscardInBuffer();          // 清空串口缓冲区数据
                    received_num = 0;
                    open_series = 1;
                    open2.Text = "关闭";
                }
                catch { MessageBox.Show("无法打开连续识别模式！", "错误"); }
            }
            else
            {
                try
                {
                    serialPort1.DiscardInBuffer();          // 清空串口缓冲区数据
                    received_num = 0;
                    open_series = 0;
                    open2.Text = "打开";
                }
                catch { MessageBox.Show("无法关闭连续识别模式！", "错误"); }
            }
        }

        // 新壹佰统计值清零
        private void Button2_Click(object sender, EventArgs e)
        {
            money_new_100 = 0;
            label17.Text = "0";
            money = (money_new_100 + money_old_100) * 100 + money_50 * 50 + money_20 * 20 + money_10 * 10 + money_5 * 5;
            money_num = money_new_100 + money_old_100 + money_50 + money_20 + money_10 + money_5 + money_0;
            label18.Text = money.ToString();
            label30.Text = money_num.ToString();
        }

        // 旧壹佰统计值清零
        private void Button3_Click(object sender, EventArgs e)
        {
            money_old_100 = 0;
            label20.Text = "0";
            money = (money_new_100 + money_old_100) * 100 + money_50 * 50 + money_20 * 20 + money_10 * 10 + money_5 * 5;
            money_num = money_new_100 + money_old_100 + money_50 + money_20 + money_10 + money_5 + money_0;
            label18.Text = money.ToString();
            label30.Text = money_num.ToString();
        }

        // 伍拾圆统计值清零
        private void Button4_Click(object sender, EventArgs e)
        {
            money_50 = 0;
            label21.Text = "0";
            money = (money_new_100 + money_old_100) * 100 + money_50 * 50 + money_20 * 20 + money_10 * 10 + money_5 * 5;
            money_num = money_new_100 + money_old_100 + money_50 + money_20 + money_10 + money_5 + money_0;
            label18.Text = money.ToString();
            label30.Text = money_num.ToString();
        }

        // 贰拾圆统计值清零
        private void Button1_Click(object sender, EventArgs e)
        {
            money_20 = 0;
            label28.Text = "0";
            money = (money_new_100 + money_old_100) * 100 + money_50 * 50 + money_20 * 20 + money_10 * 10 + money_5 * 5;
            money_num = money_new_100 + money_old_100 + money_50 + money_20 + money_10 + money_5 + money_0;
            label18.Text = money.ToString();
            label30.Text = money_num.ToString();
        }

        // 拾圆统计值清零
        private void Button5_Click(object sender, EventArgs e)
        {
            money_10 = 0;
            label22.Text = "0";
            money = (money_new_100 + money_old_100) * 100 + money_50 * 50 + money_20 * 20 + money_10 * 10 + money_5 * 5;
            money_num = money_new_100 + money_old_100 + money_50 + money_20 + money_10 + money_5 + money_0;
            label18.Text = money.ToString();
            label30.Text = money_num.ToString();
        }

        // 伍圆统计值清零
        private void Button6_Click(object sender, EventArgs e)
        {
            money_5 = 0;
            label23.Text = "0";
            money = (money_new_100 + money_old_100) * 100 + money_50 * 50 + money_20 * 20 + money_10 * 10 + money_5 * 5;
            money_num = money_new_100 + money_old_100 + money_50 + money_20 + money_10 + money_5 + money_0;
            label18.Text = money.ToString();
            label30.Text = money_num.ToString();
        }

        // 统计值全局清零
        private void Button7_Click(object sender, EventArgs e)
        {
            money_new_100 = 0;
            money_old_100 = 0;
            money_50 = 0;
            money_20 = 0;
            money_10 = 0;
            money_5 = 0;
            money_0 = 0;
            money = 0;
            money_num = 0;

            label17.Text = "0";
            label18.Text = "0";
            label20.Text = "0";
            label21.Text = "0";
            label22.Text = "0";
            label23.Text = "0";
            label28.Text = "0";
            label30.Text = "0";
            label32.Text = "0";
        }
    }
}
