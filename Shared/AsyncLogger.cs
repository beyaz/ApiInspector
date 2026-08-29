global using static ApiInspector.AsyncLogger;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace ApiInspector;

static class AsyncLogger
{
    static readonly HttpClient _httpClient = new();

    static readonly ConcurrentQueue<string> _queue = new();

    static readonly SemaphoreSlim _signal = new(0);

    static bool _started;

    internal static void Start(string apiUrl)
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

                        var content = new StringContent(message, Encoding.UTF8, "text/plain");

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

    internal static void WaitAsyncLogsForFinish()
    {
        while (!_queue.IsEmpty)
        {
            Thread.Sleep(50);
        }
    }

    internal static void WriteLog(string message)
    {
        _queue.Enqueue(message);

        _signal.Release();
    }

    internal static void WriteLog(IEnumerable<string> messages)
    {
        if (messages is null)
        {
            return;
        }
        
        foreach (var message in messages)
        {
            _queue.Enqueue(message);
        }
        
        _signal.Release();
    }
}