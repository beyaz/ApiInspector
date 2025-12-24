using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApiInspector;

public static class AsyncLogger
{
    private static readonly ConcurrentQueue<string> _queue = new();
    private static readonly SemaphoreSlim _signal = new(0);
    private static readonly HttpClient _httpClient = new();
    private static bool _started = false;

    public static void Start(string apiUrl)
    {
        if (_started) return;
        _started = true;

        Task.Run(async () =>
        {
            while (true)
            {
                await _signal.WaitAsync();

                if (_queue.TryDequeue(out var message))
                {
                    try
                    {
                        var content = new StringContent(
                            JsonSerializer.Serialize(message),
                            Encoding.UTF8,
                            "application/json");

                        await _httpClient.PostAsync(apiUrl, content);
                    }
                    catch
                    {
                        // burada istersen retry / file fallback yaparsın
                    }
                }
            }
        });
    }

    public static void Log(string message)
    {
        Console.WriteLine(message); // normal console output
        _queue.Enqueue(message);
        _signal.Release();
    }
}