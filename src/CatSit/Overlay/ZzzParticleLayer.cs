using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace CatSit.Overlay;

public sealed class ZzzParticleLayer
{
    private readonly Canvas _canvas;
    private readonly BitmapSource _zzzBitmap;

    public ZzzParticleLayer(Canvas canvas, BitmapSource zzzBitmap)
    {
        _canvas = canvas;
        _zzzBitmap = zzzBitmap;
    }

    public double ParticleSize { get; set; } = 32;

    public void Spawn(double x, double y)
    {
        var image = new Image
        {
            Source = _zzzBitmap,
            Width = ParticleSize,
            Height = ParticleSize,
            IsHitTestVisible = false
        };
        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
        Canvas.SetLeft(image, x);
        Canvas.SetTop(image, y);
        _canvas.Children.Add(image);
        var duration = TimeSpan.FromSeconds(2);
        var drift = new DoubleAnimation(x, x + 24, duration);
        var rise = new DoubleAnimation(y, y - 40, duration);
        var fade = new DoubleAnimation(1, 0, duration);
        fade.Completed += (_, _) => _canvas.Children.Remove(image);
        image.BeginAnimation(Canvas.LeftProperty, drift);
        image.BeginAnimation(Canvas.TopProperty, rise);
        image.BeginAnimation(UIElement.OpacityProperty, fade);
    }

    public void Clear() => _canvas.Children.Clear();
}
