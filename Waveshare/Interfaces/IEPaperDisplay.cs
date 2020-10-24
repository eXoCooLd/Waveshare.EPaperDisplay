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
using System.Drawing;
#endregion Usings

namespace Waveshare.Interfaces
{
    /// <summary>
    /// Interface for all E-Paper Devices
    /// </summary>
    public interface IEPaperDisplay : IDisposable
    {

        /// <summary>
        /// Pixel Width of the Display
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Pixel Height of the Display
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Wait until the display is ready
        /// </summary>
        /// <returns>true if device is ready, false for timeout</returns>
        bool WaitUntilReady();

        /// <summary>
        /// Wait until the display is ready
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>true if device is ready, false for timeout</returns>
        bool WaitUntilReady(int timeout);

        /// <summary>
        /// Power the controller on.  Do not use with SleepMode.
        /// </summary>
        void PowerOn();

        /// <summary>
        /// Power the controler off.  Do not use with SleepMode.
        /// </summary>
        void PowerOff();

        /// <summary>
        /// Send the Display into SleepMode
        /// </summary>
        void Sleep();

        /// <summary>
        /// WakeUp the Display from SleepMode
        /// </summary>
        void WakeUp();

        /// <summary>
        /// Clear the Display to White
        /// </summary>
        void Clear();

        /// <summary>
        /// Clear the Display to Black
        /// </summary>
        void ClearBlack();

        /// <summary>
        /// Reset the Display
        /// </summary>
        void Reset();

        /// <summary>
        /// Display a Image on the Display
        /// </summary>
        /// <param name="image">Bitmap that should be displayed</param>
        void DisplayImage(Bitmap image);

    }
}