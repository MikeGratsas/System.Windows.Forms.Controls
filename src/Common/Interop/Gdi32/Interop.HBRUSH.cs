﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

internal static partial class Interop
{
    internal static partial class Gdi32
    {
        public readonly struct HBRUSH
        {
            public IntPtr Handle { get; }

            public HBRUSH(IntPtr handle) => Handle = handle;

            public bool IsNull => Handle == 0;

            public static implicit operator IntPtr(HBRUSH hbrush) => hbrush.Handle;
            public static explicit operator HBRUSH(IntPtr hbrush) => new(hbrush);
            public static implicit operator HGDIOBJ(HBRUSH hbrush) => new(hbrush.Handle);
            public static explicit operator HBRUSH(HGDIOBJ hbrush) => new(hbrush.Handle);
        }
    }
}
