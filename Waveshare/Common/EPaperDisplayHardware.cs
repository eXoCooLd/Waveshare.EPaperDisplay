#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2019 Andre Wehrli

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
using System.Device.Gpio;
using System.Device.Spi;
using System.IO;
using Waveshare.Interfaces;

#endregion Usings

namespace Waveshare.Common
{
    /// <summary>
    /// E-Paper Display Hardware on the SPI Bus
    /// </summary>
    internal sealed class EPaperDisplayHardware : IEPaperDisplayHardware
    {

        //########################################################################################

        #region Constants

        /// <summary>
        /// GPIO Reset Pin Number
        /// </summary>
        private const int GpioResetPin = 17;

        /// <summary>
        /// GPIO SPI DC Pin Number
        /// </summary>
        private const int GpioSpiDcPin = 25;

        /// <summary>
        /// GPIO SPI CS Pin Number
        /// </summary>
        private const int GpioSpiCsPin = 8;

        /// <summary>
        /// GPIO Busy Pin Number
        /// </summary>
        private const int GpioBusyPin = 24;

        #endregion Constants

        //########################################################################################

        #region Fields

        private bool Disposed = false;

        #endregion Fields

        //########################################################################################

        #region Properties

        /// <summary>
        /// SPI Bus Device
        /// </summary>
        internal SpiDevice SpiDevice { get; private set; }

        /// <summary>
        /// GPIO Controller Device
        /// </summary>
        internal GpioController GpioController { get; private set; }

        /// <summary>
        /// GPIO Reset Pin
        /// </summary>
        public PinValue ResetPin
        {
            get => GpioController.Read(GpioResetPin);
            set => GpioController.Write(GpioResetPin, value);
        }

        /// <summary>
        /// GPIO SPI DC Pin
        /// </summary>
        public PinValue SpiDcPin
        {
            get => GpioController.Read(GpioSpiDcPin);
            set => GpioController.Write(GpioSpiDcPin, value);
        }

        /// <summary>
        /// GPIO SPI CS Pin
        /// </summary>
        public PinValue SpiCsPin
        {
            get => GpioController.Read(GpioSpiCsPin);
            set => GpioController.Write(GpioSpiCsPin, value);
        }

        /// <summary>
        /// GPIO Busy Pin
        /// </summary>
        public PinValue BusyPin
        {
            get => GpioController.Read(GpioBusyPin);
            set => GpioController.Write(GpioBusyPin, value);
        }

        #endregion Properties

        //########################################################################################

        #region Constructor / Dispose / Finalizer

        /// <summary>
        /// Default Constructor
        /// </summary>
        public EPaperDisplayHardware() : this(CreateSpiDevice(), CreateGpioController())
        {

        }

        /// <summary>
        /// Internal Constructor with Hardware Interfaces (SPI and GPIO)
        /// </summary>
        /// <param name="spiDevice"></param>
        /// <param name="gpioController"></param>
        internal EPaperDisplayHardware(SpiDevice spiDevice, GpioController gpioController)
        {
            GpioController = gpioController;

            GpioController?.OpenPin(GpioResetPin);
            GpioController?.OpenPin(GpioSpiDcPin);
            GpioController?.OpenPin(GpioSpiCsPin);
            GpioController?.OpenPin(GpioBusyPin);

            GpioController?.SetPinMode(GpioResetPin, PinMode.Output);
            GpioController?.SetPinMode(GpioSpiDcPin, PinMode.Output);
            GpioController?.SetPinMode(GpioSpiCsPin, PinMode.Output);
            GpioController?.SetPinMode(GpioBusyPin, PinMode.Input);

            GpioController?.Write(GpioSpiCsPin, PinValue.High);

            SpiDevice = spiDevice;
        }

        /// <summary>
        /// Dispose the SPI and GPIO Devices
        /// </summary>
        public void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                GpioController?.Write(GpioSpiCsPin, PinValue.Low);
                GpioController?.Write(GpioSpiDcPin, PinValue.Low);
                GpioController?.Write(GpioResetPin, PinValue.Low);

                GpioController?.Dispose();
                GpioController = null;

                SpiDevice?.Dispose();
                SpiDevice = null;
            }

            Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EPaperDisplayHardware() => Dispose(false);

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Write stream to the SPI device
        /// </summary>
        /// <param name="stream">The stream that contains the data to be written to the SPI device</param>
        public void Write(MemoryStream stream)
        {
            byte[] buffer = new byte[Math.Min(4096, stream.Length)];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            while (bytesRead == buffer.Length)
            {
                SpiDevice?.Write(buffer);
                bytesRead = stream.Read(buffer, 0, buffer.Length);
            }

            if (bytesRead > 0 && bytesRead < buffer.Length)
            {
                Array.Resize(ref buffer, bytesRead);
                SpiDevice?.Write(buffer);
            }
        }

        /// <summary>
        /// Write data to the SPI device
        /// </summary>
        /// <param name="buffer">The buffer that contains the data to be written to the SPI device</param>
        public void Write(byte[] buffer)
        {
            SpiDevice?.Write(buffer);
        }

        /// <summary>
        /// Write a byte to the SPI device
        /// </summary>
        /// <param name="value">The byte to be written to the SPI device</param>
        public void WriteByte(byte value)
        {
            SpiDevice?.WriteByte(value);
        }

        #endregion Public Methods

        //########################################################################################

        #region Private Methods
        /// <summary>
        /// Create the GPIO Controller
        /// </summary>
        /// <returns></returns>
        private static GpioController CreateGpioController()
        {
            var gpioController = new GpioController();
            return gpioController;
        }

        /// <summary>
        /// Create the SPI Device
        /// </summary>
        /// <returns></returns>
        private static SpiDevice CreateSpiDevice()
        {
            var spiConnectionSettings = new SpiConnectionSettings(0, 0);
            return SpiDevice.Create(spiConnectionSettings);
        }

        #endregion Private Methods

        //########################################################################################

    }
}