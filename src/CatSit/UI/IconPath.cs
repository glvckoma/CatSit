using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CatSit.UI;

public sealed class IconPath : Viewbox
{
    public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register(
        nameof(Geometry), typeof(Geometry), typeof(IconPath),
        new PropertyMetadata(null, (d, e) => ((IconPath)d)._path.Data = (Geometry?)e.NewValue));

    public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
        nameof(Foreground), typeof(Brush), typeof(IconPath),
        new PropertyMetadata(Brushes.White, (d, e) => ((IconPath)d)._path.Stroke = (Brush?)e.NewValue));

    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
        nameof(Size), typeof(double), typeof(IconPath),
        new PropertyMetadata(18.0, (d, e) => ((IconPath)d).ApplySize((double)e.NewValue)));

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
        nameof(StrokeThickness), typeof(double), typeof(IconPath),
        new PropertyMetadata(1.8, (d, e) => ((IconPath)d)._path.StrokeThickness = (double)e.NewValue));

    private readonly Path _path = new()
    {
        StrokeThickness = 1.8,
        StrokeStartLineCap = PenLineCap.Round,
        StrokeEndLineCap = PenLineCap.Round,
        StrokeLineJoin = PenLineJoin.Round
    };

    public IconPath()
    {
        var canvas = new Canvas { Width = 24, Height = 24 };
        canvas.Children.Add(_path);
        Child = canvas;
        Stretch = Stretch.Uniform;
        ApplySize(Size);
    }

    public Geometry? Geometry
    {
        get => (Geometry?)GetValue(GeometryProperty);
        set => SetValue(GeometryProperty, value);
    }

    public Brush? Foreground
    {
        get => (Brush?)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    private void ApplySize(double size)
    {
        Width = size;
        Height = size;
    }
}
