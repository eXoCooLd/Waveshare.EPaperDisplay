#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2020 Greg Cannon

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

namespace Waveshare.Devices.Epd7in5_V2
{
    /// <summary>
    /// E-Paper Commands from the Specification
    /// https://www.waveshare.com/w/upload/6/60/7.5inch_e-Paper_V2_Specification.pdf
    /// </summary>
    // ReSharper disable UnusedMember.Global
    // ReSharper disable InconsistentNaming
    internal enum Epd7In5_V2Commands
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
        /// Power ON Measure (PMES) (R05H)
        /// </summary>
        PowerOnMeasure = 0x05,
        /// <summary>
        /// Booster Soft Start (BTST) (R06H)
        /// </summary>
        BoosterSoftStart = 0x06,
        /// <summary>
        /// Deep sleep (DSLP) (R07H)
        /// </summary>
        DeepSleep = 0x07,
        /// <summary>
        /// Data Start Transmission 1 (DTM1, White/Black) (R10H)
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
        /// Display Start transmission 2 (DTM2, Red) (R13H)
        /// </summary>
        DataStartTransmission2 = 0x13,
        /// <summary>
        /// Dual SPI (R15H)
        /// </summary>
        DualSpi = 0x15,
        /// <summary>
        /// Auto Sequence (AUTO) (R17H)
        /// </summary>
        AutoSequence = 0x17,
        /// <summary>
        /// KW LUT option (KWOPT) (R2BH)
        /// </summary>
        LutOption = 0x2b,
        /// <summary>
        /// PLL Control (PLL) (R30H)
        /// </summary>
        PllControl = 0x30,
        /// <summary>
        /// Temperature Sensor Calibration (TSC) (R40H)
        /// </summary>
        TemperatureSensorCommand = 0x40,
        /// <summary>
        /// Temperature Sensor Internal/External (TSE) (R41H)
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
        /// Panel Break Check (PBC) (R44H)
        /// </summary>
        PanelBreakCheck = 0x44,
        /// <summary>
        /// VCOM and Data Interval Setting (CDI) (R50H)
        /// </summary>
        VcomAndDataIntervalSetting = 0x50,
        /// <summary>
        /// Low Power Detection(LPD) (R51H)
        /// </summary>
        LowPowerDetection = 0x51,
        /// <summary>
        /// End Voltage Setting (EVS) (R52H)
        /// </summary>
        EndVoltageSetting = 0x52,
        /// <summary>
        /// TCON Setting (TCON) (R60h)
        /// </summary>
        TconSetting = 0x60,
        /// <summary>
        /// Resolution Setting (TRES) (R61H)
        /// </summary>
        TconResolution = 0x61,
        /// <summary>
        /// Gate/Source Start setting (GSST) (R65H)
        /// </summary>
        GateSourceStartSetting = 0x65,
        /// <summary>
        /// Revision (REV) (R70H)
        /// </summary>
        Revision = 0x70,
        /// <summary>
        /// Get status (FLG) (R71H)
        /// </summary>
        GetStatus = 0x71,
        /// <summary>
        /// Auto measure vcom (AMV) (R80H)
        /// </summary>
        AutoMeasurementVcom = 0x80,
        /// <summary>
        /// VCOM Value (VV) (R81H)
        /// </summary>
        ReadVcomValue = 0x81,
        /// <summary>
        /// VCOM-DC Setting (VDCS) (R82H)
        /// </summary>
        VcmDcSetting = 0x82,
        /// <summary>
        /// Partial Window (PTL) (R90H) **NEW
        /// </summary>
        PartialWindow = 0x90,
        /// <summary>
        /// Partial In (PTIN) (R91H) **NEW
        /// </summary>
        PartialIn = 0x91,
        /// <summary>
        /// Partial Out (PTOUT) (R92H) **NEW
        /// </summary>
        PartialOut = 0x92,
        /// <summary>
        /// Program Mode (PGM) (RA0H)
        /// </summary>
        ProgramMode = 0xA0,
        /// <summary>
        /// Active Programming (APG) (RA1H)
        /// </summary>
        ActiveProgramming = 0xA1,
        /// <summary>
        /// Read OTP (ROTP) (RA2H)
        /// </summary>
        ReadOtp = 0xA2,
        /// <summary>
        /// Cascade Setting (CCSET) (RE0H)
        /// </summary>
        CascadeSetting = 0xE0,
        /// <summary>
        /// Power Saving (PWS) (RE3H)
        /// </summary>
        PowerSaving = 0xE3,
        /// <summary>
        /// LVD Voltage Select (LVSEL) (RE4H)
        /// </summary>
        LvdVoltageSelect = 0xE4,
        /// <summary>
        /// Force Temperature (TSSET) (RE5H)
        /// </summary>
        ForceTemperature = 0xE5,
        /// <summary>
        /// Temperature Boundary Phase-C2 (TSBDRY) (RE7H)
        /// </summary>
        TemperatureBoundaryPhaseC2 = 0xE7
    }
    // ReSharper restore InconsistentNaming
    // ReSharper restore UnusedMember.Global
}