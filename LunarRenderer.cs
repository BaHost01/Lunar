using Raylib_cs;
using System.Numerics;
using MoonSharp.Interpreter;

namespace Lunar.Core;

public static class LunarRenderer
{
    private static Camera3D _camera;
    private static bool _windowOpen = false;

    public static void Initialize(int width = 1280, int height = 720, string title = "Lunar Engine 3D")
    {
        if (_windowOpen) return;

        Raylib.InitWindow(width, height, title);
        Raylib.SetTargetFPS(60);

        _camera = new Camera3D
        {
            Position = new Vector3(10.0f, 10.0f, 10.0f),
            Target = new Vector3(0.0f, 0.0f, 0.0f),
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            FovY = 45.0f,
            Projection = CameraProjection.Perspective
        };

        _windowOpen = true;
    }

    public static void Register(Script script)
    {
        var rendererTable = new Table(script);
        
        rendererTable["init"] = (Action<int, int, string>)Initialize;
        rendererTable["beginDraw"] = (Action)BeginDraw;
        rendererTable["endDraw"] = (Action)EndDraw;
        rendererTable["drawCube"] = (Action<float, float, float, float, float, float, string>)DrawCube;
        rendererTable["drawGrid"] = (Action<int, float>)Raylib.DrawGrid;
        rendererTable["close"] = (Action)Raylib.CloseWindow;
        rendererTable["shouldClose"] = (Func<bool>)(() => Raylib.WindowShouldClose());
        
        script.Globals["renderer"] = rendererTable;
    }

    public static void BeginDraw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Raylib.BeginMode3D(_camera);
    }

    public static void EndDraw()
    {
        Raylib.EndMode3D();
        Raylib.DrawFPS(10, 10);
        Raylib.EndDrawing();
    }

    public static void DrawCube(float x, float y, float z, float w, float h, float d, string colorName)
    {
        Color color = colorName.ToLower() switch
        {
            "red" => Color.Red,
            "blue" => Color.Blue,
            "green" => Color.Green,
            "white" => Color.White,
            "gold" => Color.Gold,
            _ => Color.Gray
        };
        Raylib.DrawCube(new Vector3(x, y, z), w, h, d, color);
        Raylib.DrawCubeWires(new Vector3(x, y, z), w, h, d, Color.Black);
    }

    public static void UpdateCamera(float px, float py, float pz, float tx, float ty, float tz)
    {
        _camera.Position = new Vector3(px, py, pz);
        _camera.Target = new Vector3(tx, ty, tz);
    }
}
