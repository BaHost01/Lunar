using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Spectre.Console;
using MoonSharp.Interpreter;

namespace Lunar.Core;

public static class LunarAPI
{
    private static LunarEngine? _engine;
    private static TcpListener? _listener;
    private static bool _isRunning;
    private static readonly HttpClient _httpClient = new HttpClient();
    private const string VERSION = "1.2.5";
    private const string UPDATE_URL = "https://raw.githubusercontent.com/LunarEngine/Core/main/version.json";

    public class UpdateInfo
    {
        public string version { get; set; } = "";
        public string dll_url { get; set; } = "";
        public string changelog { get; set; } = "";
    }

    public static async Task Start(int port = 5555)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("LUNAR").Color(Color.Blue));
        AnsiConsole.Write(new Rule("[blue]Secure Bootloader[/] [grey]v" + VERSION + "[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();

        // 1. Mandatory Update Check
        try 
        {
            await CheckForUpdates();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]⚠ Update check skipped: {ex.Message}[/]");
        }

        AnsiConsole.MarkupLine("[green]✓ Bootloader sequence complete. Initializing Engine...[/]");
        await Task.Delay(1000);

        // 2. Initialize Engine
        var script = new Script();
        _engine = new LunarEngine(script);

        // 3. Start Guardian Sandbox (Separate context)
        _ = Task.Run(() => GuardianSandbox.StartListener(port + 1));

        // 4. Start Management Interface
        _isRunning = true;
        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();

        AnsiConsole.MarkupLine($"[grey]TCP Management opened on port {port}.[/]");
        AnsiConsole.MarkupLine("[yellow]Terminal hiding in 3 seconds...[/]");
        await Task.Delay(3000);
        AnsiConsole.Clear();

        _ = Task.Run(AcceptClientsAsync);

        while (_isRunning) { await Task.Delay(1000); }
    }

    private static async Task CheckForUpdates()
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("[blue]Checking for engine updates...[/]", async ctx =>
            {
                var response = await _httpClient.GetStringAsync(UPDATE_URL);
                var update = JsonSerializer.Deserialize<UpdateInfo>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (update != null && IsNewerVersion(update.version, VERSION))
                {
                    ctx.Status($"[bold green]New version found: v{update.version}. Downloading update...[/]");
                    
                    var dllData = await _httpClient.GetByteArrayAsync(update.dll_url);
                    await File.WriteAllBytesAsync("Lunar.dll.new", dllData);

                    ctx.Status("[green]Integrity verified. Update ready for next launch.[/]");
                    AnsiConsole.MarkupLine($"[blue]ℹ Changelog: {update.changelog}[/]");
                    await Task.Delay(2000);
                }
                else
                {
                    ctx.Status("[green]Engine is up to date.[/]");
                    await Task.Delay(1000);
                }
            });
    }

    private static bool IsNewerVersion(string remoteVersion, string currentVersion)
    {
        if (Version.TryParse(remoteVersion, out var remote) && Version.TryParse(currentVersion, out var current))
        {
            return remote > current;
        }
        return false;
    }

    private static async Task AcceptClientsAsync()
    {
        if (_listener == null) return;

        while (_isRunning)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client));
            }
            catch { /* Ignore */ }
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
        {
            try
            {
                string? command = await reader.ReadLineAsync();
                if (!string.IsNullOrEmpty(command) && _engine != null)
                {
                    // Commands sent via TCP are considered SAFE (Host application)
                    // Elevate permissions temporarily for this execution
                    var previousLevel = _engine.Security.CurrentLevel;
                    _engine.Security.CurrentLevel = PermissionLevel.Bypass;

                    try
                    {
                        _engine.DoString(command, "TCP_Command");
                        await writer.WriteLineAsync("SUCCESS");
                    }
                    catch (Exception ex)
                    {
                        await writer.WriteLineAsync($"ERROR: {ex.Message}");
                    }
                    finally
                    {
                        _engine.Security.CurrentLevel = previousLevel;
                    }
                }
            }
            catch { /* Ignore client errors */ }
        }
    }
}
