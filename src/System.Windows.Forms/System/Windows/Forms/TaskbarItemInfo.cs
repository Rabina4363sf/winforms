// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace System.Windows.Forms;

/// <summary>
///  Provides information about the progress indicator displayed in a form's taskbar button.
/// </summary>
/// <remarks>
///  <para>
///   The progress indicator is displayed by the Windows shell and is available on
///   Windows 7 and later. On unsupported systems, setting the properties has no effect.
///  </para>
///  <para>
///   Use the <see cref="Form.TaskbarItemInfo"/> property to associate this information
///   with a <see cref="Form"/>.
///  </para>
/// </remarks>
public sealed class TaskbarItemInfo
{
    private const uint CLSCTX_INPROC_SERVER = 1;
    private const int S_OK = 0;

    private static readonly Guid s_taskbarListClassId = new("56FDF344-FD6D-11D0-958A-006097C9A090");
    private static readonly Guid s_taskbarList3InterfaceId = new("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF");

    private readonly Form? _owner;
    private ITaskbarList3? _taskbarList;
    private TaskbarItemProgressState _progressState;
    private double _progressValue;

    /// <summary>
    ///  Initializes a new instance of the <see cref="TaskbarItemInfo"/> class.
    /// </summary>
    public TaskbarItemInfo()
    {
    }

    internal TaskbarItemInfo(Form owner)
    {
        _owner = owner;
    }

    /// <summary>
    ///  Gets or sets the state of the progress indicator in the taskbar button.
    /// </summary>
    /// <remarks>
    ///  Setting this property updates the taskbar button when the associated form has
    ///  a handle.
    /// </remarks>
    public TaskbarItemProgressState ProgressState
    {
        get => _progressState;
        set
        {
            if (_progressState == value)
            {
                return;
            }

            _progressState = value;
            UpdateTaskbarProgress();
        }
    }

    /// <summary>
    ///  Gets or sets the progress value displayed in the taskbar button.
    /// </summary>
    /// <value>
    ///  A value between 0 and 1. Values outside this range are clamped.
    ///  <see cref="double.NaN"/> is treated as 0.
    /// </value>
    /// <remarks>
    ///  Setting this property updates the taskbar button when the associated form has
    ///  a handle.
    /// </remarks>
    public double ProgressValue
    {
        get => _progressValue;
        set
        {
            double progressValue = double.IsNaN(value) ? 0 : Math.Clamp(value, 0, 1);

            if (_progressValue == progressValue)
            {
                return;
            }

            _progressValue = progressValue;
            UpdateTaskbarProgress();
        }
    }

    internal void OnHandleCreated()
        => UpdateTaskbarProgress();

    internal void Dispose()
    {
        if (_taskbarList is not null)
        {
            Marshal.FinalReleaseComObject(_taskbarList);
            _taskbarList = null;
        }
    }

    private void UpdateTaskbarProgress()
    {
        if (_owner is null || !_owner.IsHandleCreated || !OperatingSystem.IsWindowsVersionAtLeast(6, 1))
        {
            return;
        }

        _taskbarList ??= CreateTaskbarList();

        if (_taskbarList is null)
        {
            return;
        }

        nint handle = _owner.Handle;
        try
        {
            _taskbarList.SetProgressState(handle, (uint)_progressState);

            if (_progressState is TaskbarItemProgressState.Normal
                or TaskbarItemProgressState.Error
                or TaskbarItemProgressState.Paused)
            {
                const ulong Maximum = 10_000;
                _taskbarList.SetProgressValue(handle, (ulong)(_progressValue * Maximum), Maximum);
            }
        }
        catch (COMException)
        {
            // The taskbar may be unavailable even when the operating system supports it.
            Dispose();
        }
    }

    private static ITaskbarList3? CreateTaskbarList()
    {
        int hr = CoCreateInstance(
            s_taskbarListClassId,
            IntPtr.Zero,
            CLSCTX_INPROC_SERVER,
            s_taskbarList3InterfaceId,
            out ITaskbarList3? taskbarList);

        if (hr != S_OK || taskbarList is null || taskbarList.HrInit() != S_OK)
        {
            if (taskbarList is not null)
            {
                Marshal.FinalReleaseComObject(taskbarList);
            }

            return null;
        }

        return taskbarList;
    }

    [DllImport("ole32.dll", ExactSpelling = true)]
    private static extern int CoCreateInstance(
        in Guid rclsid,
        nint pUnkOuter,
        uint dwClsContext,
        in Guid riid,
        [MarshalAs(UnmanagedType.Interface)] out ITaskbarList3? ppv);

    /// <summary>
    ///  Provides access to the Windows taskbar progress APIs.
    /// </summary>
    [ComImport]
    [Guid("EA1AFB91-9E28-4B86-90E9-9E9F8A5EEFAF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITaskbarList3
    {
        int HrInit();
        int AddTab(nint hwnd);
        int DeleteTab(nint hwnd);
        int ActivateTab(nint hwnd);
        int SetActiveAlt(nint hwnd);
        int MarkFullscreenWindow(nint hwnd, [MarshalAs(UnmanagedType.Bool)] bool fullscreen);
        int SetProgressValue(nint hwnd, ulong completed, ulong total);
        int SetProgressState(nint hwnd, uint state);
    }
}
