namespace DevHub.Presentation.Services;

public class TrayService : IDisposable
{
    private System.Windows.Forms.NotifyIcon? _notifyIcon;

    public event Action? TrayDoubleClick;
    public event Action? ShowWindowRequested;
    public event Action? ExitRequested;

    public void Initialize()
    {
        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Text = "DevHub",
            Icon = CreateDefaultIcon(),
            Visible = false
        };

        _notifyIcon.DoubleClick += (_, _) => TrayDoubleClick?.Invoke();

        var contextMenu = new System.Windows.Forms.ContextMenuStrip();

        var showItem = new System.Windows.Forms.ToolStripMenuItem("Show DevHub");
        showItem.Click += (_, _) => ShowWindowRequested?.Invoke();

        var exitItem = new System.Windows.Forms.ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => ExitRequested?.Invoke();

        contextMenu.Items.Add(showItem);
        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    public void Show()
    {
        if (_notifyIcon is not null)
            _notifyIcon.Visible = true;
    }

    public void Hide()
    {
        if (_notifyIcon is not null)
            _notifyIcon.Visible = false;
    }

    public void ShowBalloon(string title, string message)
    {
        if (_notifyIcon is not null)
        {
            _notifyIcon.ShowBalloonTip(3000, title, message, System.Windows.Forms.ToolTipIcon.Info);
        }
    }

    public void Dispose()
    {
        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
    }

    private static System.Drawing.Icon CreateDefaultIcon()
    {
        try
        {
            var exePath = Environment.ProcessPath;
            if (exePath is not null && System.IO.File.Exists(exePath))
            {
                return System.Drawing.Icon.ExtractAssociatedIcon(exePath)
                    ?? CreateGeneratedIcon();
            }
        }
        catch { }

        return CreateGeneratedIcon();
    }

    private static System.Drawing.Icon CreateGeneratedIcon()
    {
        using var bitmap = new System.Drawing.Bitmap(16, 16);
        using var graphics = System.Drawing.Graphics.FromImage(bitmap);
        graphics.Clear(System.Drawing.ColorTranslator.FromHtml("#4CAF50"));
        graphics.DrawString("D", new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
            System.Drawing.Brushes.White, 0, 0);

        var hIcon = bitmap.GetHicon();
        return System.Drawing.Icon.FromHandle(hIcon);
    }
}
