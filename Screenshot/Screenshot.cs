using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Size = System.Drawing.Size;

namespace Screenshot
{
    /// <summary>
    /// Defines the <see cref="Screenshot" />.
    /// </summary>
    public class Screenshot
    {
        #region Fields

        /// <summary>
        /// 截图后的图像通过此委托返回...
        /// </summary>
        public static Action<BitmapSource> ReturnScreenShotEvent;

        /// <summary>
        /// 是否将截图保存在系统剪切板
        /// </summary>
        public static bool IsSaveInClipboard = false;

        #endregion

        #region Methods

        /// <summary>
        /// 获取屏幕当前截图.
        /// </summary>
        /// <returns>.</returns>
        public static BitmapSource CaptureAllScreens()
        {
            return CaptureRegion(new Rect(SystemParameters.VirtualScreenLeft,
                                          SystemParameters.VirtualScreenTop,
                                          SystemParameters.VirtualScreenWidth,
                                          SystemParameters.VirtualScreenHeight));
        }

        /// <summary>
        /// 根据传入的矩形截取屏幕上的一部分.
        /// </summary>
        /// <param name="rect">.</param>
        /// <returns>.</returns>
        public static BitmapSource CaptureRegion(Rect rect)
        {
            using (var bitmap = new Bitmap((int)rect.Width, (int)rect.Height, PixelFormat.Format32bppArgb))
            {
                var graphics = Graphics.FromImage(bitmap);

                graphics.CopyFromScreen((int)rect.X, (int)rect.Y, 0, 0, new Size((int)rect.Size.Width, (int)rect.Size.Height),
                                        CopyPixelOperation.SourceCopy);

                return bitmap.ToBitmapSource();
            }
        }

        /// <summary>
        /// 开始截图.
        /// </summary>
        /// <param name="options">对透明度以及框的颜色进行设置.</param>
        public static BitmapSource CaptureRegion(ScreenshotOptions options = null)
        {
            BitmapSource bitmapSource = null;
            options = options ?? new ScreenshotOptions();

            var bitmap = CaptureAllScreens();

            var left = SystemParameters.VirtualScreenLeft;
            var top = SystemParameters.VirtualScreenTop;
            var right = left + SystemParameters.VirtualScreenWidth;
            var bottom = right + SystemParameters.VirtualScreenHeight;

            var window = new RegionSelectionWindow
            {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Topmost = true,
                ShowInTaskbar = false,
                BorderThickness = new Thickness(0),
                BackgroundImage =
                             {
                                 Source = bitmap,
                                 Opacity = options.BackgroundOpacity
                             },
                InnerBorder = { BorderBrush = options.SelectionRectangleBorderBrush },
                Left = left,
                Top = top,
                Width = right - left,
                Height = bottom - top
            };

            window.ShowDialog();

            if (window.SelectedRegion == null)
            {
                ReturnScreenShotEvent?.BeginInvoke(bitmapSource, null, null);
                return null;
            }

            bitmapSource = GetBitmapRegion(bitmap, window.SelectedRegion.Value);
            //将图片保存在系统剪切板的语句
            if (IsSaveInClipboard)
            {
                System.Windows.Clipboard.SetImage(bitmapSource);
            }

            ReturnScreenShotEvent?.BeginInvoke(bitmapSource, null, null);
            return bitmapSource;
        }
        public static BitmapSource StartScreenshot(Rect rect)
        {
            var bitmap = CaptureAllScreens();
            var bitmapSource = GetBitmapRegion(bitmap, rect);
            return bitmapSource;
        }
        /// <summary>
        /// 注册热键.
        /// </summary>
        /// <param name="window">接收消息的窗口.</param>
        /// <param name="modifiers">功能键.</param>
        /// <param name="key">附加按键.</param>
        public static void RegisterHotKey(Window window, ModifierKeys modifiers, Keys key)
        {
            HotKey.Register(window, modifiers, key, delegate
            {
                Screenshot.CaptureRegion();
            });
        }

        /// <summary>
        /// 从图片中截取一部分.
        /// </summary>
        /// <param name="bitmap">The bitmap<see cref="BitmapSource"/>.</param>
        /// <param name="rect">The rect<see cref="Rect"/>.</param>
        /// <returns>The <see cref="BitmapSource"/>.</returns>
        private static BitmapSource GetBitmapRegion(BitmapSource bitmap, Rect rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return null;
            }

            return new CroppedBitmap(bitmap, new Int32Rect
            {
                X = (int)rect.X,
                Y = (int)rect.Y,
                Width = (int)rect.Width,
                Height = (int)rect.Height
            });
        }

        #endregion
    }
}
