﻿using System.Numerics;
using FNAFStudio_Runtime_RCS.Data;
using FNAFStudio_Runtime_RCS.Data.Definitions;
using FNAFStudio_Runtime_RCS.Data.Definitions.GameObjects;
using Raylib_CsLo;

namespace FNAFStudio_Runtime_RCS.Office.Scenes;

public class CameraHandler : IScene
{
    public static float ScrollX;
    public static float Direction = -1;
    public static float timeSinceSwitch;
    public string Name => "CameraHandler";

    public async Task UpdateAsync()
    {
        foreach (var button in GameCache.Buttons.Values) await button.UpdateAsync(ScrollX);
    }

    public void Draw()
    {
        if (OfficeCore.LoadingLock || OfficeCore.Office == null || OfficeCore.OfficeState == null) return;

        var curCam = OfficeCore.OfficeState.Cameras[OfficeCore.OfficeState.Player.CurrentCamera];
        if (curCam.States.TryGetValue(curCam.State, out var path))
            if (!string.IsNullOrEmpty(path))
            {
                var curState = Cache.GetTexture(path);
                float maxScroll = Math.Abs(curState.width - 1280),
                    scroll = curCam.Scroll * 30,
                    deltaTime = Raylib.GetFrameTime() * 100;
                var velocity = Math.Clamp((int)Math.Abs(scroll - 640), 0, 640);
                timeSinceSwitch += deltaTime;

                if (timeSinceSwitch >= 500) // 5 seconds
                {
                    if (ScrollX == maxScroll || ScrollX == 0)
                    {
                        Direction = -Direction;
                        timeSinceSwitch = 0;
                    }

                    ScrollX += Direction * (velocity < 320 ? 0.1f : velocity < 640 ? 0.3f : 0.5f) * deltaTime;
                }

                ScrollX = Math.Clamp(ScrollX, 0, maxScroll);
                Raylib.DrawTexture(curState, (int)-Math.Round(ScrollX), 0, Raylib.WHITE);
            }

        foreach (var sprite in OfficeCore.OfficeState.CameraUI.Sprites.Values)
        {
            if (!sprite.Visible || string.IsNullOrEmpty(sprite.Sprite))
                continue;

            var position = new Vector2(sprite.X * Globals.xMagic, sprite.Y * Globals.yMagic);
            Raylib.DrawTextureEx(Cache.GetTexture(sprite.Sprite), position, 0, 1, Raylib.WHITE);
        }

        foreach (var button in OfficeCore.OfficeState.CameraUI.Buttons)
        {
            if (string.IsNullOrEmpty(button.Value.Sprite) || !button.Value.Visible)
                continue;

            string UID = $"{button.Value.ID ?? ""}{button.Value.Sprite}";
            Vector2 position = new(button.Value.X * Globals.xMagic, button.Value.Y * Globals.yMagic);
            if (!GameCache.Buttons.TryGetValue(UID, out var cachedButton))
            {
                cachedButton = new Button2D(position, texture: Cache.GetTexture(button.Value.Sprite));
                GameCache.Buttons[UID] = cachedButton;
            }

            cachedButton.Draw(position);
        }

        OfficeUtils.DrawHUD();
    }
}