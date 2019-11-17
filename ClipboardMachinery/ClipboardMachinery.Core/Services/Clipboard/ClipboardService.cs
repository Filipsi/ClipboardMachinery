using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Castle.Core.Logging;
using WinClipboard = System.Windows.Clipboard;

namespace ClipboardMachinery.Core.Services.Clipboard {

    public class ClipboardService : IClipboardService {

        #region Properties

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public bool IsRunning { get; private set; }

        #endregion

        #region Events

        public event EventHandler<ClipboardEventArgs> ClipboardChanged;

        #endregion

        #region Fields

        private const string pngImageHeader = "data:image/png;base64,";

        private string lastSetContent;
        private HwndSource hwndSource;

        #endregion

        public ClipboardService() {
        }

        #region Logic

        public void Start(IntPtr handle) {
            // Service is already running, ignore the call
            if (IsRunning) {
                return;
            }

            // Create HWND source from provided pointer
            hwndSource = HwndSource.FromHwnd(handle);

            // Check if the pointer was a valid handle and create the HWND source
            if (hwndSource == null) {
                Logger.Error("Unable to create HwndSource from supplied pointer handle.");
                return;
            }

            Logger.Info($"Starting clipboard service, adding hook to {handle}.");

            // Initialize the service and start listening for clipboard changes
            NativeMethods.AddClipboardFormatListener(handle);
            hwndSource.AddHook(WndProc);
            IsRunning = true;
        }

        public void Stop() {
            // Service is not running, ignore the call
            if (!IsRunning) {
                return;
            }

            Logger.Info($"Stopping clipboard service, removing hook form {hwndSource.Handle}.");

            // Unhook the handle and remove the clipboard change listener
            hwndSource.RemoveHook(WndProc);
            NativeMethods.RemoveClipboardFormatListener(hwndSource.Handle);
            IsRunning = false;
        }

        public void SetClipboardContent(string content) {
            if (string.IsNullOrEmpty(content)) {
                return;
            }

            if (content.StartsWith(pngImageHeader)) {
                try {
                    byte[] rawImage = Convert.FromBase64String(content.Remove(0, pngImageHeader.Length));
                    using (MemoryStream imageStream = new MemoryStream(rawImage, 0, rawImage.Length)) {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = imageStream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        WinClipboard.SetImage(bitmap);
                    }
                }
                catch (Exception ex) {
                    Logger.Error("Error while trying to set clipboard image content!", ex);
                    return;
                }
            }
            else {
                WinClipboard.SetText(content);
            }

            lastSetContent = content;
        }

        private string GetClipboardContent() {
            if (WinClipboard.ContainsText()) {
                return WinClipboard.GetText();
            }

            // ReSharper disable once InvertIf
            if (WinClipboard.ContainsImage()) {
                BitmapSource bitmap = GetBitmapFromClipboardDib();

                if (bitmap != null) {
                    using (MemoryStream rawImage = new MemoryStream()) {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmap));
                        encoder.Save(rawImage);
                        return $"{pngImageHeader}{Convert.ToBase64String(rawImage.ToArray())}";
                    }
                }

                Logger.Error("Unable to create bitmap from image copied into the clipboard!");
            }

            return string.Empty;
        }

        private static BitmapSource GetBitmapFromClipboardDib() {
            MemoryStream ms = (MemoryStream) WinClipboard.GetData(DataFormats.Dib);

            if (ms == null) {
                return null;
            }

            byte[] dibBuffer = new byte[ms.Length];
            ms.Read(dibBuffer, 0, dibBuffer.Length);

            BITMAPINFOHEADER infoHeader = BinaryStructConverter.FromByteArray<BITMAPINFOHEADER>(
                bytes: dibBuffer
            );

            int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
            int infoHeaderSize = infoHeader.biSize;
            int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

            BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER {
                bfType = BITMAPFILEHEADER.BM,
                bfSize = fileSize,
                bfReserved1 = 0,
                bfReserved2 = 0,
                bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4
            };

            byte[] fileHeaderBytes = BinaryStructConverter.ToByteArray(
                obj: fileHeader
            );

            MemoryStream msBitmap = new MemoryStream();
            msBitmap.Write(fileHeaderBytes, 0, fileHeaderSize);
            msBitmap.Write(dibBuffer, 0, dibBuffer.Length);
            msBitmap.Seek(0, SeekOrigin.Begin);

            return BitmapFrame.Create(msBitmap);
        }

        #endregion

        #region Handlers

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case NativeMethods.WM_CLIPBOARDUPDATE:
                    string content = GetClipboardContent();
                    if (content != lastSetContent) {
                        ClipboardChanged?.Invoke(this, new ClipboardEventArgs(WindowHelper.GetActiveProcessName(), content));
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        #endregion

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct BITMAPFILEHEADER {
            public static readonly short BM = 0x4d42;

            public short bfType;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER {
            public readonly int biSize;
            private readonly int biWidth;
            private readonly int biHeight;
            private readonly short biPlanes;
            private readonly short biBitCount;
            private readonly int biCompression;
            public readonly int biSizeImage;
            private readonly int biXPelsPerMeter;
            private readonly int biYPelsPerMeter;
            public readonly int biClrUsed;
            private readonly int biClrImportant;
        }

        private static class NativeMethods {

            // http://msdn.microsoft.com/en-us/library/ms649021%28v=vs.85%29.aspx
            public const int WM_CLIPBOARDUPDATE = 0x031D;

            // http://msdn.microsoft.com/en-us/library/ms632599%28VS.85%29.aspx#message_only
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AddClipboardFormatListener(IntPtr hwnd);

            // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-removeclipboardformatlistener
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        }

    }

}
