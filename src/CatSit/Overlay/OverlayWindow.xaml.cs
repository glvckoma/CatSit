using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CatSit.Core.Animation;
using CatSit.Core.Settings;
using CatSit.Core.Sprites;
using CatSit.Interop;


namespace CatSit.Overlay;

public partial class OverlayWindow : Window
{
    private const int SpriteWidth = CatSprites.FrameWidth;
    private const int SpriteHeight = CatSprites.FrameHeight;

    private readonly SpriteBitmapCache _cache;
    private readonly AppSettings _settings;
    private readonly ZzzParticleLayer _zzzLayer;
    private readonly CatAnimator _animator = new();
    private readonly DispatcherTimer _timer;

    public OverlayWindow(SpriteBitmapCache cache, AppSettings settings)
    {
        InitializeComponent();
        _cache = cache;
        _settings = settings;
        _zzzLayer = new ZzzParticleLayer(ZzzCanvas, cache.ZzzBitmap);
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _timer.Tick += OnTick;
        var area = SystemParameters.WorkArea;
        Left = area.Left;
        Top = area.Top;
        Width = area.Width;
        Height = area.Height;
        SourceInitialized += OnSourceInitialized;
    }

    public event Action? CatClicked;

    public void ShowCat(string hotkeyText)
    {
        ApplyAppearance();
        var timings = new AnimationTimings { LayingDurationMs = _settings.SecondsUntilSleep * 1000 };
        var enterFromLeft = _settings.EntryEdge switch
        {
            EntryEdge.Left => true,
            EntryEdge.Right => false,
            _ => Random.Shared.Next(2) == 0
        };
        var restX = Width * (0.35 + 0.30 * Random.Shared.NextDouble()) - CatImage.Width / 2;
        _animator.Start(Width, CatImage.Width, restX, enterFromLeft, timings, Environment.TickCount64);
        Canvas.SetTop(CatImage, Height - CatImage.Height);
        Canvas.SetLeft(CatImage, _animator.X);
        CatImage.Source = _cache.Get(_animator.State, _animator.FrameIndex, _animator.FacingLeft);
        if (_settings.ShowUnlockHint) ShowHint(hotkeyText);
        else HintBorder.Opacity = 0;
        _timer.Start();
        Show();
    }

    public void WakeAndDismiss()
    {
        if (_animator.State is CatState.Hidden) return;
        _animator.Wake(Environment.TickCount64);
    }

    public void RefreshAppearance()
    {
        ApplyAppearance();
        if (_animator.State is not CatState.Hidden)
        {
            CatImage.Source = _cache.Get(_animator.State, _animator.FrameIndex, _animator.FacingLeft);
        }
    }

    public bool ContainsCatScreenPoint(int x, int y)
    {
        if (!IsVisible || _animator.State is CatState.Hidden) return false;
        var point = PointFromScreen(new Point(x, y));
        var left = Canvas.GetLeft(CatImage);
        var top = Canvas.GetTop(CatImage);
        return point.X >= left && point.X <= left + CatImage.ActualWidth
            && point.Y >= top && point.Y <= top + CatImage.ActualHeight;
    }

    private void ApplyAppearance()
    {
        CatImage.Width = SpriteWidth * _settings.CatScale;
        CatImage.Height = SpriteHeight * _settings.CatScale;
        CatImage.IsHitTestVisible = _settings.ClickToDismiss;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var now = Environment.TickCount64;
        _animator.Advance(now);
        if (_animator.ExitedScreen)
        {
            _timer.Stop();
            _zzzLayer.Clear();
            Hide();
            return;
        }
        Canvas.SetLeft(CatImage, _animator.X);
        Canvas.SetTop(CatImage, Height - CatImage.Height);
        CatImage.Source = _cache.Get(_animator.State, _animator.FrameIndex, _animator.FacingLeft);
        if (_settings.ShowZzz && _animator.TryConsumeZzz(now))
        {
            var headX = _animator.X + (_animator.FacingLeft ? 8 : CatImage.Width - CatImage.Width / 3);
            _zzzLayer.Spawn(headX, Height - CatImage.Height - 16);
        }
    }

    private void ShowHint(string hotkeyText)
    {
        HintText.Text = $"{hotkeyText} to unlock";
        HintBorder.Opacity = 1;
        var fade = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(1)) { BeginTime = TimeSpan.FromSeconds(3) };
        HintBorder.BeginAnimation(OpacityProperty, fade);
    }

    private void OnCatClicked(object sender, MouseButtonEventArgs e) => CatClicked?.Invoke();

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var style = NativeMethods.GetWindowLongW(hwnd, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLongW(hwnd, NativeMethods.GWL_EXSTYLE,
            style | NativeMethods.WS_EX_TOOLWINDOW | NativeMethods.WS_EX_NOACTIVATE);
    }
}
