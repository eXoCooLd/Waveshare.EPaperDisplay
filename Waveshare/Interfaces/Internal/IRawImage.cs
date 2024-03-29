﻿#region Copyright
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

#endregion Usings

namespace Waveshare.Interfaces.Internal
{
    /// <summary>
    /// Interface for the RAW Image that should be displayed
    /// </summary>
    internal interface IRawImage : IDisposable
    {
        /// <summary>
        /// Width of the Image or Device Width
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Height of the Image or Device Height
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Length of a ScanLine in Bytes
        /// </summary>
        int Stride { get; }

        /// <summary>
        /// Used Bytes per Pixel
        /// </summary>
        int BytesPerPixel { get; }

        /// <summary>
        /// IntPointer to the Byte Array of the Image
        /// </summary>
        IntPtr ScanLine { get; }
    }
}