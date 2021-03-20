
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
//using System.Drawing;
using Color = UnityEngine.Color;

namespace TgaDecoder
{
    public class TgaDecoder
    {
        private static TgaData tga;

        public static Texture2D FromFile(string path)
        {
            try
            {
                return decode(File.ReadAllBytes(path));
            }
            catch (Exception)
            {
                return new Texture2D(2,2);
            }
        }

        public static Texture2D FromBinary(byte[] image)
        {
            return decode(image);
        }

        protected static Texture2D decode(byte[] image)
        {
            tga = new TgaData(image);

            Texture2D tex = new Texture2D(tga.Width, tga.Height);

            for (int y = 0; y < tga.Height; ++y)
            {
                for (int x = 0; x < tga.Width; ++x)
                {
                    Color color = tga.GetPixel(x, y);
                    tex.SetPixel(x, y, color);
                    
                }
            }
            tex.Apply(true);
            tex.Compress(false);
            return tex;
        }

        protected class TgaData
        {
            private const int TgaHeaderSize = 18;
            private int colorMapType;
            private int imageType;
            private int imageWidth;
            private int imageHeight;
            private int bitPerPixel;
            private int descriptor;
            private byte[] colorData;

            public TgaData(byte[] image)
            {
                this.colorMapType = image[1];
                this.imageType = image[2];
                this.imageWidth = image[13] << 8 | image[12];
                this.imageHeight = image[15] << 8 | image[14];
                this.bitPerPixel = image[16];
                this.descriptor = image[17];

                var length = image.Length - TgaHeaderSize;
                if (colorData == null || length > colorData.Length)
                    this.colorData = new byte[length];

                Array.Copy(image, TgaHeaderSize, this.colorData, 0, length);
                // Index color RLE or Full color RLE or Gray RLE
                if (this.imageType == 9 || this.imageType == 10 || this.imageType == 11)
                {
                    this.colorData = this.decodeRLE();
                }
            }

            public int Width
            {
                get { return this.imageWidth; }
            }

            public int Height
            {
                get { return this.imageHeight; }
            }

            private Color newcolor = new Color();

            public Color GetPixel(int x, int y)
            {
                if (colorMapType == 0)
                {
                    switch (this.imageType)
                    {
                        // Index color
                        case 1:
                        case 9:
                            // not implemented
                            return newcolor;

                        // Full color
                        case 2:
                        case 10:
                            int elementCount = this.bitPerPixel / 8;
                            int dy = ((this.descriptor & 0x20) == 0 ? y : (this.imageHeight - 1 - y)) * (this.imageWidth * elementCount);
                            int dx = ((this.descriptor & 0x10) == 0 ? x : (this.imageWidth - 1 - x)) * elementCount;
                            int index = dy + dx;

                            int b = this.colorData[index + 0] & 0xFF;
                            int g = this.colorData[index + 1] & 0xFF;
                            int r = this.colorData[index + 2] & 0xFF;

                            if (elementCount == 4) // this.bitPerPixel == 32
                            {
                                int a = this.colorData[index + 3] & 0xFF;
                                newcolor.r = r / 255f;
                                newcolor.g = g / 255f;
                                newcolor.b = b / 255f;
                                newcolor.a = a / 255f;

                                return newcolor;// (a << 24) | (r << 16) | (g << 8) | b;
                            }
                            else if (elementCount == 3) // this.bitPerPixel == 24
                            {
                                newcolor.r = r / 255f;
                                newcolor.g = g / 255f;
                                newcolor.b = b / 255f;
                                newcolor.a = 1f;
                                return newcolor;// (r << 16) | (g << 8) | b;
                            }
                            break;

                        // Gray
                        case 3:
                        case 11:
                            // not implemented
                            return newcolor;
                    }
                    return newcolor;
                }
                else
                {
                    // not implemented
                    return newcolor;
                }
            }

            private static byte[] decodeBuffer;

            protected byte[] decodeRLE()
            {
                int elementCount = this.bitPerPixel / 8;
                byte[] elements = new byte[elementCount];
                int decodeBufferLength = elementCount * this.imageWidth * this.imageHeight;

                if (decodeBuffer == null || decodeBufferLength > decodeBuffer.Length)
                    decodeBuffer = new byte[decodeBufferLength];

                int decoded = 0;
                int offset = 0;
                while (decoded < decodeBufferLength)
                {
                    int packet = this.colorData[offset++] & 0xFF;
                    if ((packet & 0x80) != 0)
                    {
                        for (int i = 0; i < elementCount; i++)
                        {
                            elements[i] = this.colorData[offset++];
                        }
                        int count = (packet & 0x7F) + 1;
                        for (int i = 0; i < count; i++)
                        {
                            for (int j = 0; j < elementCount; j++)
                            {
                                decodeBuffer[decoded++] = elements[j];
                            }
                        }
                    }
                    else
                    {
                        int count = (packet + 1) * elementCount;
                        for (int i = 0; i < count; i++)
                        {
                            decodeBuffer[decoded++] = this.colorData[offset++];
                        }
                    }
                }
                return decodeBuffer;
            }
        }


    }
}