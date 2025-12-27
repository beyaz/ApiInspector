global using static ApiInspector.AsyncLogger;

using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace ApiInspector;

public static class AsyncLogger
{
    public static void WriteLog(string message)
    {
        Log(message);
    }
   
    static readonly HttpClient _httpClient = new();

    static readonly ConcurrentQueue<string> _queue = new();

    static readonly SemaphoreSlim _signal = new(0);

    static bool _started;

    public static void Log(string message)
    {
        _queue.Enqueue(message);
        _signal.Release();
    }

    public static void Start(string apiUrl)
    {
        if (_started)
        {
            return;
        }

        _started = true;

        Task.Run(InfiniteLoop);

        return;

        async Task InfiniteLoop()
        {
            while (true)
            {
                await _signal.WaitAsync();

                if (_queue.TryDequeue(out var message))
                {
                    try
                    {
                        var json = JsonConvert.SerializeObject(new[] { message });

                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        await _httpClient.PostAsync(apiUrl, content);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}