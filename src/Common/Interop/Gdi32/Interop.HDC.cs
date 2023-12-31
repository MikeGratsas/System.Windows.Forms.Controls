﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

internal static partial class Interop
{
    internal static partial class Gdi32
    {
        public readonly struct HDC : IEquatable<HDC>
        {
            public IntPtr Handle { get; }

            public HDC(IntPtr handle) => Handle = handle;

            public bool IsNull => Handle == IntPtr.Zero;

            public static implicit operator IntPtr(HDC hdc) => hdc.Handle;
            public static explicit operator HDC(IntPtr hdc) => new HDC(hdc);
            public static implicit operator HGDIOBJ(HDC hdc) => new HGDIOBJ(hdc.Handle);

            public static bool operator ==(HDC value1, HDC value2) => value1.Handle == value2.Handle;
            public static bool operator !=(HDC value1, HDC value2) => value1.Handle != value2.Handle;
            public override bool Equals(object? obj) => obj is HDC hdc && hdc.Handle == Handle;
            public bool Equals(HDC other) => other.Handle == Handle;
            public override int GetHashCode() => Handle.GetHashCode();
        }
    }
}
