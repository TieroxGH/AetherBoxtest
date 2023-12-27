using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using System.ComponentModel;
using ImGuiNET;
using System.Reflection;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AetherBox.Helpers.Extensions;

public static class ImGuiExtra
{
    internal static void SetNextWidthWithName(string name)
    {

        ImGui.SetNextItemWidth(Math.Max(80 * ImGuiHelpers.GlobalScale, ImGui.CalcTextSize(name).X + 30 * ImGuiHelpers.GlobalScale));
    }

    public static string GetMemberName(this MemberInfo info)
    {
        if (info == null) return "Not Chosen";

        return info.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? info.Name;
    }

    const float INDENT_WIDTH = 180;

    public static unsafe ImFontPtr GetFont(float size)
    {
        var style = new Dalamud.Interface.GameFonts.GameFontStyle(Dalamud.Interface.GameFonts.GameFontStyle.GetRecommendedFamilyAndSize(Dalamud.Interface.GameFonts.GameFontFamily.Axis, size));
        var font = Svc.PluginInterface.UiBuilder.GetGameFontHandle(style).ImFont;

        if ((nint)font.NativePtr == nint.Zero)
        {
            return ImGui.GetFont();
        }
        font.Scale = size / style.BaseSizePt;
        return font;
    }

    public static unsafe bool SelectableCombo(string popUp, string[] items, ref int index)
    {
        var count = items.Length;
        var name = items[index % count] + "##" + popUp;

        var result = false;

        if (SelectableButton(name))
        {
            if (count < 3)
            {
                index = (index + 1) % count;
                result = true;
            }
            else
            {
                if (!ImGui.IsPopupOpen(popUp)) ImGui.OpenPopup(popUp);
            }
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        if (ImGui.BeginPopup(popUp))
        {
            for (var i = 0; i < count; i++)
            {
                if (ImGui.Selectable(items[i]))
                {
                    index = i;
                    result = true;
                }
            }
            ImGui.EndPopup();
        }

        return result;
    }

    public static unsafe bool SelectableButton(string name)
    {
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.HeaderActive)));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.HeaderHovered)));
        ImGui.PushStyleColor(ImGuiCol.Button, 0);
        var result = ImGui.Button(name);
        ImGui.PopStyleColor(3);
        return result;
    }

    internal static void DrawItemMiddle(Action drawAction, float wholeWidth, float width, bool leftAlign = true)
    {
        if (drawAction == null) return;
        var distance = (wholeWidth - width) / 2;
        if (leftAlign) distance = MathF.Max(distance, 0);
        ImGui.SetCursorPosX(distance);
        drawAction();
    }

    #region Image
    internal unsafe static bool SilenceImageButton(nint handle, Vector2 size, bool selected, string id = "")
    {
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.HeaderActive)));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.HeaderHovered)));
        ImGui.PushStyleColor(ImGuiCol.Button, selected ? ImGui.ColorConvertFloat4ToU32(*ImGui.GetStyleColorVec4(ImGuiCol.Header)) : 0);

        var result = NoPaddingImageButton(handle, size, id);
        ImGui.PopStyleColor(3);

        return result;
    }

    internal unsafe static bool NoPaddingNoColorImageButton(nint handle, Vector2 size, string id = "")
    {
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0);
        ImGui.PushStyleColor(ImGuiCol.Button, 0);
        var result = NoPaddingImageButton(handle, size, id);
        ImGui.PopStyleColor(3);

        return result;
    }

    internal static bool NoPaddingImageButton(nint handle, Vector2 size, string id = "")
    {
        var padding = ImGui.GetStyle().FramePadding;
        ImGui.GetStyle().FramePadding = Vector2.Zero;

        ImGui.PushID(id);
        var result = ImGui.ImageButton(handle, size);
        ImGui.PopID();
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        ImGui.GetStyle().FramePadding = padding;
        return result;
    }

    internal static bool TextureButton(IDalamudTextureWrap texture, float wholeWidth, float maxWidth, string id = "")
    {
        if (texture == null) return false;

        var size = new Vector2(texture.Width, texture.Height) * MathF.Min(1, MathF.Min(maxWidth, wholeWidth) / texture.Width);

        var result = false;
        DrawItemMiddle(() =>
        {
            result = NoPaddingNoColorImageButton(texture.ImGuiHandle, size, id);
        }, wholeWidth, size.X);
        return result;
    }
    #endregion

    #region PopUp
    public static void DrawHotKeysPopup(string key, string command, params (string name, Action action, string[] keys)[] pairs)
    {
        using var popup = ImRaii.Popup(key);
        if (popup)
        {
            if (ImGui.BeginTable(key, 2, ImGuiTableFlags.BordersOuter))
            {
                foreach (var (name, action, keys) in pairs)
                {
                    if (action == null) continue;
                    DrawHotKeys(name, action, keys);
                }
                if (!string.IsNullOrEmpty(command))
                {
                    DrawHotKeys($"Execute \"{command}\"", () => ExecuteCommand(command), "Alt");

                    DrawHotKeys($"Copy \"{command}\"", () => CopyCommand(command), "Ctrl");
                }
                ImGui.EndTable();
            }
        }
    }
    public static void PrepareGroup(string key, string command, Action reset)
    {
        DrawHotKeysPopup(key, command, ("Reset to Default Value.", reset, new string[] { "Backspace" }));
    }
    public static void ReactPopup(string key, string command, Action reset, bool showHand = true)
    {
        ExecuteHotKeysPopup(key, command, string.Empty, showHand, (reset, new VirtualKey[] { VirtualKey.BACK }));
    }

    public static void ExecuteHotKeysPopup(string key, string command, string tooltip, bool showHand, params (Action action, VirtualKey[] keys)[] pairs)
    {
        if (!ImGui.IsItemHovered()) return;
        if (!string.IsNullOrEmpty(tooltip)) ImguiTooltips.Tooltip(tooltip);

        if (showHand) ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);

        if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            if (!ImGui.IsPopupOpen(key))
            {
                ImGui.OpenPopup(key);
            }
        }

        foreach (var (action, keys) in pairs)
        {
            if (action == null) continue;
            ExecuteHotKeys(action, keys);
        }
        if (!string.IsNullOrEmpty(command))
        {
            ExecuteHotKeys(() => ExecuteCommand(command), VirtualKey.MENU);
            ExecuteHotKeys(() => CopyCommand(command), VirtualKey.CONTROL);
        }
    }

    private static void ExecuteCommand(string command)
    {
        Svc.Commands.ProcessCommand(command);
    }

    private static void CopyCommand(string command)
    {
        ImGui.SetClipboardText(command);
        Notify.Success($"\"{command}\" copied to clipboard.");
    }

    private static readonly SortedList<string, bool> _lastChecked = new();
    private static void ExecuteHotKeys(Action action, params VirtualKey[] keys)
    {
        if (action == null) return;
        var name = string.Join(' ', keys);

        if (!_lastChecked.TryGetValue(name, out var last)) last = false;
        var now = keys.All(k => Svc.KeyState[k]);
        _lastChecked[name] = now;

        if (!last && now) action();
    }

    private static void DrawHotKeys(string name, Action action, params string[] keys)
    {
        if (action == null) return;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        if (ImGui.Selectable(name))
        {
            action();
            ImGui.CloseCurrentPopup();
        }

        ImGui.TableNextColumn();
        ImGui.TextDisabled(string.Join(' ', keys));
    }
    #endregion

    public static void TextWrappedColored(Vector4 color, string text)
    {
        ImGui.PushTextWrapPos();
        ImGui.TextColored(color, text);
        ImGui.PopTextWrapPos();
    }

    public static bool IsInRect(Vector2 leftTop, Vector2 size)
    {
        var pos = ImGui.GetMousePos() - leftTop;
        if (pos.X <= 0 || pos.Y <= 0 || pos.X >= size.X || pos.Y >= size.Y) return false;
        return true;
    }
}
