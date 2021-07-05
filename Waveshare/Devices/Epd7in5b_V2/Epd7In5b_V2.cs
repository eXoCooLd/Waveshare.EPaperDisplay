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

using System.Threading;
using Waveshare.Common;
using Waveshare.Interfaces.Internal;

#endregion Usings

namespace Waveshare.Devices.Epd7in5b_V2
{
    /// <summary>
    /// Type: Waveshare 7.5inch e-Paper V2
    /// Color: Black, White and Red
    /// Display Resolution: 800*480
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal sealed class Epd7In5b_V2 : EPaperDisplayBase
    {

        //########################################################################################

        #region Properties

        /// <summary>
        /// Pixels per Byte on the Device
        /// </summary>
        public override int PixelPerByte { get; } = 8;

        /// <summary>
        /// Pixel Width of the Display
        /// </summary>
        public override int Width { get; } = 800;

        /// <summary>
        /// Pixel Height of the Display
        /// </summary>
        public override int Height { get; } = 480;

        /// <summary>
        /// Supported Colors of the E-Paper Device
        /// </summary>
        public override ByteColor[] SupportedByteColors { get; } = { ByteColors.White, ByteColors.Black, ByteColors.Red };

        /// <summary>
        /// Color Bytes of the E-Paper Device corresponding to the supported colors
        /// </summary>
        public override byte[] DeviceByteColors { get; } = { Epd7In5b_V2Colors.White, Epd7In5b_V2Colors.Black, Epd7In5b_V2Colors.Red };

        /// <summary>
        /// Get Status Command
        /// </summary>
        protected override byte GetStatusCommand { get; } = (byte)Epd7In5b_V2Commands.GetStatus;

        /// <summary>
        /// Start Data Transmission Command
        /// </summary>
        protected override byte StartDataTransmissionCommand { get; } = (byte)Epd7In5b_V2Commands.DataStartTransmission1;

        /// <summary>
        /// Stop Data Transmission Command
        /// </summary>
        protected override byte StopDataTransmissionCommand { get; } = byte.MaxValue;

        #endregion Properties

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Clear the Display to White
        /// </summary>
        public override void Clear()
        {
            FillColor(Epd7In5b_V2Commands.DataStartTransmission1, ByteColors.White);
            FillColor(Epd7In5b_V2Commands.DataStartTransmission2, ByteColors.Black);
            TurnOnDisplay();
        }

        /// <summary>
        /// Clear the Display to Black
        /// </summary>
        public override void ClearBlack()
        {
            FillColor(Epd7In5b_V2Commands.DataStartTransmission1, ByteColors.Black);
            FillColor(Epd7In5b_V2Commands.DataStartTransmission2, ByteColors.Black);
            TurnOnDisplay();
        }

        /// <summary>
        /// Power the controller on.
        /// Do not use with SleepMode.
        /// </summary>
        public override void PowerOn()
        {
            SendCommand(Epd7In5b_V2Commands.PowerOn);
            DeviceWaitUntilReady();
        }

        /// <summary>
        /// Power the controller off.
        /// Do not use with SleepMode.
        /// </summary>
        public override void PowerOff()
        {
            SendCommand(Epd7In5b_V2Commands.PowerOff);
            DeviceWaitUntilReady();
        }

        /// <summary>
        /// Send the Display into SleepMode
        /// </summary>
        public override void Sleep()
        {
            PowerOff();
            SendCommand(Epd7In5b_V2Commands.DeepSleep);
            SendData(0xA5);
        }

        /// <summary>
        /// Wait until the display is ready
        /// </summary>
        public void DeviceWaitUntilReady()
        {
            WaitUntilReady();
            Thread.Sleep(200);
        }

        #endregion Public Methods

        //########################################################################################

        #region Protected Methods

        /// <summary>
        /// Device specific Initializer
        /// </summary>
        protected override void DeviceInitialize()
        {
            Reset();

            SendCommand(Epd7In5b_V2Commands.BoosterSoftStart);
            SendData(0x17);
            SendData(0x17);
            SendData(0x27);
            SendData(0x17);

            SendCommand(Epd7In5b_V2Commands.PowerSetting);
            SendData(0x07); // VGH: 20V
            SendData(0x07); // VGL: -20V
            SendData(0x3f); // VDH: 15V
            SendData(0x3f); // VDL: -15V

            SendCommand(Epd7In5b_V2Commands.PowerOn);
            Thread.Sleep(100);
            DeviceWaitUntilReady();

            SendCommand(Epd7In5b_V2Commands.PanelSetting);
            SendData(0x0F); // KW-3f   KWR-2F	BWROTP 0f	BWOTP 1f

            SendCommand(Epd7In5b_V2Commands.TconResolution);
            SendData(0x03); // source 800
            SendData(0x20);
            SendData(0x01); // gate 480
            SendData(0xe0);

            SendCommand(Epd7In5b_V2Commands.DualSpi);
            SendData(0x00);

            SendCommand(Epd7In5b_V2Commands.VcomAndDataIntervalSetting);
            SendData(0x11);
            SendData(0x07);

            SendCommand(Epd7In5b_V2Commands.TconSetting);
            SendData(0x22);

            SendCommand(Epd7In5b_V2Commands.GateSourceStartSetting);  // Resolution setting
            SendData(0x00);
            SendData(0x00);//800*480
            SendData(0x00);
            SendData(0x00);
        }

        /// <summary>
        /// Turn the Display PowerOn after a Sleep
        /// </summary>
        protected override void TurnOnDisplay()
        {
            SendCommand(Epd7In5b_V2Commands.DisplayRefresh);
            Thread.Sleep(100);
            DeviceWaitUntilReady();
        }

        /// <summary>
        /// Convert a pixel to a DataByte
        /// </summary>
        /// <param name="rgb">color bytes</param>
        /// <returns>Pixel converted to specific byte value for the hardware</returns>
        protected override byte ColorToByte(ByteColor rgb)
        {
            if (!rgb.IsMonochrome && !IsRed(rgb))
            {
                rgb.Desaturate();
            }

            return rgb.R >= 128 ? Epd7In5b_V2Colors.Red : Epd7In5b_V2Colors.NotRed;
        }

        /// <summary>
        /// Generate a display writer for this device
        /// </summary>
        /// <returns>Returns a display writer</returns>
        protected override IEPaperDisplayWriter GetDisplayWriter()
        {
            return new Epd7In5BV2Writer(this);
        }

        #endregion Protected Methods

        //########################################################################################


        #region Private Methods

        /// <summary>
        /// Check if a Pixel is Red
        /// </summary>
        /// <param name="rgb"></param>
        /// <returns></returns>
        private static bool IsRed(ByteColor rgb)
        {
            return rgb.R > (rgb.G + 20) && rgb.R > (rgb.B + 20) && rgb.R >= 64;
        }

        /// <summary>
        /// Helper to send a Command based o the Epd7In5b_V2Commands Enum
        /// </summary>
        /// <param name="command">Command to send</param>
        private void SendCommand(Epd7In5b_V2Commands command)
        {
            SendCommand((byte)command);
        }

        /// <summary>
        /// Fill the screen with a color
        /// </summary>
        /// <param name="command">Start Data Transmission Command</param>
        /// <param name="rgb">Color to fill the screen</param>
        private void FillColor(Epd7In5b_V2Commands command, ByteColor rgb)
        {
            var outputLine = GetColoredLineOnDevice(rgb);

            SendCommand(command);

            for (var y = 0; y < Height; y++)
            {
                SendData(outputLine);
            }
        }

        #endregion Private Methods

        //########################################################################################

    }
}
