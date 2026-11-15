using System.Reflection;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace WinterWindow;

public static class Program
{
    public static void Log(string m) { try { Directory.CreateDirectory(DataDir); File.AppendAllText(Path.Combine(DataDir, "log.txt"), DateTime.Now.ToString("HH:mm:ss.fff") + " " + m + Environment.NewLine); } catch { } }

    static readonly string DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AsciiWinterWindow");

    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.ThreadException += (_, e) => Log("thread exception: " + e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log("unhandled: " + e.ExceptionObject);
        Log("start args=" + string.Join(' ', args));
        var mode = ParseMode(args);
        switch (mode)
        {
            case 's':
                RunSaver();
                break;
            case 'p':
                break;
            default:
                MessageBox.Show("Зимове вікно: налаштувань немає.\n\nЗапуск: правою кнопкою на файлі .scr, пункт Test.", "Зимове вікно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
        }
    }

    static char ParseMode(string[] args)
    {
        if (args.Length == 0) return 'c';
        var a = args[0].Trim().ToLowerInvariant().TrimStart('/', '-');
        return a.Length == 0 ? 'c' : a[0];
    }

    static void RunSaver()
    {
        var www = Path.Combine(DataDir, "www");
        Directory.CreateDirectory(www);
        using (var res = Assembly.GetExecutingAssembly().GetManifestResourceStream("index.html")!)
        using (var file = File.Create(Path.Combine(www, "index.html")))
        {
            res.CopyTo(file);
        }
        foreach (var screen in Screen.AllScreens)
        {
            new SaverForm(screen, www, Path.Combine(DataDir, "profile")).Show();
        }
        Application.Run();
    }
}

sealed class SaverForm : Form
{
    readonly WebView2 view = new();

    public SaverForm(Screen screen, string www, string profile)
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = screen.Bounds;
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = Color.Black;
        view.Dock = DockStyle.Fill;
        view.DefaultBackgroundColor = Color.Black;
        Controls.Add(view);
        Shown += async (_, _) =>
        {
            Cursor.Hide();
            var env = await CoreWebView2Environment.CreateAsync(null, profile);
            await view.EnsureCoreWebView2Async(env);
            var s = view.CoreWebView2.Settings;
            s.AreDevToolsEnabled = false;
            s.AreDefaultContextMenusEnabled = false;
            s.IsStatusBarEnabled = false;
            s.AreBrowserAcceleratorKeysEnabled = false;
            s.IsZoomControlEnabled = false;
            view.CoreWebView2.SetVirtualHostNameToFolderMapping("winter.local", www, CoreWebView2HostResourceAccessKind.Allow);
            view.CoreWebView2.WebMessageReceived += (_, e) => { Program.Log("exit by page: " + e.TryGetWebMessageAsString()); Application.Exit(); };
            Program.Log("webview ready");
            view.CoreWebView2.Navigate("https://winter.local/index.html?saver");
        };
        FormClosed += (_, _) => { Program.Log("form closed"); Application.Exit(); };
        KeyDown += (_, _) => { Program.Log("form key"); Application.Exit(); };
        MouseDown += (_, _) => { Program.Log("form mouse"); Application.Exit(); };
    }
}
