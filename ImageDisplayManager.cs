using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DRSystem
{
    public class ImageDisplayManager
    {
        // 图像参数
        public readonly int ImageWidth = 3072;
        public readonly int BlockHeight = 8;
        public int imageHeight = 2048;

        // 属性封装，添加setter处理显示图像高度的变化
        public int ImageHeight
        {
            get { return imageHeight; }
            set
            {
                if (value != imageHeight && value > 0)
                {
                    imageHeight = value;
                    InitializeImageRessources();
                    ResetViewToInitial();
                    DisplayPictureBox?.Invalidate();
                }
            }
        }

        // 缩放和平移参数
        public float zoomFactor = 1.0f;
        private float InitialZoom = 1.0f;                           // 其值取决如初始计算 ResetView
        private float TransitionZoom = 1.2f;
        private PointF imagePosition = new PointF(0, 0);
        private Point lastMousePosition;
        private bool isDragging = false;

        // 图像数据缓存参数
        public ushort[] FrameDataBuffer;
        public byte[] WholeImageStoreGrey;
        public ushort[] RecvDataStore;
        public short[] ShortRecvDataStore;
        public byte[] RecvDataStoreGrey;

        // 图像显示参数
        public PictureBox DisplayPictureBox;
        public static readonly object imageLock = new object();
        private Bitmap displayBitmap;


        // 双缓冲相关
        private BufferedGraphics bufferedGraphics;
        private readonly BufferedGraphicsContext context;


        // 类构造函数
        public ImageDisplayManager(PictureBox pictureBox, int imageHeight)
        {
            context = BufferedGraphicsManager.Current;

            this.DisplayPictureBox = pictureBox;
            this.ImageHeight = imageHeight;             // 自动触发InitializeImageRessources()进行初始化

            // 初始化双缓冲
            //context = BufferedGraphicsManager.Current;
            UpdateBufferedGraphics();       // Initialize中已经包含了UpdateBuffer

            // 添加事件处理
            AttachEventHandles();
        }



        #region 初始化方法
        // 初始化图像资源的方法
        private void InitializeImageRessources()
        {
            // 重新初始化数据储存
            RecvDataStore = new ushort[ImageWidth * BlockHeight];
            ShortRecvDataStore = new short[ImageWidth * BlockHeight];
            RecvDataStoreGrey = new byte[ImageWidth * BlockHeight];
            WholeImageStoreGrey = new byte[ImageWidth * ImageHeight];
            FrameDataBuffer = new ushort[ImageWidth * ImageHeight];


            // 释放旧的位图资源
            displayBitmap?.Dispose();

            // 初始化新的位图
            displayBitmap = new Bitmap(ImageWidth, ImageHeight, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            SetGrayscalePalette(displayBitmap);

            // 更新双缓冲区大小
            UpdateBufferedGraphics();
        }

        // 更新双缓冲的方法
        private void UpdateBufferedGraphics()
        {
            bufferedGraphics?.Dispose();
            // 更新最大缓冲区大小
            context.MaximumBuffer = new Size(ImageWidth + 1, ImageHeight + 1);

            // 如果PictureBox已经创建，则分配新的缓冲区
            if (DisplayPictureBox != null && !DisplayPictureBox.IsDisposed)
            {
                bufferedGraphics = context.Allocate(DisplayPictureBox.CreateGraphics(), DisplayPictureBox.ClientRectangle);
            }
        }

        // 事件处理
        private void AttachEventHandles()
        {
            if (DisplayPictureBox != null)
            {
                DisplayPictureBox.MouseWheel += DetectorDisplay_MouseWheel;
                DisplayPictureBox.MouseDown += DetectorDisplay_MouseDown;
                DisplayPictureBox.MouseUp += DetectorDisplay_MouseUp;
                DisplayPictureBox.MouseMove += DetectorDisplay_MouseMove;
                DisplayPictureBox.Paint += DetectorDisplay_Paint;

                // 添加picturebox大小改变事件处理
                DisplayPictureBox.SizeChanged += (s, e) =>
                {
                    UpdateBufferedGraphics();
                    PictureBoxSizeChangeResetView();            // 使用保持的缩放的ResetView
                    DisplayPictureBox.Invalidate();
                };
            }
        }

        private void SetGrayscalePalette(Bitmap bitmap)
        {
            ColorPalette palette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = palette;
        }
        #endregion


        #region 鼠标处理时间
        private void DetectorDisplay_MouseWheel(object sender, MouseEventArgs e)
        {
            if (displayBitmap == null) return;

            float oldZoom = zoomFactor;
            float zoomDelta = e.Delta > 0 ? 1.1f : 0.9f;
            float newZoom = zoomFactor * zoomDelta;
            newZoom = Math.Max(InitialZoom, Math.Min(10f, newZoom));

            if (Math.Abs(newZoom - zoomFactor) < 0.001f) return;

            float scaledWidth = ImageWidth * newZoom;
            float scaledHeight = ImageHeight * newZoom;

            // 计算缩放中心点（计算的是缩放中心点到图像左上角点的距离）
            PointF zoomCenter = new PointF(e.X - imagePosition.X, e.Y - imagePosition.Y);

            zoomFactor = newZoom;
            float scale = newZoom / oldZoom;

            if (newZoom <= 1.0f)  // 完全居中显示
            {
                imagePosition.X = (DisplayPictureBox.Width - scaledWidth) / 2;
                imagePosition.Y = (DisplayPictureBox.Height - scaledHeight) / 2;
            }
            else if (newZoom <= TransitionZoom)  // 过渡区域
            {
                // 计算过渡比例
                float transitionRatio = (newZoom - 1.0f) / (TransitionZoom - 1.0f);

                // 计算鼠标中心点位置
                float mouseCenterX = e.X - (zoomCenter.X * scale);
                float mouseCenterY = e.Y - (zoomCenter.Y * scale);

                // 计算居中位置
                float centerX = (DisplayPictureBox.Width - scaledWidth) / 2;
                float centerY = (DisplayPictureBox.Height - scaledHeight) / 2;

                // 根据过渡比例插值计算实际位置
                imagePosition.X = mouseCenterX * transitionRatio + centerX * (1 - transitionRatio);
                imagePosition.Y = mouseCenterY * transitionRatio + centerY * (1 - transitionRatio);
            }
            else  // 使用鼠标位置
            {
                imagePosition.X = e.X - (zoomCenter.X * scale);
                imagePosition.Y = e.Y - (zoomCenter.Y * scale);
            }

            // 边界检查
            ClampImagePosition();

            DisplayPictureBox.Invalidate(); // 触发重绘
        }

        private void DetectorDisplay_MouseDown(object sender, MouseEventArgs e)
        {
            // 启动平移
            if (e.Button == MouseButtons.Left)
            {
                lastMousePosition = e.Location;
                isDragging = true;
            }
        }

        private void DetectorDisplay_MouseUp(object sender, MouseEventArgs e)
        {
            // 结束平移
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        private void DetectorDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            // 移动中
            if (isDragging)
            {
                imagePosition.X += e.X - lastMousePosition.X;
                imagePosition.Y += e.Y - lastMousePosition.Y;
                // 边界检查,确保不会托出窗口之外
                ClampImagePosition();
                lastMousePosition = e.Location;
                DisplayPictureBox.Invalidate();
            }
        }

        private void DetectorDisplay_Paint(object sender, PaintEventArgs e)
        {
            // 重绘方法
            if (bufferedGraphics == null) return;

            bufferedGraphics.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;          //从最近邻改为双线性插值
            bufferedGraphics.Graphics.PixelOffsetMode = PixelOffsetMode.Half;                  // 用于控制绘图过程中像素对其方式，影响图像的位置精度和边缘平滑度
            bufferedGraphics.Graphics.CompositingQuality = CompositingQuality.HighQuality;     // 控制图像的合成质量
            bufferedGraphics.Graphics.SmoothingMode = SmoothingMode.HighQuality;               // 用于控制曲线和边缘的平滑程度，可选Default

            bufferedGraphics.Graphics.Clear(DisplayPictureBox.BackColor);

            lock (imageLock)
            {
                if (displayBitmap != null)
                {
                    float scaledWidth = ImageWidth * zoomFactor;
                    float scaledHeight = ImageHeight * zoomFactor;

                    int x = (int)Math.Round(imagePosition.X);
                    int y = (int)Math.Round(imagePosition.Y);
                    int width = (int)Math.Round(scaledWidth);
                    int height = (int)Math.Round(scaledHeight);

                    bufferedGraphics.Graphics.DrawImage(displayBitmap, imagePosition.X, imagePosition.Y, scaledWidth, scaledHeight);
                    //bufferedGraphics.Graphics.DrawImage(displayBitmap, x, y, width, height);
                }
            }

            bufferedGraphics.Render(e.Graphics);
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 限制图像位置
        /// </summary>
        private void ClampImagePosition()
        {
            float scaledWidth = ImageWidth * zoomFactor;
            float scaledHeight = ImageHeight * zoomFactor;

            if (scaledWidth > DisplayPictureBox.Width)
            {
                imagePosition.X = Math.Min(0, Math.Max(DisplayPictureBox.Width - scaledWidth, imagePosition.X));
            }
            else
            {
                imagePosition.X = (DisplayPictureBox.Width - scaledWidth) / 2;
            }

            if (scaledHeight > DisplayPictureBox.Height)
            {
                imagePosition.Y = Math.Min(0, Math.Max(DisplayPictureBox.Height - scaledHeight, imagePosition.Y));
            }
            else
            {
                imagePosition.Y = (DisplayPictureBox.Height - scaledHeight) / 2;
            }
        }

        // 回到显示的初始状态
        private void ResetViewToInitial()
        {
            if (DisplayPictureBox == null) return;

            // 重置缩放参数
            float ratioX = (float)this.DisplayPictureBox.Width / ImageWidth;
            float ratioY = (float)this.DisplayPictureBox.Height / ImageHeight;
            InitialZoom = Math.Min(ratioX, ratioY);
            zoomFactor = InitialZoom;                   // 重置当前缩放比例到初始值
            TransitionZoom = 1.2f;

            // 重置平移参数
            float scaledWidth = ImageWidth * zoomFactor;
            float scaledHeight = ImageHeight * zoomFactor;
            imagePosition.X = (this.DisplayPictureBox.Width - scaledWidth) / 2;
            imagePosition.Y = (this.DisplayPictureBox.Height - scaledHeight) / 2;

            // 重置拖动状态
            isDragging = false;
            lastMousePosition = Point.Empty;
        }

        // 当窗口大小改变时初始化显示
        private void PictureBoxSizeChangeResetView()
        {
            if (DisplayPictureBox == null) return;

            float currentZoom = zoomFactor;

            // 计算新的初始缩放值
            float ratioX = (float)this.DisplayPictureBox.Width / ImageWidth;
            float ratioY = (float)this.DisplayPictureBox.Height / ImageHeight;
            InitialZoom = Math.Min(ratioX, ratioY);

            // 如果当前的缩放小于新的初始值，则使用新的初始值
            if (currentZoom < InitialZoom)
                zoomFactor = InitialZoom;
            else
                zoomFactor = currentZoom;

            // 从新计算图像位置以保持居中
            float scaledWidth = ImageWidth * zoomFactor;
            float scaledHeight = ImageHeight * zoomFactor;

            // 确保图像位置在有效的范围内
            ClampImagePosition();
        }

        // 添加资源释放方法
        public void Dispose()
        {
            displayBitmap?.Dispose();
            bufferedGraphics?.Dispose();

            if (DisplayPictureBox != null)
            {
                DisplayPictureBox.MouseWheel -= DetectorDisplay_MouseWheel;
                DisplayPictureBox.MouseDown -= DetectorDisplay_MouseDown;
                DisplayPictureBox.MouseUp -= DetectorDisplay_MouseUp;
                DisplayPictureBox.MouseMove -= DetectorDisplay_MouseMove;
                DisplayPictureBox.Paint -= DetectorDisplay_Paint;
            }
        }
        #endregion

        #region 图像更新方法
        public void DataMoveUp(byte[] ByteDataInsert)
        {
            // WholeImageStoreGrey  byte图像显示数组
            //Array.Copy(WholeImageStoreGrey, ImageWidth * BlockHeight, WholeImageStoreGrey, 0, ImageWidth * (ImageHeight - BlockHeight));    // 源数组，源起点，目的数组，目的起点，要复制的源数组长度
            //Array.Copy(ByteDataInsert, 0, WholeImageStoreGrey, ImageWidth * (ImageHeight - BlockHeight), ByteDataInsert.Length);

            Buffer.BlockCopy(WholeImageStoreGrey, ImageWidth * BlockHeight, WholeImageStoreGrey, 0, ImageWidth * (ImageHeight - BlockHeight));
            Buffer.BlockCopy(ByteDataInsert, 0, WholeImageStoreGrey, ImageWidth * (ImageHeight - BlockHeight), ByteDataInsert.Length);

            //BufferWholeImageStoreGrey = WholeImageStoreGrey;
        }

        public void DataMoveUp(ushort[] UshortDataInsert)
        {
            // FrameDataBuffer ushort图像显示数组
            Array.Copy(FrameDataBuffer, ImageWidth * BlockHeight, FrameDataBuffer, 0, ImageWidth * (ImageHeight - BlockHeight));
            Array.Copy(UshortDataInsert, 0, FrameDataBuffer, ImageWidth * (ImageHeight - BlockHeight), UshortDataInsert.Length);

            // 转化为字节大小
            //int moveBytes = ImageWidth * (ImageHeight - BlockHeight) * sizeof(ushort);
            //int insertBytes = UshortDataInsert.Length * sizeof(ushort);

            //Buffer.BlockCopy(FrameDataBuffer, insertBytes, FrameDataBuffer, 0, moveBytes);
            //Buffer.BlockCopy(UshortDataInsert, 0, FrameDataBuffer, moveBytes, insertBytes);
        }

        public void UpdateBitmap()
        {
            BitmapData bitmapData = displayBitmap.LockBits(new Rectangle(0, 0, ImageWidth, ImageHeight), ImageLockMode.WriteOnly, displayBitmap.PixelFormat);

            try
            {
                Marshal.Copy(WholeImageStoreGrey, 0, bitmapData.Scan0, WholeImageStoreGrey.Length);
            }
            finally
            {
                displayBitmap.UnlockBits(bitmapData);
            }
        }

        // short类型数据转化为ushort类型的数据
        public void ShortRecvDataStoreToUshort()
        {
            //Parallel.For(0, ShortRecvDataStore.Length, i =>
            //{
            //    RecvDataStore[i] = (ushort)ShortRecvDataStore[i];
            //});

            for (int i = 0; i < ShortRecvDataStore.Length; ++i)
            {
                RecvDataStore[i] = (ushort)ShortRecvDataStore[i];
            }
        }
        #endregion
    }
}

