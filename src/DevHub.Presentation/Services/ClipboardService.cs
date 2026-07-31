using System.Collections.Concurrent;
using DevHub.Domain.Interfaces;

namespace DevHub.Presentation.Services;

public class ClipboardService : IClipboardService, IDisposable
{
    private readonly Thread _staThread;
    private readonly BlockingCollection<Action> _queue = new();

    public ClipboardService()
    {
        _staThread = new Thread(RunStaLoop)
        {
            IsBackground = true,
            Name = "DevHub-Clipboard-STA"
        };
        _staThread.SetApartmentState(ApartmentState.STA);
        _staThread.Start();
    }

    private void RunStaLoop()
    {
        try
        {
            foreach (var work in _queue.GetConsumingEnumerable())
            {
                work();
            }
        }
        catch (InvalidOperationException) { }
    }

    public Task<string?> GetTextAsync(CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (_queue.IsAddingCompleted)
        {
            tcs.TrySetCanceled();
            return tcs.Task;
        }

        _queue.Add(() =>
        {
            try
            {
                string? text = null;
                if (System.Windows.Clipboard.ContainsText())
                    text = System.Windows.Clipboard.GetText();
                tcs.TrySetResult(text);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }, ct);

        return tcs.Task;
    }

    public Task SetTextAsync(string text, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        if (_queue.IsAddingCompleted)
        {
            tcs.TrySetCanceled();
            return tcs.Task;
        }

        _queue.Add(() =>
        {
            try
            {
                System.Windows.Clipboard.SetText(text);
                tcs.TrySetResult();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }, ct);

        return tcs.Task;
    }

    public void Dispose()
    {
        _queue.CompleteAdding();
        _staThread.Join(TimeSpan.FromSeconds(2));
    }
}
