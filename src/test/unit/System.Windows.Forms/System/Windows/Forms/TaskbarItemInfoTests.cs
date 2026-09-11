// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.Windows.Forms.Tests;

public class TaskbarItemInfoTests
{
    [WinFormsFact]
    public void Form_TaskbarItemInfo_GetReturnsSameInstance()
    {
        using Form form = new();

        Assert.False(form.IsHandleCreated);
        Assert.Same(form.TaskbarItemInfo, form.TaskbarItemInfo);
        Assert.Equal(TaskbarItemProgressState.NoProgress, form.TaskbarItemInfo.ProgressState);
        Assert.Equal(0, form.TaskbarItemInfo.ProgressValue);
        Assert.False(form.IsHandleCreated);
    }

    [WinFormsFact]
    public void Form_TaskbarItemInfo_SetWithHandle_DoesNotThrow()
    {
        using Form form = new();
        form.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
        form.TaskbarItemInfo.ProgressValue = 0.5;

        _ = form.Handle;

        Assert.Equal(TaskbarItemProgressState.Normal, form.TaskbarItemInfo.ProgressState);
        Assert.Equal(0.5, form.TaskbarItemInfo.ProgressValue);
    }

    [WinFormsTheory]
    [InlineData(double.NaN, 0)]
    [InlineData(-1, 0)]
    [InlineData(0.5, 0.5)]
    [InlineData(2, 1)]
    [InlineData(double.PositiveInfinity, 1)]
    public void TaskbarItemInfo_ProgressValue_SetClamps(double value, double expected)
    {
        TaskbarItemInfo taskbarItemInfo = new();

        taskbarItemInfo.ProgressValue = value;

        Assert.Equal(expected, taskbarItemInfo.ProgressValue);
    }

    [WinFormsTheory]
    [EnumData<TaskbarItemProgressState>]
    public void TaskbarItemInfo_ProgressState_SetGetReturnsExpected(TaskbarItemProgressState value)
    {
        TaskbarItemInfo taskbarItemInfo = new();

        taskbarItemInfo.ProgressState = value;

        Assert.Equal(value, taskbarItemInfo.ProgressState);
    }
}
