#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2021 Greg Cannon

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// --------------------------------------------------------------------------------------------------------------------
#endregion Copyright

#region Usings

using System;
using System.IO;
using Waveshare.Common;

#endregion Usings

namespace Waveshare.Devices.Epd7in5b_V2
{
    /// <summary>
    /// Write image to Waveshare 7.5inch e-Paper V2 Black, White and Red Display
    /// </summary>
    internal class Epd7in5b_V2Writer : EPaperDisplayWriter
    {
        //########################################################################################

        #region Fields

        private readonly int RedIndex;
        private readonly byte RedPixel;
        private readonly int BlackIndex;
        private readonly byte BlackPixel;
        private readonly byte[] BlackLine;
        private MemoryStream memStream;
        private byte OutByte;
        private bool Disposed = false;

        #endregion Fields

        //########################################################################################

        #region Constructor / Dispose / Finalizer

        internal Epd7in5b_V2Writer(EPaperDisplayBase display) 
            : base (display)
        {
            memStream = new MemoryStream();
            RedIndex = display.GetColorIndex(ByteColors.Red);
            RedPixel = display.DeviceByteColors[RedIndex];
            BlackIndex = display.GetColorIndex(ByteColors.Black);
            BlackPixel = display.DeviceByteColors[BlackIndex];
            BlackLine = display.GetColoredLineOnDevice(ByteColors.Black);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (Disposed)
            {
                return;
            }

            if (disposing && memStream != null)
            {
                Finish();
                memStream.Close();
                memStream.Dispose();
                memStream = null;
            }

            Disposed = true;
        }

        public override void Dispose()
        {
            base.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Epd7in5b_V2Writer() => Dispose(false);

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

        #region Public Methods

        public override void Finish()
        {
            base.Finish();
            if (memStream.Length > 0)
            {
                Display.SendCommand((byte)Epd7In5b_V2Commands.DataStartTransmission2);
                memStream.Position = 0;
                Display.SendData(memStream);
            }
            OutByte = 0;
        }

        public override void Write(int index)
        {
            byte value;
            if (index == RedIndex)
            {
                base.Write(BlackIndex);
                value = RedPixel;
            }
            else
            {
                base.Write(index);
                value = BlackPixel;
            }

            if (PixelPerByte == 1)
            {
                memStream.WriteByte(value);
            }
            else
            {
                OutByte <<= BitShift;
                OutByte |= value;
                if (ByteCount % PixelPerByte == PixelThreshold)
                {
                    memStream.WriteByte(OutByte);
                    OutByte = 0;
                }
            }
        }

        public override void WriteBlankLine()
        {
            while (PixelPerByte > 1 && ByteCount % PixelPerByte != PixelThreshold)
            {
                Write(WhiteIndex);
            }

            memStream.Write(BlackLine, 0, BlackLine.Length);
            base.WriteBlankLine();
        }

        #endregion Public Methods
    }
}
