using Lunar.Core;

namespace Lunar;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Lunar Engine Host Application Entry Point
        // Starts the engine in background mode with TCP communication enabled.
        await LunarAPI.Start(5555);
    }
}
