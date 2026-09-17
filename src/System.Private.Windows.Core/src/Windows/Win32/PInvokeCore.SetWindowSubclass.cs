// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Windows.Win32.UI.Controls;

namespace Windows.Win32;

internal static partial class PInvokeCore
{
    [DllImport(Libraries.Comctl32, SetLastError = true)]
    private static extern BOOL SetWindowSubclassNative(
        HWND hWnd,
        SUBCLASSPROC pfnSubclass,
        nuint uIdSubclass,
        nuint dwRefData);

    [DllImport(Libraries.Comctl32, SetLastError = true)]
    private static extern BOOL RemoveWindowSubclassNative(
        HWND hWnd,
        SUBCLASSPROC pfnSubclass,
        nuint uIdSubclass);

    [DllImport(Libraries.Comctl32)]
    private static extern LRESULT DefSubclassProcNative(
        HWND hWnd,
        uint uMsg,
        WPARAM wParam,
        LPARAM lParam);

    public static bool SetWindowSubclass(
        HWND hWnd,
        SUBCLASSPROC pfnSubclass,
        nuint uIdSubclass,
        nuint dwRefData)
    {
        if (SetWindowSubclassNative(hWnd, pfnSubclass, uIdSubclass, dwRefData))
        {
            return true;
        }

        return false;
    }

    public static bool RemoveWindowSubclass(
        HWND hWnd,
        SUBCLASSPROC pfnSubclass,
        nuint uIdSubclass)
    {
        if (RemoveWindowSubclassNative(hWnd, pfnSubclass, uIdSubclass))
        {
            return true;
        }

        return false;
    }

    public static LRESULT DefSubclassProc(
        HWND hWnd,
        uint uMsg,
        WPARAM wParam,
        LPARAM lParam)
        => DefSubclassProcNative(hWnd, uMsg, wParam, lParam);
}
