// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Windows.Win32.UI.Controls;

internal delegate LRESULT SUBCLASSPROC(
    HWND hWnd,
    uint uMsg,
    WPARAM wParam,
    LPARAM lParam,
    nuint uIdSubclass,
    nuint dwRefData);
