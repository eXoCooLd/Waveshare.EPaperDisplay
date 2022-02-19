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

namespace Waveshare.Devices.Epd5in65F
{
    /// <summary>
    /// E-Paper Commands from the Specification
    /// https://www.waveshare.com/w/upload/7/7a/5.65inch_e-Paper_(F)_Sepecification.pdf
    /// </summary>
    // ReSharper disable UnusedMember.Global
    internal enum Epd5in65fCommands
    {
        /// <summary>
        /// Default value
        /// </summary>
        None = -1,
        /// <summary>
        /// Panel Setting (PSR) (R00H)
        /// </summary>
        PanelSetting = 0x00,
        /// <summary>
        /// Power Setting (PWR) (R01H)
        /// </summary>
        PowerSetting = 0x01,
        /// <summary>
        /// Power OFF (POF) (R02H)
        /// </summary>
        PowerOff = 0x02,
        /// <summary>
        /// Power OFF Sequence Setting(PFS) (R03H)
        /// </summary>
        PowerOffSequenceSetting = 0x03,
        /// <summary>
        /// Power ON (PON) (R04H)
        /// </summary>
        PowerOn = 0x04,
        /// <summary>
        /// Booster Soft Start (BTST) (R06H)
        /// </summary>
        BoosterSoftStart = 0x06,
        /// <summary>
        /// Deep sleep (DSLP) (R07H)
        /// </summary>
        DeepSleep = 0x07,
        /// <summary>
        /// Data Start Transmission 1 (DTM1) (R10H)
        /// </summary>
        DataStartTransmission1 = 0x10,
        /// <summary>
        /// Data stop (DSP) (R11H)
        /// </summary>
        DataStop = 0x11,
        /// <summary>
        /// Display Refresh (DRF) (R12H)
        /// </summary>
        DisplayRefresh = 0x12,
        /// <summary>
        /// Image Process Command (IPC) (R13H)
        /// </summary>
        ImageProcess = 0x13,
        /// <summary>
        /// PLL Control (PLL) (R30H)
        /// </summary>
        PllControl = 0x30,
        /// <summary>
        /// Temperature Sensor Calibration(TSC) (R40H)
        /// </summary>
        TemperatureSensorCommand = 0x40,
        /// <summary>
        /// Temperature Sensor Internal/External(TSE) (R41H)
        /// </summary>
        TemperatureCalibration = 0x41,
        /// <summary>
        /// Temperature Sensor Write (TSW) (R42H)
        /// </summary>
        TemperatureSensorWrite = 0x42,
        /// <summary>
        /// Temperature Sensor Read (TSR) (R43H)
        /// </summary>
        TemperatureSensorRead = 0x43,
        /// <summary>
        /// VCOM and Data Interval Setting(CDI) (R50H)
        /// </summary>
        VcomAndDataIntervalSetting = 0x50,
        /// <summary>
        /// Low Power Detection(LPD) (R51h)
        /// </summary>
        LowPowerDetection = 0x51,
        /// <summary>
        /// TCON Setting(TCON) (R60h) Undocumented!
        /// </summary>
        TconSetting = 0x60,
        /// <summary>
        /// Resolution Setting(TRES) (R61H)
        /// </summary>
        TconResolution = 0x61,
        /// <summary>
        /// Get status(FLG) (R71H)
        /// </summary>
        GetStatus = 0x71,
        /// <summary>
        /// VCOM Value(VV) (R81h)
        /// </summary>
        ReadVcomValue = 0x81,
        /// <summary>
        /// VCOM-DC Setting(VDCS) (R82H)
        /// </summary>
        VcmDcSetting = 0x82,
        /// <summary>
        /// Flash Mode (RE5H) Undocumented!
        /// </summary>
        FlashMode = 0xE3
    }
    // ReSharper restore UnusedMember.Global
}
