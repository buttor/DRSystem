using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TYMDetector;
using static DRSystem.Detector;

namespace DRSystem
{
    public partial class MainForm : Form
    {
        private struct ControlInfo
        {          
            public float OriginalLeft;         // 原始左边距
            public float OriginalTop;          // 原始上边距
            public float OriginalFontSize;     // 原始字体大小
            public Font OriginalFont;          // 原始字体
            public Size OriginalSize;          // 原始尺寸
            public Point OriginalLocation;     // 原始位置
            public Padding OriginalPadding;    // 原始内边距
            public Padding OriginalMargin;     // 原始外边距
            public float[] OriginalRowHeights; // 原始行高数组
            public float[] OriginalColumnWidths; // 原始列宽数组
        }

        // 用于存储所有控件的原始信息的字典
        private Dictionary<Control, ControlInfo> controlsInfo = new Dictionary<Control, ControlInfo>();
        private Size formOriginalSize;
        private bool isInitialized = false;
        private FormWindowState previousWindowState;    // 记录前一个窗口状态

        // 探测器相关初始化
        private const int DISCONNECT = -1;
        private int instanceId = DISCONNECT;
        public int systemType;
        public int bytePerPixel = 2;
        public int integratinTime = 7500;
        // 判断是否正在采集数据，当为ture，禁用界面放缩功能
        public bool isCollectData = false;

        SdkInterface.tymfn_datacallback tymfn_Datacallback;

        // 图像显示类定义
        private int imageHeight = 3072;
        private int imageWidth = 3072;
        private int blockHeight = 8;           // 主要原因就在block这里 太小的话会导致数据采集速度和显示速度的冲突

        
        private ImageDisplayManager imageDisplayManager;

        // 窗宽窗位类定义
        private WindowLevelForm windowLevelForm = WindowLevelForm.Instance;


        public MainForm()
        {
            InitializeComponent();

            tbx_HostIP.Text = "192.168.10.100";
            tbx_CmdPort.Text = "7171";
            tbx_ImgPort.Text = "7474";

            // 初始化图像管理类
            imageDisplayManager = new ImageDisplayManager(this.pictureBox_XrayImg, imageHeight);
            // 设置回调函数
            tymfn_Datacallback = callback;

            // 设置窗体的绘制方式，启用双缓冲减少闪烁
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |  // 所有绘制都在WM_PAINT中进行
                ControlStyles.UserPaint |             // 控件将自行绘制，而不是通过操作系统来绘制
                ControlStyles.OptimizedDoubleBuffer,           // 启用双缓冲
                true);
        }

        #region 界面窗口事件
        private void MainForm_Load(object sender, EventArgs e)
        {
            formOriginalSize = this.ClientSize;
            previousWindowState = this.WindowState;     // 初始化前一个窗口状态

            // 初始化所有控件的原始信息
            InitializeControlsInfo(this);

            // 标记初始化完成
            isInitialized = true;
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e)
        {
            //if (isCollectData) return;

            // 暂停窗体布局，提高性能
            this.SuspendLayout();
            // 暂停所有控件的布局
            foreach (Control control in controlsInfo.Keys)
            {
                if (!control.IsDisposed)
                {
                    control.SuspendLayout();
                }
            }
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            //if(isCollectData) return;

            // 确保已经完成初始化
            if (!isInitialized) return;

            try
            {
                // 计算宽度和高度的缩放比例
                float scaleX = (float)this.ClientSize.Width / formOriginalSize.Width;
                float scaleY = (float)this.ClientSize.Height / formOriginalSize.Height;

                // 递归调整所有控件的大小
                ResizeControls(this, scaleX, scaleY);

                // 恢复所有控件的布局
                foreach (Control control in controlsInfo.Keys)
                {
                    if (!control.IsDisposed)
                    {
                        control.ResumeLayout(true);
                    }
                }
                // 恢复窗体布局
                this.ResumeLayout(true);
            }
            catch (Exception ex)
            {
                // 错误处理
                MessageBox.Show($"Resize error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);                                                     
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            // 如果正在采集数据 return
            //if (isCollectData) return;

            if(!isInitialized) return;

            // 处理窗口状态变化
            if(this.WindowState != previousWindowState)
            {
                // 如果最大化或者恢复正常大小、立即调整控件
                if(this.WindowState == FormWindowState.Maximized ||
                    (previousWindowState == FormWindowState.Maximized && this.WindowState == FormWindowState.Normal))
                {
                    ResizeControlsForCurrentState();
                }
                previousWindowState = this.WindowState;
            }
        }
        #endregion

        /// <summary>
        /// 根据当前窗口状态调整控件大小
        /// </summary>
        private void ResizeControlsForCurrentState()
        {
            // Application.DoEvents();
            try
            {
                this.SuspendLayout();

                // 计算当前客户区大小与原始大小的比例
                float scaleX = (float)this.ClientSize.Width / formOriginalSize.Width;
                float scaleY = (float)this.ClientSize.Height / formOriginalSize.Height;

                // 暂停所有控件的布局
                foreach (Control control in controlsInfo.Keys)
                {
                    if (!control.IsDisposed)
                    {
                        control.SuspendLayout();
                    }
                }

                // 递归调整所有控件的大小
                ResizeControls(this, scaleX, scaleY);

                // 恢复所有控件的布局
                foreach (Control control in controlsInfo.Keys)
                {
                    if (!control.IsDisposed)
                    {
                        control.ResumeLayout(true);
                    }
                }
                // 恢复窗体布局
                this.ResumeLayout(true);
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Resize error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 初始化控件信息，递归处理所有的子控件
        /// </summary>
        /// <param name="parent">父控件</param>
        private void InitializeControlsInfo(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                // 记录控件的原始信息到字典中
                controlsInfo[control] = new ControlInfo
                {
                    OriginalSize = control.Size,
                    OriginalLocation = control.Location,
                    OriginalFont = control.Font,
                    OriginalFontSize = control.Font.Size,
                    // 检查控件是否支持padding和margin                    
                    OriginalPadding = (control as Control)?.Padding ?? new Padding(),
                    OriginalMargin = (control as Control)?.Margin ?? new Padding(),                  
                };

                // 特殊处理tablelayoutpanel控件
                if (control is TableLayoutPanel)
                {
                    TableLayoutPanel tlp = control as TableLayoutPanel;
                    var info = controlsInfo[control];

                    // 记录行高
                    info.OriginalRowHeights = new float[tlp.RowCount];
                    for (int i = 0; i < tlp.RowCount; i++)
                    {
                        if (tlp.RowStyles[i].SizeType == SizeType.Absolute)
                        {
                            info.OriginalRowHeights[i] = tlp.RowStyles[i].Height;
                        }
                        else
                            info.OriginalRowHeights[i] = -1;
                    }
                    // 记录列宽
                    info.OriginalColumnWidths = new float[tlp.ColumnCount];
                    for (int i = 0; i < tlp.ColumnCount; i++)
                    {
                        if (tlp.ColumnStyles[i].SizeType == SizeType.Absolute)
                        {
                            info.OriginalColumnWidths[i] = tlp.ColumnStyles[i].Width;
                        }
                        else
                            info.OriginalColumnWidths [i] = -1;
                    }

                    controlsInfo[control] = info;
                }

                // 递归处理子控件
                if (control.Controls.Count > 0)
                {
                    InitializeControlsInfo(control);
                }
            }            
        }

        /// <summary>
        /// 递归调整控件大小
        /// </summary>
        /// <param name="parent">父控件</param>
        /// <param name="scaleX">宽度缩放比例</param>
        /// <param name="scaleY">高度缩放比例</param>
        private void ResizeControls(Control parent, float scaleX, float scaleY)
        {
            foreach (Control control in parent.Controls)
            {
                if (controlsInfo.TryGetValue(control, out ControlInfo originalInfo))
                {
                    if (control is TableLayoutPanel tlp)
                    {
                        // TableLayoutPanel需要特殊处理                         
                        ResizeTableLayoutPanel(tlp, scaleX, scaleY, originalInfo);
                    }       
                    else if(control is TabControl tcl)
                    {
                        ResizeTabcontrol(tcl, scaleX, scaleY, originalInfo);
                    }
                    else if (control is Panel || control.Dock == DockStyle.Fill)
                    {
                        // 对于停靠方式为Fill的Panel，只需处理其子控件                     
                        ResizeControls(control, scaleX, scaleY);
                    }
                    else
                    {
                        // 处理其他类型的控件
                        ResizeGeneralControl(control, scaleX, scaleY, originalInfo);
                    }
                }
            }
        }

        /// <summary>
        /// 调整TableLayoutPanel控件的大小
        /// </summary>
        private void ResizeTableLayoutPanel(TableLayoutPanel tlp, float scaleX, float scaleY, ControlInfo originalInfo)
        {
            // 调整行高
            for (int i = 0; i < tlp.RowCount; i++)
            {
                if (originalInfo.OriginalRowHeights[i] > 0)
                {
                    tlp.RowStyles[i].Height = originalInfo.OriginalRowHeights[i] * scaleY;
                }
            }

            // 调整列宽
            for (int i = 0; i < tlp.ColumnCount; i++)
            {
                if (originalInfo.OriginalColumnWidths[i] > 0)
                {
                    tlp.ColumnStyles[i].Width = originalInfo.OriginalColumnWidths[i] * scaleX;
                }
            }
            // 处理TableLayoutPanel内的子控件
            ResizeControls(tlp, scaleX, scaleY);
        }

        /// <summary>
        /// 特殊处理Tabcontrol控件: 调整TabPage的大小
        /// </summary>
        /// <param name="tcl"></param>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        /// <param name="originalInfo"></param>
        private void ResizeTabcontrol(TabControl tcl, float scaleX, float scaleY, ControlInfo originalInfo)
        {
            float newFontSize = originalInfo.OriginalFontSize * Math.Min(scaleX, scaleY);

            // 确保字体大小在合理范围内(6-72)
            if (newFontSize >= 6 && newFontSize <= 20)
            {
                tcl.Font = new Font(tcl.Font.Name, newFontSize, tcl.Font.Style, tcl.Font.Unit);              
            }
            // 处理TabControl中的子控件
            ResizeControls(tcl, scaleX, scaleY);
        }

        /// <summary>
        /// 调整普通控件的大小
        /// </summary>
        private void ResizeGeneralControl(Control control, float scaleX, float scaleY, ControlInfo originalInfo)
        {
            // 如果控件没有停靠，则调整其大小和位置
            if (control.Dock == DockStyle.None)
            {
                // 调整大小
                control.Size = new Size(
                    (int)(originalInfo.OriginalSize.Width * scaleX),
                    (int)(originalInfo.OriginalSize.Height * scaleY));

                // 调整位置
                control.Location = new Point(
                    (int)(originalInfo.OriginalLocation.X * scaleX),
                    (int)(originalInfo.OriginalLocation.Y * scaleY));
            }
            // 需要调控字体的控件列表
            bool shouldResizeFont = control is Label || control is Button || control is TextBox || control is ComboBox
                                   || control is CheckBox || control is RadioButton || control is GroupBox;
                                                     
            if (shouldResizeFont)
            {
                // 计算新的字体大小，使用较小的缩放比例以确保文字不会过大
                float newFontSize = originalInfo.OriginalFontSize * Math.Min(scaleX, scaleY);
                // 确保字体大小在合理范围内(6-72)
                if (newFontSize >= 6 && newFontSize <= 20)
                {
                    control.Font = new Font(control.Font.Name, newFontSize, control.Font.Style, control.Font.Unit);                   
                }
            }

            if (control is GroupBox || control is Panel)
            {
                // 调整容器控件的内边距
                if (originalInfo.OriginalPadding != new Padding())
                {
                    control.Padding = new Padding(
                        (int)(originalInfo.OriginalPadding.Left * scaleX),
                        (int)(originalInfo.OriginalPadding.Top * scaleY),
                        (int)(originalInfo.OriginalPadding.Right * scaleX),
                        (int)(originalInfo.OriginalPadding.Bottom * scaleY));
                }
            }
            // 递归处理子控件
            if (control.Controls.Count > 0)
            {
                ResizeControls(control, scaleX, scaleY);
            }
        }

        /// <summary>
        /// 重写窗体的绘制方法
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            // 设置高质量绘制模式
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;              // HighQuality
            base.OnPaint(e);
        }

        #region 探测器操作

        #endregion

        private void btn_D_Connect_Click(object sender, EventArgs e)
        {
            // 本机端口
            string DaqIP = "192.168.10.1";
            SdkInterface.tymscan_init(tbx_HostIP.Text, DaqIP, Convert.ToInt32(tbx_CmdPort.Text), 
            Convert.ToInt32(tbx_ImgPort.Text), ref instanceId);

            bool inited = false;
            SdkInterface.tymscan_inited(ref inited, instanceId);

            bool connected = false;
            SdkInterface.tymscan_connected(ref connected, instanceId);

            // 绿色代表连接成功
            if(inited && connected)
            {
                btn_D_Connect.BackColor = Color.Green;
            }

            if(instanceId != DISCONNECT)
            {
                btn_D_Connect.Enabled = false;
                //探测板默认单能
                SdkInterface.tymscan_get_systype(ref systemType, ref bytePerPixel, instanceId);
                // 更新显示状态
                Console.WriteLine($"systemType{systemType}, bytePerPixel{bytePerPixel}");

                SdkInterface.tymscan_set_datacallback(tymfn_Datacallback, default(IntPtr), blockHeight, instanceId);
            }
            UpdateDetectorDisplayParamUI();
        }

        private void callback(IntPtr buffer, IntPtr extra_info, IntPtr user_data, int instance)
        {
            if (instance == instanceId)
            {
                // 图像采集 非托管代码到托管代码的转换
                SdkInterface.tymdata_buffer strucut_buf = (SdkInterface.tymdata_buffer)Marshal.PtrToStructure(buffer, typeof(SdkInterface.tymdata_buffer));

                var dataLength = imageDisplayManager.ImageWidth * imageDisplayManager.BlockHeight;
                // 先转化为short类型的数据
                Marshal.Copy(strucut_buf.data, imageDisplayManager.ShortRecvDataStore, 0, dataLength);
                imageDisplayManager.ShortRecvDataStoreToUshort();

                // 异步更新显示图像
                this.pictureBox_XrayImg.BeginInvoke(new Action(() =>
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    UpdateImage(imageDisplayManager.RecvDataStore);

                    sw.Stop();
                    Console.WriteLine("图像更新运行时间：" + sw.ElapsedMilliseconds + " ms");
                }));


            }
        }

        private void UpdateImage(ushort[] newData)
        {         
            if (newData == null) return;
            
            lock (ImageDisplayManager.imageLock)
            {
                // 更新FrameDataBuffer               
                imageDisplayManager.DataMoveUp(newData);

                // 施加窗宽窗为效果并更新显示用的灰度图像
                byte[] newBlockGrey = windowLevelForm.ApplyWindowLevelWithLUT(newData);              
                imageDisplayManager.DataMoveUp(newBlockGrey);
               
                imageDisplayManager.UpdateBitmap();             // 1-2ms
            }
            pictureBox_XrayImg.Invalidate();          
        }

        


        private void btn_Start_Click(object sender, EventArgs e)
        {
            SdkInterface.tymscan_grab_start(instanceId);
            isCollectData = true;
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            SdkInterface.tymscan_grab_stop(instanceId);
            isCollectData = false;
        }

        // 
        private void btn_CollectValueSet_Click(object sender, EventArgs e)
        {
            // 设置积分时间
            SdkInterface.tymscan_set_integral_time(Convert.ToInt32(tbx_IntegrationTime.Text), instanceId);
            // 设置工作模式
            //SdkInterface.tymscan_set_test_pattern(cmbx_TestPattern.SelectedIndex, instanceId);
            // 设置显示帧高
            //imageDisplayManager.ImageHeight = Convert.ToInt32(tbx_ImageHeight.Text);


            // 增益设置
            SdkInterface.tymscan_set_gain_low(0, cmbx_Gain.SelectedIndex + 1, instanceId);      // select初始值为1
        }

        private void btn_WindowSetting_Click(object sender, EventArgs e)
        {
            // 传递图像显示管理类
            windowLevelForm.forWindowIDM = imageDisplayManager;
            windowLevelForm.Show();
        }

        private void chbx_OffsetCalibrate_CheckedChanged(object sender, EventArgs e)
        {
            SdkInterface.tymcan_set_sendCalibratedData_Enable(true, true, true, instanceId);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 释放资源
            imageDisplayManager?.Dispose();
            base.OnFormClosing(e);
        }


        private void UpdateDetectorDisplayParamUI()
        {
            // 如果有保存的参数数据，则加载参数数据，如果没有则加载默认值
            bool paramExisted = false;
            if(paramExisted)
            {
                // 加载现存的参数
            }
            else
            {
                // 加载默认参数
                tbx_ImageHeight.Text = imageHeight.ToString();
                //SdkInterface.tymscan_get_integral_time(ref integratinTime, instanceId);
                tbx_IntegrationTime.Text = integratinTime.ToString();

                int testPatternMin = 0;
                int testPatternMax = 0;
                // 获取测试模式范围
                SdkInterface.tymscan_get_test_pattern_range(ref testPatternMin, ref testPatternMax, instanceId);
                string[] testPatternRange = new string[testPatternMax - testPatternMin + 1];
                for (int i = 0; i < testPatternRange.Length; i++)
                {
                    testPatternRange[i] = i.ToString();
                }
                cmbx_TestPattern.Items.Clear();
                cmbx_TestPattern.Items.AddRange(testPatternRange);

                int testpattern = 0;
                SdkInterface.tymscan_get_test_pattern(ref testpattern, instanceId);
                cmbx_TestPattern.SelectedIndex = testpattern;
                SdkInterface.tymscan_set_test_pattern(cmbx_TestPattern.SelectedIndex, instanceId);

                // 加载增益值
                int gain_min = 0;
                int gain_max = 0;
                SdkInterface.tymscan_get_gain_range(ref gain_min, ref gain_max, instanceId);
                string[] gain_range = new string[gain_max - gain_min + 1];
                for (int i = 1; i <= gain_range.Length; i++)
                {
                    gain_range[i - 1] = i.ToString();
                }
                cmbx_Gain.Items.Clear();
                cmbx_Gain.Items.AddRange(gain_range);
                cmbx_Gain.SelectedIndex = 1;
            }
        }
    }
}
