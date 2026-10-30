// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms.UITests;

public class AnchorLayoutTests : ControlTestBase
{
    private const AnchorStyles AnchorAllDirection = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

    public AnchorLayoutTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [WinFormsTheory]
    [InlineData(AnchorStyles.Top, 120, 30, 20, 30)]
    [InlineData(AnchorStyles.Left, 20, 180, 20, 30)]
    [InlineData(AnchorStyles.Right, 220, 180, 20, 30)]
    [InlineData(AnchorStyles.Bottom, 120, 330, 20, 30)]
    [InlineData(AnchorStyles.Top | AnchorStyles.Left, 20, 30, 20, 30)]
    [InlineData(AnchorStyles.Top | AnchorStyles.Right, 220, 30, 20, 30)]
    [InlineData(AnchorStyles.Top | AnchorStyles.Bottom, 120, 30, 20, 330)]
    [InlineData(AnchorStyles.Left | AnchorStyles.Right, 20, 180, 220, 30)]
    [InlineData(AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left, 20, 30, 220, 330)]
    public void Control_ResizeAnchoredControls_ParentHandleCreated_NewAnchorsApplied(AnchorStyles anchors, int expectedX, int expectedY, int expectedWidth, int expectedHeight)
    {
        LaunchFormAndVerify(anchors, expectedX, expectedY, expectedWidth, expectedHeight);
    }

    [WinFormsTheory]
    [InlineData(AnchorStyles.Top, 120, 30, 20, 30)]
    [InlineData(AnchorStyles.Left, 20, 180, 20, 30)]
    [InlineData(AnchorStyles.Right, 220, 180, 20, 30)]
    [InlineData(AnchorStyles.Bottom, 120, 330, 20, 30)]
    [InlineData(AnchorStyles.Top | AnchorStyles.Left, 20, 30, 20, 30)]
    [InlineData(AnchorStyles.Top | AnchorStyles.Right, 220, 30, 20, 30)]
    [InlineData(AnchorStyles.Top | AnchorStyles.Bottom, 120, 30, 20, 330)]
    [InlineData(AnchorStyles.Left | AnchorStyles.Right, 20, 180, 220, 30)]
    [InlineData(AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left, 20, 30, 220, 330)]
    public void Control_AnchorLayoutV2_ResizeAnchoredControls_ParentHandleCreated_NewAnchorsApplied(AnchorStyles anchors, int expectedX, int expectedY, int expectedWidth, int expectedHeight)
    {
        using AnchorLayoutV2Scope scope = new(enable: true);
        LaunchFormAndVerify(anchors, expectedX, expectedY, expectedWidth, expectedHeight);
    }

    [WinFormsFact]
    public void Control_NotParented_AnchorsNotComputed()
    {
        using AnchorLayoutV2Scope scope = new(enable: true);
        (Form form, Button button) = GetFormWithAnchoredButton(AnchorAllDirection);

        try
        {
            DefaultLayout.AnchorInfo? anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.Null(anchorInfo);

            // Unparent button and resume layout.
            form.Controls.Remove(button);
            form.ResumeLayout(performLayout: false);

            anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.Null(anchorInfo);
        }
        finally
        {
            Dispose(form, button);
        }
    }

    [WinFormsFact]
    public void Control_SuspendedLayout_AnchorsNotComputed()
    {
        using AnchorLayoutV2Scope scope = new(enable: true);
        (Form form, Button button) = GetFormWithAnchoredButton(AnchorAllDirection);

        try
        {
            DefaultLayout.AnchorInfo? anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.Null(anchorInfo);

            form.Controls.Add(button);
            anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.Null(anchorInfo);
        }
        finally
        {
            Dispose(form, button);
        }
    }

    [WinFormsFact]
    public void Control_ResumedLayout_AnchorsComputed()
    {
        using AnchorLayoutV2Scope scope = new(enable: true);
        (Form form, Button button) = GetFormWithAnchoredButton(AnchorAllDirection);

        try
        {
            DefaultLayout.AnchorInfo? anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.Null(anchorInfo);

            form.Controls.Add(button);
            anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.Null(anchorInfo);

            form.ResumeLayout(performLayout: false);
            anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.NotNull(anchorInfo);
        }
        finally
        {
            Dispose(form, button);
        }
    }

    [WinFormsFact]
    public void ConfigSwitch_Disabled_SuspendedLayout_AnchorsComputed()
    {
        using AnchorLayoutV2Scope scope = new(enable: false);
        (Form form, Button button) = GetFormWithAnchoredButton(AnchorAllDirection);

        try
        {
            form.Controls.Add(button);

            DefaultLayout.AnchorInfo? anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.NotNull(anchorInfo);
        }
        finally
        {
            Dispose(form, button);
        }
    }

    [WinFormsFact]
    public void NestedContainer_AnchorsComputed()
    {
        using AnchorLayoutV2Scope scope = new(enable: true);
        (Form form, Button button) = GetFormWithAnchoredButton(AnchorAllDirection);
        try
        {
            using ContainerControl container = new();
            container.Dock = DockStyle.Fill;
            container.SuspendLayout();
            container.Controls.Add(button);

            DefaultLayout.AnchorInfo? anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.Null(anchorInfo);

            form.Controls.Add(container);
            Assert.Null(anchorInfo);

            container.ResumeLayout(false);
            form.ResumeLayout(false);
            anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.NotNull(anchorInfo);
        }
        finally
        {
            Dispose(form, button);
        }
    }

    [WinFormsFact]
    public void ParentChanged_AnchorsUpdated()
    {
        using AnchorLayoutV2Scope scope = new(enable: true);
        (Form form, Button button) = GetFormWithAnchoredButton(AnchorAllDirection);
        try
        {
            using ContainerControl container = new();
            container.Dock = DockStyle.Fill;
            container.SuspendLayout();
            container.Controls.Add(button);

            DefaultLayout.AnchorInfo? anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.Null(anchorInfo);

            form.Controls.Add(container);
            Assert.Null(anchorInfo);

            container.ResumeLayout(false);
            form.ResumeLayout(false);

            anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.NotNull(anchorInfo);

            container.Controls.Remove(button);
            Assert.NotNull(anchorInfo);

            var previousDisplayRect = anchorInfo.DisplayRectangle;
            form.Controls.Add(button);
            Assert.NotEqual(previousDisplayRect, anchorInfo.DisplayRectangle);
        }
        finally
        {
            Dispose(form, button);
        }
    }

    [WinFormsFact]
    public void AnchorLayoutV2_ReplaceParentWhileAncestorLayoutSuspended_AnchorsRemainConsistent()
    {
        // Regression test for https://github.com/dotnet/winforms/issues/14500
        using AnchorLayoutV2Scope scope = new(enable: true);
        using Form form = new() { ClientSize = new Size(400, 300) };
        using Panel originalPanel = new()
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Location = new Point(20, 30),
            Size = new Size(100, 120)
        };
        using Panel replacementPanel = new();

        form.Controls.Add(originalPanel);
        form.ResumeLayout(performLayout: true);

        form.ClientSize = new Size(500, 350);
        Assert.Equal(new Rectangle(20, 30, 200, 170), originalPanel.Bounds);

        ReplaceControl(originalPanel, replacementPanel);
        Assert.Equal(originalPanel.Anchor, replacementPanel.Anchor);
        Assert.Equal(new Rectangle(20, 30, 200, 170), replacementPanel.Bounds);

        form.ClientSize = new Size(600, 400);
        Assert.Equal(new Rectangle(20, 30, 300, 220), replacementPanel.Bounds);

        static void ReplaceControl(Control controlToRemove, Control controlToAdd)
        {
            controlToRemove.SuspendLayout();
            controlToAdd.SuspendLayout();
            controlToAdd.Anchor = controlToRemove.Anchor;
            controlToAdd.Dock = controlToRemove.Dock;
            controlToAdd.Location = controlToRemove.Location;
            controlToAdd.Size = controlToRemove.Size;
            controlToRemove.ResumeLayout(true);
            controlToAdd.ResumeLayout(true);

            Control parentOfControlToRemove = controlToRemove.Parent!;
            Control? parentOfControlToAdd = controlToAdd.Parent;
            int index = parentOfControlToRemove.Controls.IndexOf(controlToRemove);
            parentOfControlToRemove.SuspendLayout();
            if (parentOfControlToAdd is { } parentToSuspend)
            {
                parentToSuspend.SuspendLayout();
            }

            parentOfControlToRemove.Controls.Remove(controlToRemove);
            parentOfControlToRemove.Controls.Add(controlToAdd);
            parentOfControlToRemove.Controls.SetChildIndex(controlToAdd, index);
            parentOfControlToRemove.ResumeLayout(true);

            if (parentOfControlToAdd is { } parentToResume)
            {
                parentToResume.ResumeLayout(true);
            }
        }
    }

    [WinFormsFact]
    public void SetBoundsOnAnchoredControl_BoundsChanged()
    {
        using AnchorLayoutV2Scope scope = new(enable: true);
        (Form form, Button button) = GetFormWithAnchoredButton(AnchorAllDirection);
        try
        {
            form.Controls.Add(button);

            DefaultLayout.AnchorInfo? anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.Null(anchorInfo);

            form.ResumeLayout(false);

            anchorInfo = DefaultLayout.GetAnchorInfo(button);
            Assert.NotNull(anchorInfo);

            var bounds = button.Bounds;
            button.SetBounds(bounds.X + 5, bounds.Y + 5, bounds.Width + 10, bounds.Height + 10, BoundsSpecified.None);
            Assert.Equal(bounds, button.Bounds); // Bounds Specified is None.

            button.SetBounds(bounds.X + 5, bounds.Y + 5, bounds.Width + 10, bounds.Height + 10, BoundsSpecified.All);
            Assert.NotEqual(bounds, button.Bounds); // Bounds Specified is None.

            bounds = button.Bounds;
            button.SetBounds(bounds.X + 5, bounds.Y + 5, bounds.Width + 10, bounds.Height + 10);
            Assert.NotEqual(bounds, button.Bounds);
        }
        finally
        {
            Dispose(form, button);
        }
    }

    [WinFormsTheory]
    [InlineData(false, 0.5f)]
    [InlineData(true, 0.5f)]
    [InlineData(false, 2f)]
    [InlineData(true, 2f)]
    public void Maximized_FontChanged_RightBottomAnchoredControl_ScalesAnchorDistancesAndSize(bool useAnchorLayoutV2, float fontScale)
    {
        using AnchorLayoutV2Scope scope = new(enable: useAnchorLayoutV2);
        using Form form = new()
        {
            AutoScaleMode = AutoScaleMode.Font,
            ClientSize = new Size(800, 600)
        };

        using Button button = new()
        {
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            Location = new Point(600, 400),
            Size = new Size(100, 30),
            MinimumSize = new Size(100, 30),
            MaximumSize = new Size(100, 30)
        };

        form.Controls.Add(button);

        using Font scaledFont = new(form.Font.FontFamily, form.Font.Size * fontScale);
        Rectangle boundsBefore = default;
        Rectangle displayRectangleBefore = default;
        Size clientSizeBefore = default;
        SizeF scaleFactor = default;

        form.WindowState = FormWindowState.Maximized;
        form.Shown += OnFormShown;
        form.ShowDialog();

        Assert.Equal(clientSizeBefore, form.ClientSize);
        AssertScaledAnchorDistances(button, boundsBefore, displayRectangleBefore, scaleFactor);
        Size expectedSize = new(
            (int)Math.Round(boundsBefore.Width * scaleFactor.Width),
            (int)Math.Round(boundsBefore.Height * scaleFactor.Height));
        Assert.Equal(expectedSize, button.Size);
        Assert.Equal(expectedSize, button.MinimumSize);
        Assert.Equal(expectedSize, button.MaximumSize);

        return;

        void OnFormShown(object? sender, EventArgs e)
        {
            boundsBefore = button.Bounds;
            displayRectangleBefore = form.DisplayRectangle;
            clientSizeBefore = form.ClientSize;
            SizeF dimensionsBefore = form.AutoScaleDimensions;

            form.Font = scaledFont;

            scaleFactor = new SizeF(
                form.AutoScaleDimensions.Width / dimensionsBefore.Width,
                form.AutoScaleDimensions.Height / dimensionsBefore.Height);
            form.Close();
        }
    }

    [WinFormsTheory]
    [InlineData(false, 0.5f, false, DockStyle.None)]
    [InlineData(true, 0.5f, false, DockStyle.None)]
    [InlineData(false, 2f, false, DockStyle.None)]
    [InlineData(true, 2f, false, DockStyle.None)]
    [InlineData(false, 0.5f, true, DockStyle.None)]
    [InlineData(true, 0.5f, true, DockStyle.None)]
    [InlineData(false, 2f, true, DockStyle.None)]
    [InlineData(true, 2f, true, DockStyle.None)]
    [InlineData(false, 0.5f, false, DockStyle.Fill)]
    [InlineData(true, 0.5f, false, DockStyle.Fill)]
    [InlineData(false, 2f, false, DockStyle.Fill)]
    [InlineData(true, 2f, false, DockStyle.Fill)]
    public void Maximized_FontChanged_NestedAnchoredControl_ScalesAnchorDistances(
        bool useAnchorLayoutV2,
        float fontScale,
        bool useContainerControl,
        DockStyle dock)
    {
        using AnchorLayoutV2Scope scope = new(enable: useAnchorLayoutV2);
        using Form form = new()
        {
            AutoScaleMode = AutoScaleMode.Font,
            ClientSize = new Size(652, 375)
        };
        using Control panel = useContainerControl ? new UserControl() : new Panel();
        panel.Anchor = AnchorAllDirection;
        panel.Bounds = new Rectangle(12, 72, 615, 164);
        panel.Dock = dock;
        using Panel footer = new()
        {
            Dock = DockStyle.Bottom,
            Height = 115
        };
        using Button button = new()
        {
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            Bounds = new Rectangle(495, 114, 100, 30)
        };
        panel.Controls.Add(button);
        form.Controls.Add(panel);
        form.Controls.Add(footer);

        using Font scaledFont = new(form.Font.FontFamily, form.Font.Size * fontScale);
        Rectangle panelBoundsBefore = default;
        Rectangle buttonBoundsBefore = default;
        Rectangle formDisplayRectangleBefore = default;
        Rectangle panelDisplayRectangleBefore = default;
        Size clientSizeBefore = default;
        int footerHeightBefore = 0;
        SizeF scaleFactor = default;

        form.WindowState = FormWindowState.Maximized;
        form.Shown += OnFormShown;
        form.ShowDialog();

        Assert.Equal(clientSizeBefore, form.ClientSize);
        AssertScaledAnchorDistances(panel, panelBoundsBefore, formDisplayRectangleBefore, scaleFactor);
        AssertScaledAnchorDistances(button, buttonBoundsBefore, panelDisplayRectangleBefore, scaleFactor);
        Assert.Equal(form.DisplayRectangle.Bottom, footer.Bottom);
        Assert.Equal(form.DisplayRectangle.Width, footer.Width);
        Assert.Equal((int)Math.Round(footerHeightBefore * scaleFactor.Height), footer.Height);

        return;

        void OnFormShown(object? sender, EventArgs e)
        {
            panelBoundsBefore = panel.Bounds;
            buttonBoundsBefore = button.Bounds;
            formDisplayRectangleBefore = form.DisplayRectangle;
            panelDisplayRectangleBefore = panel.DisplayRectangle;
            clientSizeBefore = form.ClientSize;
            footerHeightBefore = footer.Height;
            SizeF dimensionsBefore = form.AutoScaleDimensions;

            form.Font = scaledFont;

            scaleFactor = new SizeF(
                form.AutoScaleDimensions.Width / dimensionsBefore.Width,
                form.AutoScaleDimensions.Height / dimensionsBefore.Height);
            form.Close();
        }
    }

    [WinFormsTheory]
    [InlineData(false, FormWindowState.Normal, 0.5f)]
    [InlineData(true, FormWindowState.Normal, 0.5f)]
    [InlineData(false, FormWindowState.Maximized, 0.5f)]
    [InlineData(true, FormWindowState.Maximized, 0.5f)]
    [InlineData(false, FormWindowState.Normal, 2f)]
    [InlineData(true, FormWindowState.Normal, 2f)]
    [InlineData(false, FormWindowState.Maximized, 2f)]
    [InlineData(true, FormWindowState.Maximized, 2f)]
    [InlineData(false, FormWindowState.Normal, 1f)]
    [InlineData(true, FormWindowState.Normal, 1f)]
    [InlineData(false, FormWindowState.Maximized, 1f)]
    [InlineData(true, FormWindowState.Maximized, 1f)]
    public void Minimized_FontChanged_RestoredControls_ScaleAnchorDistances(
        bool useAnchorLayoutV2,
        FormWindowState restoreState,
        float fontScale)
    {
        using AnchorLayoutV2Scope scope = new(enable: useAnchorLayoutV2);
        using Form form = new()
        {
            AutoScaleMode = AutoScaleMode.Font,
            ClientSize = new Size(400, 260),
            StartPosition = FormStartPosition.Manual,
            Location = Point.Empty,
            WindowState = restoreState
        };
        using Panel panel = new()
        {
            Anchor = AnchorAllDirection,
            Bounds = new Rectangle(12, 50, 365, 150)
        };
        using Panel footer = new()
        {
            Dock = DockStyle.Bottom,
            Height = 60
        };
        using Button button = new()
        {
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
            Bounds = new Rectangle(265, 100, 80, 25)
        };
        panel.Controls.Add(button);
        form.Controls.Add(panel);
        form.Controls.Add(footer);
        using Font scaledFont = new(form.Font.FontFamily, form.Font.Size * fontScale);
        Task? testTask = null;
        form.Shown += (_, _) => testTask = VerifyRestoredLayoutAsync();
        form.ShowDialog();

        Assert.NotNull(testTask);
        Assert.True(testTask.IsCompletedSuccessfully, testTask.Exception?.ToString());

        return;

        async Task VerifyRestoredLayoutAsync()
        {
            try
            {
                await WaitForIdleAsync();
                Rectangle panelBoundsBefore = panel.Bounds;
                Rectangle buttonBoundsBefore = button.Bounds;
                Rectangle formDisplayRectangleBefore = form.DisplayRectangle;
                Rectangle panelDisplayRectangleBefore = panel.DisplayRectangle;
                int footerHeightBefore = footer.Height;
                SizeF dimensionsBefore = form.AutoScaleDimensions;

                form.WindowState = FormWindowState.Minimized;
                await WaitForIdleAsync();
                Assert.Equal(Size.Empty, form.ClientSize);

                form.Font = scaledFont;
                SizeF scaleFactor = new(
                    form.AutoScaleDimensions.Width / dimensionsBefore.Width,
                    form.AutoScaleDimensions.Height / dimensionsBefore.Height);

                form.WindowState = restoreState;
                await WaitForIdleAsync();

                Assert.Equal(restoreState, form.WindowState);
                AssertScaledAnchorDistances(panel, panelBoundsBefore, formDisplayRectangleBefore, scaleFactor);
                AssertScaledAnchorDistances(button, buttonBoundsBefore, panelDisplayRectangleBefore, scaleFactor, tolerance: 1);
                Assert.Equal((int)Math.Round(panelBoundsBefore.X * scaleFactor.Width), panel.Left);
                Assert.Equal((int)Math.Round(panelBoundsBefore.Y * scaleFactor.Height), panel.Top);
                Assert.Equal(form.DisplayRectangle.Bottom, footer.Bottom);
                Assert.Equal(form.DisplayRectangle.Width, footer.Width);
                Assert.Equal((int)Math.Round(footerHeightBefore * scaleFactor.Height), footer.Height);
            }
            finally
            {
                form.Close();
            }
        }
    }

    private static void AssertScaledAnchorDistances(
        Control control,
        Rectangle boundsBefore,
        Rectangle displayRectangleBefore,
        SizeF scaleFactor,
        int tolerance = 0)
    {
        Rectangle displayRectangle = control.Parent!.DisplayRectangle;
        int expectedRight = (int)Math.Round((displayRectangleBefore.Right - boundsBefore.Right) * scaleFactor.Width);
        int expectedBottom = (int)Math.Round((displayRectangleBefore.Bottom - boundsBefore.Bottom) * scaleFactor.Height);
        Assert.InRange(displayRectangle.Right - control.Right, expectedRight - tolerance, expectedRight + tolerance);
        Assert.InRange(displayRectangle.Bottom - control.Bottom, expectedBottom - tolerance, expectedBottom + tolerance);
    }

    private static void LaunchFormAndVerify(AnchorStyles anchors, int expectedX, int expectedY, int expectedWidth, int expectedHeight)
    {
        (Form form, Button button) = GetFormWithAnchoredButton(anchors);
        Rectangle newButtonBounds = button.Bounds;
        try
        {
            form.ResumeLayout(true);
            form.Controls.Add(button);
            form.Shown += OnFormShown;
            form.ShowDialog();

            Assert.Equal(expectedX, newButtonBounds.X);
            Assert.Equal(expectedY, newButtonBounds.Y);
            Assert.Equal(expectedWidth, newButtonBounds.Width);
            Assert.Equal(expectedHeight, newButtonBounds.Height);
        }
        finally
        {
            Dispose(form, button);
        }

        return;

        void OnFormShown(object? sender, EventArgs e)
        {
            // Resize the form to compute button anchors.
            form.Size = new Size(400, 600);
            newButtonBounds = button.Bounds;
            form.Close();
        }
    }

    private static (Form, Button) GetFormWithAnchoredButton(AnchorStyles buttonAnchors)
    {
        Form form = new()
        {
            Size = new Size(200, 300)
        };

        form.SuspendLayout();
        Button button = new()
        {
            Location = new Point(20, 30),
            Size = new Size(20, 30),
            Anchor = buttonAnchors
        };

        DefaultLayout.AnchorInfo? anchorInfo = DefaultLayout.GetAnchorInfo(button);
        Assert.Null(anchorInfo);

        return (form, button);
    }

    private static void Dispose(Form form, Button button)
    {
        button.Dispose();
        form.Dispose();
    }
}
