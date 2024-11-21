using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public MainForm()
        {
            InitializeComponent();

            // 设置窗体的绘制方式，启用双缓冲减少闪烁
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |  // 所有绘制都在WM_PAINT中进行
                ControlStyles.UserPaint |             // 控件将自行绘制，而不是通过操作系统来绘制
                ControlStyles.DoubleBuffer,           // 启用双缓冲
                true);


        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            formOriginalSize = this.ClientSize;

            // 初始化所有控件的原始信息
            InitializeControlsInfo(this);

            // 标记初始化完成
            isInitialized = true;
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e)
        {
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


        // 初始化控件信息，递归处理所有的子控件
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
            foreach (Control control in parent.Controls)                                    // 初次调用Controls的Count=3,即:tablelayoutpanel,menustrips,statusStrips
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

        private void ResizeTabcontrol(TabControl tcl, float scaleX, float scaleY, ControlInfo originalInfo)
        {
            float newFontSize = originalInfo.OriginalFontSize * Math.Min(scaleX, scaleY);

            // 确保字体大小在合理范围内(6-72)
            if (newFontSize >= 6 && newFontSize <= 72)
            {
                tcl.Font = new Font(tcl.Font.Name, newFontSize, tcl.Font.Style, tcl.Font.Unit);              
            }
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

            // 需要调整字体的控件类型列表
            bool shouldResizeFont = control is Label ||
                                  control is Button ||
                                  control is TextBox ||
                                  control is ComboBox ||
                                  control is CheckBox ||
                                  control is RadioButton ||
                                  control is GroupBox ||
                                  control is TabPage ||
                                  control is LinkLabel;         

            // 统一处理字体缩放
            if (shouldResizeFont)
            {
                // 计算新的字体大小，使用较小的缩放比例以确保文字不会过大
                float newFontSize = originalInfo.OriginalFontSize * Math.Min(scaleX, scaleY);

                // 确保字体大小在合理范围内(6-72)
                if (newFontSize >= 6 && newFontSize <= 72)
                {
                    // 创建新字体对象，保持原有字体的其他属性（样式、字体族等）
                    //using (var newFont = new Font(
                    //    originalInfo.OriginalFont.FontFamily,
                    //    newFontSize,
                    //    originalInfo.OriginalFont.Style,
                    //    originalInfo.OriginalFont.Unit,
                    //    originalInfo.OriginalFont.GdiCharSet))
                    //{
                    //    control.Font = newFont;
                    //}

                    control.Font = new Font(control.Font.Name, newFontSize, control.Font.Style, control.Font.Unit);

                    //using (var newFont = new Font(
                    //    originalInfo.OriginalFont.Name,
                    //    originalInfo.OriginalFontSize,
                    //    originalInfo.OriginalFont.Style,
                    //    originalInfo.OriginalFont.Unit
                    //    ))
                    //{
                    //    control.Font = newFont;
                    //}
                }
            }

            // 特殊控件的处理
            //if (control is TextBox || control is ComboBox)
            //{
            //    // 调整文本控件的字体大小
            //    float newFontSize = originalInfo.OriginalFontSize * Math.Min(scaleX, scaleY);
            //    // 确保字体大小在合理范围内(6-72)
            //    if (newFontSize >= 6 && newFontSize <= 72)
            //    {
            //        control.Font = new Font(originalInfo.OriginalFont.FontFamily,
            //            newFontSize,
            //            originalInfo.OriginalFont.Style);
            //    }
            //}
            if (control is PictureBox pic)
            {
                // 图片控件缩放目前不需要特殊处理
                if (pic.SizeMode == PictureBoxSizeMode.Zoom)
                {
                    // Zoom模式下不需要特殊处理
                }
            }
            else if (control is GroupBox || control is Panel)
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
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            base.OnPaint(e);
        }
    }
}
