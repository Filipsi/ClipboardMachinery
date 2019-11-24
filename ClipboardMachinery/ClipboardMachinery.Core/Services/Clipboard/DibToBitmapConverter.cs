using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace ClipboardMachinery.Core.Services.Clipboard {

    internal class DibToBitmapConverter {

        public static BitmapSource Read(MemoryStream ms) {
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

    }
}
