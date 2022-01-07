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

#endregion Usings

namespace Waveshare.Common
{
    /// <summary>
    /// Structure for holding a three byte color
    /// </summary>
    internal struct ByteColor : IEquatable<ByteColor>
    {

        //########################################################################################

        #region Properties

        /// <summary>
        /// Red
        /// </summary>
        public byte R { get; private set; }

        /// <summary>
        /// Green
        /// </summary>
        public byte G { get; private set; }

        /// <summary>
        /// Blue
        /// </summary>
        public byte B { get; private set; }

        /// <summary>
        /// Check if the Color is Monochrom
        /// </summary>
        public bool IsMonochrome { get; private set; }

        #endregion Properties

        //########################################################################################

        #region Constructor / Dispose / Finalizer

        /// <summary>
        /// Constructor with the color Bytes
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="desaturate"></param>
        public ByteColor(byte red, byte green, byte blue, bool desaturate = false)
            : this()
        {
            R = red;
            B = blue;
            G = green;
            Update(desaturate);
        }

        #endregion Constructor / Dispose / Finalizer

        //########################################################################################

        #region Public Methods

        /// <summary>
        /// Desaturate the color
        /// </summary>
        public void Desaturate()
        {
            if (R != G || G != B)
            {
                // Widely established color weights for grayscale
                const double redWeight = 0.299;
                const double greenWeight = 0.587;
                const double blueWeight = 0.114;

                R = (byte)(R * redWeight + G * greenWeight + B * blueWeight + .005);
                G = R;
                B = R;
            }
        }

        /// <summary>
        /// Set Color Bytes blue, green, red
        /// </summary>
        /// <param name="blue"></param>
        /// <param name="green"></param>
        /// <param name="red"></param>
        /// <param name="desaturate"></param>
        // ReSharper disable once InconsistentNaming
        public void SetBGR(byte blue, byte green, byte red, bool desaturate = false)
        {
            R = red;
            B = blue;
            G = green;
            Update(desaturate);
        }

        /// <summary>
        /// Override the Equals Operator
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) => obj is ByteColor other && this.Equals(other);

        /// <summary>
        /// Equals Operator
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Equals(ByteColor p) => R == p.R && G == p.G && B == p.B;

        /// <summary>
        /// Override the HashCode generation
        /// </summary>
        /// <returns></returns>
        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => (R, G, B).GetHashCode();
        // ReSharper restore NonReadonlyMemberInGetHashCode

        /// <summary>
        /// Override the Equals Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(ByteColor lhs, ByteColor rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Override the Not Equals Operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(ByteColor lhs, ByteColor rhs) => !lhs.Equals(rhs);

        #endregion Public Methods

        //########################################################################################

        #region Private Methods

        /// <summary>
        /// Update the Color
        /// </summary>
        /// <param name="desaturate"></param>
        private void Update(bool desaturate)
        {
            IsMonochrome = (R == G && G == B);
            if (!IsMonochrome && desaturate)
            {
                Desaturate();
                IsMonochrome = true;
            }
        }

        #endregion Private Methods

        //########################################################################################

    }
}
