#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// MIT License
// Copyright(c) 2021 Andre Wehrli

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
using Waveshare.Interfaces;

#endregion Usings

namespace Waveshare.Image
{
    /// <summary>
    /// Base Class for Image Loading into a E-Paper Display
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class EPaperImageBase<T> : IEPaperDisplayImage<T>
    {

        //########################################################################################

        #region Properties

        /// <summary>
        /// Internal E-Paper Display
        /// </summary>
        private IEPaperDisplayInternal EPaperDisplay { get; set; }

        /// <summary>
        /// Pixel Width of the Display
        /// </summary>
        public int Width => EPaperDisplay.Width;

        /// <summary>
        /// Pixel Height of the Display
        /// </summary>
        public int Height => EPaperDisplay.Height;

        #endregion Properties

        //########################################################################################

        #region Constructor / Dispose / Finalizer

        /// <summary>
        /// Constructor with internal E-Paper Display
        /// </summary>
        /// <param name="ePaperDisplay"></param>
        protected EPaperImageBase(IEPaperDisplayInternal ePaperDisplay)
        {
            EPaperDisplay = ePaperDisplay;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~EPaperImageBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            EPaperDisplay?.Dispose();
            EPaperDisplay = null;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose for sub classes
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {

        }

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Wait until the display is ready
        /// </summary>
        /// <returns>true if device is ready, false for timeout</returns>
        public bool WaitUntilReady() => EPaperDisplay.WaitUntilReady();

        /// <summary>
        /// Wait until the display is ready
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns>true if device is ready, false for timeout</returns>
        public bool WaitUntilReady(int timeout) => EPaperDisplay.WaitUntilReady(timeout);

        /// <summary>
        /// Power the controller on.  Do not use with SleepMode.
        /// </summary>
        public void PowerOn() => EPaperDisplay.PowerOn();

        /// <summary>
        /// Power the controller off.  Do not use with SleepMode.
        /// </summary>
        public void PowerOff() => EPaperDisplay.PowerOff();

        /// <summary>
        /// Send the Display into SleepMode
        /// </summary>
        public void Sleep() => EPaperDisplay.Sleep();

        /// <summary>
        /// WakeUp the Display from SleepMode
        /// </summary>
        public void WakeUp() => EPaperDisplay.WakeUp();

        /// <summary>
        /// Clear the Display to White
        /// </summary>
        public void Clear() => EPaperDisplay.Clear();

        /// <summary>
        /// Clear the Display to Black
        /// </summary>
        public void ClearBlack() => EPaperDisplay.ClearBlack();

        /// <summary>
        /// Reset the Display
        /// </summary>
        public void Reset() => EPaperDisplay.Reset();

        /// <summary>
        /// Display a Image on the Display
        /// </summary>
        /// <param name="image">Image that should be displayed</param>
        public void DisplayImage(T image)
        {
            using (var rawImage = LoadImage(image))
            {
                EPaperDisplay.ColorBytesPerPixel = rawImage.BytesPerPixel;
                EPaperDisplay.DisplayImage(rawImage);
            }
        }

        #endregion Public Methods

        //########################################################################################

        #region Protected Methods

        /// <summary>
        /// Load a Image into the RawImage
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        protected abstract IRawImage LoadImage(T image);

        #endregion Protected Methods

        //########################################################################################

    }
}