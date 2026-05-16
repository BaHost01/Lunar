using MoonSharp.Interpreter;
using Spectre.Console;
using System.Security.Cryptography;
using System.Reflection;
using System.Text.Json;

namespace Lunar.Core;

public class BytecodeHeader
{
    public string Magic { get; set; } = "LUNAR";
    public int Version { get; set; } = 1;
    public string Hash { get; set; } = "";
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}

public class StoryScript
{
    public string Name { get; set; } = "";
    public string OriginalSource { get; set; } = "";
    public bool IsPersistent { get; set; } = false;
    public bool IsRestored { get; set; } = false;
}

public class LunarEngine
{
    private readonly Script _script;
    private readonly List<DynValue> _activeThreads = new();
    private readonly Dictionary<string, StoryScript> _storyScripts = new();
    private readonly SecurityManager _security = new();
    private readonly Dictionary<string, int> _bytecodeSizeCheck = new();
    private bool _isZoneLoaded = true;
    private const string CACHE_FILE = "lunar_cache.json";
    private string _bypassToken = "";

    public bool IsZoneLoaded 
    { 
        get => _isZoneLoaded; 
        set {
            _isZoneLoaded = value;
            if (_isZoneLoaded) {
                AnsiConsole.MarkupLine("[bold green]>> Zone Loaded: Resuming Scripts...[/]");
            } else {
                AnsiConsole.MarkupLine("[bold yellow]>> Zone Unloaded: Scripts Paused (Loading Screen Active)[/]");
                HandleZoneUnload();
            }
        }
    }

    public LunarEngine(Script script)
    {
        _script = script;
        InitializeSecureFileSystem();
        LunarRenderer.Register(_script);
        RegisterBuiltins();
        LoadBuiltinScripts();
        TryLoadCache();
        StartSecurityWatchdog();
    }

    private void InitializeSecureFileSystem()
    {
        Directory.CreateDirectory("Scripts");
        Directory.CreateDirectory("Lua");

        string ymlPath = Path.Combine("Lua", "ApplicationLogic.yml");
        if (!File.Exists(ymlPath))
        {
            _bypassToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            string noise = Convert.ToBase64String(RandomNumberGenerator.GetBytes(128));
            string ymlContent = $"# LUNAR SECURE CONFIG\nFunctions: [Compile, Set, Run]\nRequireBypass: {_bypassToken}\n_Noise: {noise}";
            File.WriteAllText(ymlPath, CryptoUtils.Encrypt(ymlContent));
            AnsiConsole.MarkupLine("[bold green]>> Security Layer Initialized[/]");
        }
        else
        {
            try {
                string decrypted = CryptoUtils.Decrypt(File.ReadAllText(ymlPath));
                if (decrypted.Contains("RequireBypass: "))
                    _bypassToken = decrypted.Split("RequireBypass: ")[1].Split("\n")[0].Trim();
            } catch { AnsiConsole.MarkupLine("[red]!! Security Compromised[/]"); }
        }

        AutoCompileScripts();
    }

    public SecurityManager Security => _security;

    private void AutoCompileScripts()
    {
        foreach (var file in Directory.GetFiles("Scripts", "*.lua"))
        {
            string dest = Path.ChangeExtension(file, ".byenc");
            if (!File.Exists(dest) || File.GetLastWriteTime(file) > File.GetLastWriteTime(dest))
            {
                byte[] bytecode = ExportBytecode(File.ReadAllText(file));
                _bytecodeSizeCheck[dest] = bytecode.Length;
                string encryptedBc = CryptoUtils.Encrypt(Convert.ToBase64String(bytecode));
                File.WriteAllText(dest, encryptedBc);
                AnsiConsole.MarkupLine($"[grey]  Compiled & Encrypted:[/] [blue]{Path.GetFileName(dest)}[/]");
            }
        }

        foreach (var file in Directory.GetFiles("Scripts", "*.luamodule"))
        {
            string dest = Path.ChangeExtension(file, ".bymodenc");
            if (!File.Exists(dest) || File.GetLastWriteTime(file) > File.GetLastWriteTime(dest))
            {
                byte[] bytecode = ExportBytecode(File.ReadAllText(file));
                _bytecodeSizeCheck[dest] = bytecode.Length;
                string encryptedBc = CryptoUtils.Encrypt(Convert.ToBase64String(bytecode));
                File.WriteAllText(dest, encryptedBc);
                AnsiConsole.MarkupLine($"[grey]  Module Compiled & Encrypted:[/] [blue]{Path.GetFileName(dest)}[/]");
            }
        }
    }

    private void StartSecurityWatchdog()
    {
        Task.Run(async () => {
            while (true)
            {
                foreach (var entry in _bytecodeSizeCheck)
                {
                    if (File.Exists(entry.Key))
                    {
                        var info = new FileInfo(entry.Key);
                        if (info.Length == 0) continue;
                    }
                }
                await Task.Delay(10000);
            }
        });
    }

    private void HandleZoneUnload()
    {
        AnsiConsole.MarkupLine("[bold yellow]>> All scripts suspended for Zone Transition.[/]");
    }

    private void RegisterBuiltins()
    {
        UserData.RegisterType<LunarObject>();
        UserData.RegisterType<Part>();
        UserData.RegisterType<Sound>();
        UserData.RegisterType<GameScript>();

        _script.Globals["CompileBytecode"] = (Func<string, byte[]>)(code => {
            _security.Demand(PermissionLevel.System);
            return ExportBytecode(code);
        });

        _script.Globals["SetBytecode"] = (Action<string, byte[]>)((path, data) => {
            _security.Demand(PermissionLevel.System);
            SaveBytecode(path, data);
        });

        _script.Globals["new"] = (Func<string, LunarObject?, string?, MoonSharp.Interpreter.Table?, MoonSharp.Interpreter.Table?, LunarObject>)((className, parent, source, attributes, properties) => {
            LunarObject obj = className.ToLower() switch
            {
                "part" => new Part(),
                "sound" => new Sound(),
                "localscript" or "modulescript" or "gamescript" => new GameScript(className),
                _ => new LunarObject(className)
            };

            obj.SetParent(parent);
            obj.Source = source;

            if (attributes != null)
                foreach (var kv in attributes.Pairs)
                    obj.Attributes[kv.Key.String] = kv.Value.ToObject();

            if (properties != null)
                foreach (var kv in properties.Pairs)
                    obj.Properties[kv.Key.String] = kv.Value.ToObject();

            if (obj is GameScript gs && !string.IsNullOrEmpty(source))
            {
                gs.Run(this);
            }

            return obj;
        });

        _script.Globals["SetPermissionLevel"] = (Action<int>)(lvl => _security.CurrentLevel = (PermissionLevel)lvl);

        _script.Globals["require"] = (Func<string, DynValue>)(moduleName => {
            _security.Demand(PermissionLevel.System);
            string modPath = Path.Combine("Scripts", moduleName + ".bymodenc");
            if (!File.Exists(modPath)) throw new ScriptRuntimeException($"Module '{moduleName}' not found.");
            
            string encrypted = File.ReadAllText(modPath);
            byte[] bytecode = Convert.FromBase64String(CryptoUtils.Decrypt(encrypted));

            if (_bytecodeSizeCheck.TryGetValue(modPath, out int expectedSize) && bytecode.Length != expectedSize)
            {
                throw new ScriptRuntimeException("Security Critical: Module bytecode size mismatch! Tampering detected.");
            }

            string spoofedName = "mem_" + Guid.NewGuid().ToString("N");
            var func = _script.LoadStream(new MemoryStream(bytecode), null, spoofedName);
            return func.Function.Call();
        });
    }

    private void LoadBuiltinScripts()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resources = assembly.GetManifestResourceNames();

        foreach (var resource in resources.Where(r => r.EndsWith(".lua")))
        {
            using Stream? stream = assembly.GetManifestResourceStream(resource);
            if (stream == null) continue;
            using StreamReader reader = new StreamReader(stream);
            string code = reader.ReadToEnd();
            try { _script.DoString(code); } catch {}
            AnsiConsole.MarkupLine($"[grey]  Embedded script loaded:[/] [blue]{resource}[/]");
        }
    }

    private void TryLoadCache()
    {
        if (File.Exists(CACHE_FILE))
        {
            try
            {
                var json = File.ReadAllText(CACHE_FILE);
                var cached = JsonSerializer.Deserialize<List<string>>(json);
                if (cached != null)
                {
                    AnsiConsole.MarkupLine("[bold red]⚠ PERFORMANCE WARNING: Reloading cached StoryScripts...[/]");
                    foreach (var name in cached)
                    {
                        _script.DoString($"-- Cached StoryScript: {name}");
                        _storyScripts[name] = new StoryScript { Name = name, IsRestored = true };
                    }
                }
            }
            catch (Exception ex) { AnsiConsole.MarkupLine($"[red]Error loading cache: {ex.Message}[/]"); }
        }
    }

    public void SaveCache()
    {
        var names = _storyScripts.Keys.ToList();
        var json = JsonSerializer.Serialize(names);
        File.WriteAllText(CACHE_FILE, json);
    }

    public void DoString(string code, string name = "unnamed")
    {
        bool storyAfterUnload = code.Contains("#-Behavior=StoryAfterUnload");

        if (storyAfterUnload)
        {
            var ss = new StoryScript { Name = name, OriginalSource = code, IsPersistent = true };
            _storyScripts[name] = ss;
            AnsiConsole.MarkupLine($"[blue]ℹ Script '{name}' registered as StoryAfterUnload.[/]");
        }

        _script.DoString(code);
    }

    public void RestoreStoryScript(string name)
    {
        if (_storyScripts.TryGetValue(name, out var ss))
        {
            ss.IsRestored = true;
            _script.DoString(ss.OriginalSource);
            AnsiConsole.MarkupLine($"[green]✓ StoryScript '{name}' restored.[/]");
        }
    }

    public void UnloadStoryScript(string name)
    {
        if (_storyScripts.Remove(name))
        {
            AnsiConsole.MarkupLine($"[yellow]⬡ StoryScript '{name}' unloaded.[/]");
        }
    }

    public byte[] ExportBytecode(string luaCode)
    {
        var tempScript = new Script();
        var function = tempScript.LoadString(luaCode);
        using var ms = new MemoryStream();
        tempScript.Dump(function, ms);
        return ms.ToArray();
    }

    public void SaveBytecode(string path, byte[] bytecode)
    {
        var hash = BitConverter.ToString(SHA256.HashData(bytecode)).Replace("-", "").ToLower();
        using var fs = new FileStream(path, FileMode.Create);
        using var writer = new BinaryWriter(fs);
        writer.Write("LUNAR");
        writer.Write(1);
        writer.Write(hash);
        writer.Write(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        writer.Write(bytecode.Length);
        writer.Write(bytecode);
    }

    public void LoadAndExecute(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException(path);
        using var fs = new FileStream(path, FileMode.Open);
        using var reader = new BinaryReader(fs);
        string magic = reader.ReadString();
        if (magic != "LUNAR") throw new Exception("Invalid magic.");
        int version = reader.ReadInt32();
        string hash = reader.ReadString();
        long ts = reader.ReadInt64();
        int len = reader.ReadInt32();
        byte[] bytecode = reader.ReadBytes(len);
        _script.DoStream(new MemoryStream(bytecode));
    }

    public DynValue CreateThread(DynValue function)
    {
        if (!_isZoneLoaded) AnsiConsole.MarkupLine("[yellow]⚠ Spawn delayed.[/]");
        var coroutine = _script.CreateCoroutine(function);
        _activeThreads.Add(coroutine);
        return coroutine;
    }

    public void Tick()
    {
        if (!_isZoneLoaded) return;
        foreach (var thread in _activeThreads.ToList())
        {
            if (thread.Coroutine.State == CoroutineState.Suspended || thread.Coroutine.State == CoroutineState.Running)
            {
                try { thread.Coroutine.Resume(); }
                catch (Exception ex) { AnsiConsole.WriteException(ex); }
            }
        }
        CleanupThreads();
    }

    public void MonitorThreads()
    {
        var table = new Spectre.Console.Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[blue]ID[/]")
            .AddColumn("[blue]Status[/]")
            .AddColumn("[blue]Type[/]")
            .AddColumn("[blue]Security[/]");

        for (int i = 0; i < _activeThreads.Count; i++)
        {
            var t = _activeThreads[i];
            table.AddRow(i.ToString(), t.Coroutine.State.ToString(), "Coroutine", _security.CurrentLevel.ToString());
        }
        AnsiConsole.Write(table);
    }

    public void CleanupThreads()
    {
        _activeThreads.RemoveAll(t => t.Coroutine.State == CoroutineState.Dead);
    }
}
