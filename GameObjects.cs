using MoonSharp.Interpreter;
using Spectre.Console;
using System.Net.Sockets;
using System.Text;

namespace Lunar.Core;

public class Part : LunarObject
{
    public Part(string name = "") : base("Part", name)
    {
        Properties["Position"] = new { X = 0, Y = 0, Z = 0 };
        Properties["Color"] = "White";
        Properties["Transparency"] = 0.0;
    }

    public void MoveTo(double x, double y, double z)
    {
        Properties["Position"] = new { X = x, Y = y, Z = z };
        AnsiConsole.MarkupLine($"[grey]  Part '{Name}' moved to [[{x}, {y}, {z}]][/]");
    }
}

public class Sound : LunarObject
{
    public Sound(string name = "") : base("Sound", name)
    {
        Properties["SoundId"] = "";
        Properties["Volume"] = 1.0;
        Properties["Playing"] = false;
    }

    public void Play()
    {
        Properties["Playing"] = true;
        AnsiConsole.MarkupLine($"[blue]♫ Playing sound '{Name}' (ID: {Properties["SoundId"]})[/]");
    }

    public void Stop()
    {
        Properties["Playing"] = false;
        AnsiConsole.MarkupLine($"[grey]■ Stopped sound '{Name}'[/]");
    }
}

public class GameScript : LunarObject
{
    public PermissionLevel RequiredLevel { get; set; } = PermissionLevel.User;
    public bool IsTcpBound { get; set; } = false;

    public GameScript(string className, string name = "") : base(className, name)
    {
        if (className.ToLower() == "modulescript" || className.ToLower() == "gamescript")
        {
            RequiredLevel = PermissionLevel.System;
        }

        if (className.ToLower() == "gamescript")
        {
            IsTcpBound = true;
        }
    }

    public void Run(LunarEngine engine)
    {
        if (IsTcpBound)
        {
            AnsiConsole.MarkupLine($"[bold red]🛡️ GUARDIAN DISPATCH: {Name} -> Isolated Sandbox[/]");
            
            // Send logic to Guardian Sandbox via TCP
            _ = Task.Run(async () => {
                try {
                    using var client = new TcpClient("127.0.0.1", 5556);
                    using var stream = client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(Source ?? "-- Empty GameScript");
                    await stream.WriteAsync(data, 0, data.Length);
                } catch (Exception ex) {
                    AnsiConsole.MarkupLine($"[red]!! Guardian Link Failure: {ex.Message}[/]");
                }
            });
            return;
        }

        if (!string.IsNullOrEmpty(Source))
        {
            var prev = engine.Security.CurrentLevel;
            engine.Security.CurrentLevel = RequiredLevel;

            try 
            {
                AnsiConsole.MarkupLine($"[yellow]▶ Executing {ClassName}: {Name} (Level {(int)RequiredLevel})[/]");
                engine.DoString(Source, Name);
            }
            finally
            {
                engine.Security.CurrentLevel = prev;
            }
        }
    }
}
