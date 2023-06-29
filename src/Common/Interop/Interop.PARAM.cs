// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Drawing;

internal partial class Interop
{
    /// <summary>
    ///  Helpers for creating W/LPARAM arguments for messages.
    /// </summary>
    internal static class PARAM
    {
        public static IntPtr FromLowHigh(int low, int high) => ToInt(low, high);

        public static IntPtr FromLowHighUnsigned(int low, int high)
            // Convert the int to an uint before converting it to a pointer type,
            // which ensures the high dword being zero for 64-bit pointers.
            // This corresponds to the logic of the MAKELPARAM/MAKEWPARAM/MAKELRESULT
            // macros.
            => (IntPtr)(uint)ToInt(low, high);

        public static int ToInt(int low, int high)
            => (high << 16) | (low & 0xffff);

        public static int HIWORD(int n)
            => (n >> 16) & 0xffff;

        public static int LOWORD(int n)
            => n & 0xffff;

        public static int LOWORD(IntPtr n)
            => LOWORD((int)n);

        public static int HIWORD(IntPtr n)
            => HIWORD((int)n);

        public static int SignedHIWORD(IntPtr n)
            => SignedHIWORD((int)n);

        public static int SignedLOWORD(IntPtr n)
            => SignedLOWORD(unchecked((int)n));

        public static int SignedHIWORD(int n)
            => (short)HIWORD(n);

        public static int SignedLOWORD(int n)
            => (short)LOWORD(n);

        public static IntPtr FromBool(bool value)
            => (IntPtr)(value ? BOOL.TRUE : BOOL.FALSE);

        /// <summary>
        ///  Hard casts to <see langword="int" /> without bounds checks.
        /// </summary>
        public static int ToInt(IntPtr param) => (int)param;

        /// <summary>
        ///  Hard casts to <see langword="uint" /> without bounds checks.
        /// </summary>
        public static uint ToUInt(IntPtr param) => (uint)param;

        /// <summary>
        ///  Packs a <see cref="Point"/> into a PARAM.
        /// </summary>
        public static IntPtr FromPoint(Point point)
            => PARAM.FromLowHigh(point.X, point.Y);

        /// <summary>
        ///  Unpacks a <see cref="Point"/> from a PARAM.
        /// </summary>
        public static Point ToPoint(IntPtr param)
            => new(SignedLOWORD(param), SignedHIWORD(param));
    }
}
