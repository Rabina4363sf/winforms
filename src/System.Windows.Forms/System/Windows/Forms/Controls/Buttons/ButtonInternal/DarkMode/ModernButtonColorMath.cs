// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Windows.Forms.Rendering.Button;
using System.Windows.Forms.VisualStyles;

namespace System.Windows.Forms;

/// <summary>
///  Resolves the modern button palette and state colors shared by button renderers and appearance defaults.
/// </summary>
internal static class ModernButtonColorMath
{
    internal const float MinimumTextContrastRatio = PopupButtonColorMath.MinimumReadableContrastRatio;
    private const float DefaultHoverLightenAmount = 0.08f;
    private const float DefaultPressedDarkenAmount = 0.12f;

    /// <summary>
    ///  Identifies the designated accept button independently of the temporary default state caused by focus.
    /// </summary>
    internal static bool IsAcceptButton(ButtonBase owner)
        => ReferenceEquals(owner.FindForm()?.AcceptButton, owner);

    internal static Color GetDefaultButtonColor(PushButtonState state)
        => GetDefaultButtonColor(Application.SystemVisualSettings.AccentColor, state);

    internal static Color GetDefaultButtonColor(Color accentColor, PushButtonState state)
        => state switch
        {
            PushButtonState.Hot => PopupButtonColorMath.Lighten(accentColor, DefaultHoverLightenAmount),
            PushButtonState.Pressed => PopupButtonColorMath.Darken(accentColor, DefaultPressedDarkenAmount),
            _ => accentColor
        };

    internal static Color GetReadableForeColor(Color backColor)
    {
        float blackContrast = PopupButtonColorMath.GetContrastRatio(Color.Black, backColor);
        float whiteContrast = PopupButtonColorMath.GetContrastRatio(Color.White, backColor);
        Color foreColor = PopupButtonColorMath.GetReadableForeColor(backColor);

        Debug.Assert(
            Math.Max(blackContrast, whiteContrast) >= MinimumTextContrastRatio,
            "Either black or white should meet WCAG AA contrast against an opaque background.");

        return foreColor;
    }

    /// <summary>
    ///  Resolves appearance defaults from the same palette used to paint the button.
    /// </summary>
    internal static Color GetStateColor(ButtonBase owner, FlatButtonAppearance appearance, PushButtonState state)
    {
        ButtonDarkModeRendererBase renderer = owner.FlatStyle switch
        {
            FlatStyle.Standard or FlatStyle.Popup => new ModernButtonDarkModeRenderer(),
            FlatStyle.Flat => new ModernFlatButtonRenderer(),
            FlatStyle.System => new SystemButtonDarkModeRenderer(),
            _ => throw new ArgumentOutOfRangeException(nameof(owner))
        };

        renderer.DeviceDpi = owner.DeviceDpi;
        renderer.FlatAppearance = appearance;

        bool isDefault = IsAcceptButton(owner);
        Color customBaseColor = !isDefault && owner.ShouldSerializeBackColor()
            ? owner.BackColor
            : Color.Empty;

        return renderer.GetBackgroundColor(state, isDefault, customBaseColor);
    }
}
