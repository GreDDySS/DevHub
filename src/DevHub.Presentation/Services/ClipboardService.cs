using DevHub.Domain.Interfaces;

namespace DevHub.Presentation.Services;

public class ClipboardService : IClipboardService
{
    public Task<string?> GetTextAsync(CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<string?>();
        var thread = new Thread(() =>
        {
            try
            {
                string? text = null;
                if (System.Windows.Clipboard.ContainsText())
                    text = System.Windows.Clipboard.GetText();
                tcs.SetResult(text);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);
    }

    public Task SetTextAsync(string text, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource();
        var thread = new Thread(() =>
        {
            try
            {
                System.Windows.Clipboard.SetText(text);
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return tcs.Task.WaitAsync(TimeSpan.FromSeconds(5), ct);
    }
}
