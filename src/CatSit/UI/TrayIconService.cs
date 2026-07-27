using WinForms = System.Windows.Forms;

namespace CatSit.UI;

public sealed class TrayIconService : IDisposable
{
    private readonly WinForms.NotifyIcon _icon;
    private readonly WinForms.ToolStripMenuItem _toggleItem;

    public TrayIconService()
    {
        _toggleItem = new WinForms.ToolStripMenuItem("Lock");
        var openItem = new WinForms.ToolStripMenuItem("Open");
        var exitItem = new WinForms.ToolStripMenuItem("Exit");
        _toggleItem.Click += (_, _) => ToggleRequested?.Invoke();
        openItem.Click += (_, _) => OpenRequested?.Invoke();
        exitItem.Click += (_, _) => ExitRequested?.Invoke();
        var menu = new WinForms.ContextMenuStrip();
        menu.Items.AddRange([_toggleItem, openItem, new WinForms.ToolStripSeparator(), exitItem]);
        _icon = new WinForms.NotifyIcon
        {
            Icon = TrayIconFactory.Create(),
            Text = "CatSit",
            Visible = true,
            ContextMenuStrip = menu
        };
        _icon.DoubleClick += (_, _) => OpenRequested?.Invoke();
    }

    public event Action? ToggleRequested;
    public event Action? OpenRequested;
    public event Action? ExitRequested;

    public void SetLocked(bool locked)
    {
        _toggleItem.Text = locked ? "Unlock" : "Lock";
        _icon.Text = locked ? "CatSit (locked)" : "CatSit";
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
