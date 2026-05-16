using MoonSharp.Interpreter;
using Spectre.Console;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;

namespace Lunar.Core;

/// <summary>
/// The Guardian Sandbox is a separate execution context for GameScripts.
/// Features: Randomized identity, String Obfuscation, and Guarded Execution.
/// </summary>
public class GuardianSandbox
{
    private readonly Script _script;
    private readonly string _identity;

    public GuardianSandbox()
    {
        _script = new Script();
        _identity = "G_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        
        InjectMemoryNoise();
        RegisterGlobals();
    }

    private void InjectMemoryNoise()
    {
        // Inject 100+ random strings into the global table to confuse memory scanners/dumpers
        for (int i = 0; i < 150; i++)
        {
            string noiseKey = "ptr_" + Guid.NewGuid().ToString("N").Substring(0, 16);
            string noiseVal = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            _script.Globals[noiseKey] = noiseVal;
        }
    }

    private void RegisterGlobals()
    {
        _script.Globals["print"] = (Action<DynValue>)(val => 
            AnsiConsole.MarkupLine($"[bold blue][{_identity}][/] [grey]>[/] {Markup.Escape(val.ToString())}"));
            
        _script.Globals["warn"] = (Action<string>)(msg => 
            AnsiConsole.MarkupLine($"[bold yellow][{_identity}] ⚠[/] {Markup.Escape(msg)}"));
    }

    public void Execute(string code)
    {
        try
        {
            // The sandbox runs in Level 8 context by default for GameScripts
            _script.DoString(code);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red][{_identity}] EXCEPTION:[/] {Markup.Escape(ex.Message)}");
        }
    }

    public static async Task StartListener(int port)
    {
        var listener = new TcpListener(System.Net.IPAddress.Any, port);
        listener.Start();

        AnsiConsole.MarkupLine($"[bold blue]>> Guardian Sandbox Listener Active (Identity: {Guid.NewGuid().ToString("N")})[/]");

        while (true)
        {
            try
            {
                using var client = await listener.AcceptTcpClientAsync();
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream);
                
                string? code = await reader.ReadToEndAsync();
                if (!string.IsNullOrEmpty(code))
                {
                    var sandbox = new GuardianSandbox();
                    _ = Task.Run(() => sandbox.Execute(code));
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]!! Guardian Sandbox Error:[/] {Markup.Escape(ex.Message)}");
            }
        }
    }
}
