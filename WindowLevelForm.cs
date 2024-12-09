using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DRSystem
{
    public partial class WindowLevelForm : Form
    {
        private static WindowLevelForm instance;

        // 窗宽窗位设置
        private int lowerLimit = 0;
        private int upperLimit = 65535;
        private readonly byte[] lookupTable = new byte[65536];

        // 用于从主界面传递图像显示类
        public ImageDisplayManager forWindowIDM;
        // 单例实例


        private WindowLevelForm()
        {
            InitializeComponent();

            // 初始化窗宽窗位查找表
            UpdateLookupTable(lowerLimit, upperLimit);
        }


        private void UpdateLookupTable(int lower, int upper)
        {
            //Parallel.For(0, 65536, i =>
            //{
            //    if (i <= lower)
            //        lookupTable[i] = 0;
            //    else if (i >= upper)
            //        lookupTable[i] = 255;
            //    else
            //        lookupTable[i] = (byte)((i - lower) * 255 / (upper - lower));
            //});

            for(int i = 0; i < 65536; ++i)
            {
                if (i <= lower)
                    lookupTable[i] = 0;
                else if (i >= upper)
                    lookupTable[i] = 255;
                else
                    lookupTable[i] = (byte)((i - lower) * 255 / (upper - lower));
            }
        }

        public byte[] ApplyWindowLevelWithLUT(ushort[] data)
        {
            byte[] byteData = new byte[data.Length];
            //Parallel.For(0, data.Length, i =>
            //{
            //    byteData[i] = lookupTable[data[i]];
            //});

            for(int i = 0; i < data.Length; ++i)
            {
                byteData[i] = lookupTable[data[i]];
            }

            return byteData;
        }

        private void UpdateWindowLevel(int lower, int upper)
        {
            // 更新窗宽窗位映射表
            UpdateLookupTable(lower, upper);

            // 更新显示图像
            lock (ImageDisplayManager.imageLock)
            {
                byte[] newImageGrey = ApplyWindowLevelWithLUT(forWindowIDM.FrameDataBuffer);
                Array.Copy(newImageGrey, forWindowIDM.WholeImageStoreGrey, newImageGrey.Length);
                forWindowIDM.UpdateBitmap();
            }

            forWindowIDM.DisplayPictureBox.Invalidate();
        }

        private void trackBarLowerLimit_Scroll(object sender, EventArgs e)
        {
            // 下限调整
            lowerLimit = trackBarLowerLimit.Value;
            tbx_LowerValue.Text = lowerLimit.ToString();
        }

        private void trackBarUpperLimit_Scroll(object sender, EventArgs e)
        {
            upperLimit = trackBarUpperLimit.Value;
            tbx_UpperValue.Text = upperLimit.ToString();
        }

        private void trackBarUpperLimit_ValueChanged(object sender, EventArgs e)
        {
            UpdateWindowLevel(trackBarLowerLimit.Value, trackBarUpperLimit.Value);
        }

        private void trackBarLowerLimit_ValueChanged(object sender, EventArgs e)
        {
            UpdateWindowLevel(trackBarLowerLimit.Value, trackBarUpperLimit.Value);
        }

        private void btn_ValueSet_Click(object sender, EventArgs e)
        {
            // 通过输入值进行上下限设置
            lowerLimit = int.Parse(this.tbx_LowerValue.Text);
            upperLimit = int.Parse(this.tbx_UpperValue.Text);

            trackBarUpperLimit.Value = upperLimit;
            trackBarLowerLimit.Value = lowerLimit;
        }

        // 公共静态方法，提供单例实例访问
        public static WindowLevelForm Instance
        {
            get
            {
                if (instance == null || instance.IsDisposed)
                {
                    instance = new WindowLevelForm();
                }
                return instance;
            }
        }

        private void WindowLevelForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;        // 取消关闭操作
            this.Hide();
        }
    }
}
