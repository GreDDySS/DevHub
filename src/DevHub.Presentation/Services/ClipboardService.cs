using DevHub.Domain.Interfaces;

namespace DevHub.Presentation.Services;

public class ClipboardService : IClipboardService
{
    public async Task<string?> GetTextAsync()
    {
        return await Task.Run(() =>
        {
            string? text = null;
            var thread = new Thread(() =>
            {
                if (System.Windows.Clipboard.ContainsText())
                    text = System.Windows.Clipboard.GetText();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return text;
        });
    }

    public async Task SetTextAsync(string text)
    {
        await Task.Run(() =>
        {
            var thread = new Thread(() => System.Windows.Clipboard.SetText(text));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        });
    }
}
