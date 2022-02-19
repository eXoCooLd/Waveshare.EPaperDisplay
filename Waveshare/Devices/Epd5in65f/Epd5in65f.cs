#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2022 Andre Wehrli

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
using Waveshare.Devices.Epd5in65F;

#endregion Usings

namespace Waveshare.Devices.Epd5in65F
{
    /// <summary>
    /// Type: Waveshare 5.65inch e-Paper (F)
    /// Color: Black, White, Green, Blue, Red, Yellow and Orange
    /// Display Resolution: 600*448 
    /// </summary>
    internal class Epd5in65f : EPaperDisplayBase
    {

        //########################################################################################

        #region Properties

        /// <summary>
        /// Pixels per Byte on the Device
        /// </summary>
        public override int PixelPerByte { get; } = 2;

        /// <summary>
        /// Pixel Width of the Display
        /// </summary>
        public override int Width { get; } = 600;

        /// <summary>
        /// Pixel Height of the Display
        /// </summary>
        public override int Height { get; } = 448;

        /// <summary>
        /// Supported Colors of the E-Paper Device
        /// </summary>
        public override ByteColor[] SupportedByteColors { get; } = { ByteColors.Black, ByteColors.White, ByteColors.Green, ByteColors.Blue, ByteColors.Red, ByteColors.Yellow, ByteColors.Orange };

        /// <summary>
        /// Color Bytes of the E-Paper Device corresponding to the supported colors
        /// </summary>
        public override byte[] DeviceByteColors { get; } = { Epd5in65fColors.Black, Epd5in65fColors.White, Epd5in65fColors.Green, Epd5in65fColors.Blue, Epd5in65fColors.Red, Epd5in65fColors.Yellow, Epd5in65fColors.Orange };

        /// <summary>
        /// Get Status Command
        /// </summary>
        protected override byte GetStatusCommand { get; } = (byte)Epd5in65fCommands.GetStatus;

        /// <summary>
        /// Start Data Transmission Command
        /// </summary>
        protected override byte StartDataTransmissionCommand { get; } = (byte)Epd5in65fCommands.DataStartTransmission1;

        /// <summary>
        /// Stop Data Transmission Command
        /// </summary>
        protected override byte StopDataTransmissionCommand { get; } = (byte)Epd5in65fCommands.DataStop;

        /// <summary>
        /// Display DeepSleep Command
        /// </summary>
        protected override byte DeepSleepComand { get; } = (byte)Epd5in65fCommands.DeepSleep;

        #endregion Properties

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Clear the Display to White
        /// </summary>
        public override void Clear()
        {
            FillColor(ByteColors.White);
            TurnOnDisplay();
        }

        /// <summary>
        /// Clear the Display to Black
        /// </summary>
        public override void ClearBlack()
        {
            FillColor(ByteColors.Black);
            TurnOnDisplay();
        }

        /// <summary>
        /// Power the controller on.  Do not use with SleepMode.
        /// </summary>
        public override void PowerOn()
        {
            SendCommand(Epd5in65fCommands.PowerOn);
            WaitUntilReady();
        }

        /// <summary>
        /// Power the controller off.  Do not use with SleepMode.
        /// </summary>
        public override void PowerOff()
        {
            SendCommand(Epd5in65fCommands.PowerOff);
            WaitUntilReady();
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

            SendCommand(Epd5in65fCommands.PanelSetting); // POWER_SETTING
            SendData(0xEF);
            SendData(0x08);

            SendCommand(Epd5in65fCommands.PowerSetting);
            SendData(0x37);
            SendData(0x00);
            SendData(0x23);
            SendData(0x23);

            SendCommand(Epd5in65fCommands.PowerOffSequenceSetting);
            SendData(0x00);

            SendCommand(Epd5in65fCommands.PowerOn);
            SendData(0xC7);
            SendData(0xC7);
            SendData(0x1D);

            SendCommand(Epd5in65fCommands.PllControl);
            SendData(0x3C);

            SendCommand(Epd5in65fCommands.TemperatureCalibration);
            SendData(0x00);

            SendCommand(Epd5in65fCommands.VcomAndDataIntervalSetting);
            SendData(0x37);

            SendCommand(Epd5in65fCommands.TconSetting);
            SendData(0x22);

            SendCommand(Epd5in65fCommands.TconResolution);
            SendData(0x02);
            SendData(0x58);
            SendData(0x01);
            SendData(0xC0);

            SendCommand(Epd5in65fCommands.FlashMode);
            SendData(0xAA);
        }

        /// <summary>
        /// Turn the Display PowerOn after a Sleep
        /// </summary>
        protected override void TurnOnDisplay()
        {
            SendCommand(Epd5in65fCommands.PowerOn);
            WaitUntilReady();
            SendCommand(Epd5in65fCommands.DisplayRefresh);
            Thread.Sleep(100);
            WaitUntilReady();
        }

        /// <summary>
        /// Convert a pixel to a DataByte
        /// </summary>
        /// <param name="rgb">color bytes</param>
        /// <returns>Pixel converted to specific byte value for the hardware</returns>
        protected override byte ColorToByte(ByteColor rgb)
        {
            if (rgb.IsMonochrome)
            {
                return rgb.R <= 85 ? Epd5in65fColors.Black : Epd5in65fColors.White;
            }

            if (rgb == ByteColors.Black)
            {
                return Epd5in65fColors.Black;
            }

            if (rgb == ByteColors.White)
            {
                return Epd5in65fColors.White;
            }

            if (rgb == ByteColors.Green)
            {
                return Epd5in65fColors.Green;
            }

            if (rgb == ByteColors.Blue)
            {
                return Epd5in65fColors.Blue;
            }

            if (rgb == ByteColors.Red)
            {
                return Epd5in65fColors.Red;
            }

            if (rgb == ByteColors.Yellow)
            {
                return Epd5in65fColors.Yellow;
            }

            if (rgb == ByteColors.Orange)
            {
                return Epd5in65fColors.Orange;
            }

            return Epd5in65fColors.Clean;
        }

        #endregion Protected Methods

        //########################################################################################

        #region Private Methods

        /// <summary>
        /// Helper to send a Command based o the Epd7In5BcCommands Enum
        /// </summary>
        /// <param name="command">Command to send</param>
        private void SendCommand(Epd5in65fCommands command)
        {
            SendCommand((byte)command);
        }

        /// <summary>
        /// Fill the screen with a color
        /// </summary>
        /// <param name="rgb">Color to fill the screen</param>
        private void FillColor(ByteColor rgb)
        {
            var outputLine = GetColoredLineOnDevice(rgb);

            SendCommand(StartDataTransmissionCommand);

            for (var y = 0; y < Height; y++)
            {
                SendData(outputLine);
            }

            SendCommand(StopDataTransmissionCommand);
        }

        #endregion Private Methods

        //########################################################################################

    }
}
