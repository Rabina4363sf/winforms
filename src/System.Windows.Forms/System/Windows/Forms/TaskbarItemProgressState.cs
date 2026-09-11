// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms;

/// <summary>
///  Specifies the progress state displayed in a taskbar button.
/// </summary>
public enum TaskbarItemProgressState
{
    /// <summary>
    ///  No progress is displayed.
    /// </summary>
    NoProgress = 0,

    /// <summary>
    ///  Progress is displayed as an indeterminate animation.
    /// </summary>
    Indeterminate = 1,

    /// <summary>
    ///  Progress is displayed normally.
    /// </summary>
    Normal = 2,

    /// <summary>
    ///  Progress is displayed in an error state.
    /// </summary>
    Error = 4,

    /// <summary>
    ///  Progress is displayed in a paused state.
    /// </summary>
    Paused = 8
}
