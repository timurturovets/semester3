using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using application.Models;
using application.Services;
using application.ViewModels;

namespace application;

public partial class MainWindow : Window
{
    private const double CellSize = 28;
    private const double MarginSize = 48;
    private readonly ObservableCollection<GeneratorEntry> _generatorEntries =
    [
        new(new MonomialGenerator(6, 0)),
        new(new MonomialGenerator(2, 3)),
        new(new MonomialGenerator(7, 0))
    ];

    public MainWindow()
    {
        InitializeComponent();
        GeneratorsGrid.ItemsSource = _generatorEntries;
        RefreshTextInputFromTable();
        BuildVisualizations();
    }

    private void ParseTextButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var generators = MonomialParser.ParseMany(MonomialInputTextBox.Text);
            ReplaceGenerators(generators);
            SetStatus($"Загружено образующих из текста: {generators.Count}.", isError: false);
            BuildVisualizations();
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message, isError: true);
        }
    }

    private void BuildButton_OnClick(object sender, RoutedEventArgs e)
    {
        BuildVisualizations();
    }

    private void AddGeneratorButton_OnClick(object sender, RoutedEventArgs e)
    {
        _generatorEntries.Add(new GeneratorEntry());
        SetStatus("Добавлена пустая строка образующей.", isError: false);
    }

    private void RemoveGeneratorButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (GeneratorsGrid.SelectedItem is not GeneratorEntry entry)
        {
            SetStatus("Выберите строку образующей для удаления.", isError: true);
            return;
        }

        _generatorEntries.Remove(entry);
        SetStatus("Выбранная образующая удалена.", isError: false);
        RefreshTextInputFromTable();
    }

    private void BuildVisualizations()
    {
        try
        {
            TrySyncTableFromTextInput();
            var ideal = BuildIdealFromTable();
            var (maxX, maxY) = ideal.SuggestBounds();
            BoundsTextBlock.Text = $"Автоматические границы: 0 <= m <= {maxX}, 0 <= n <= {maxY}. Добавлен запас относительно максимальных степеней образующих.";

            DrawPlane(
                IdealCanvas,
                ideal,
                ideal.EnumerateIdealPoints(maxX, maxY),
                maxX,
                maxY,
                fillBrush: new SolidColorBrush(Color.FromRgb(55, 106, 103)),
                markerBrush: new SolidColorBrush(Color.FromRgb(47, 93, 98)));

            DrawPlane(
                RemainderCanvas,
                ideal,
                ideal.EnumerateRemainderPoints(maxX, maxY),
                maxX,
                maxY,
                fillBrush: new SolidColorBrush(Color.FromRgb(188, 108, 37)),
                markerBrush: new SolidColorBrush(Color.FromRgb(156, 102, 68)));

            RefreshTextInputFromTable();
            SetStatus($"Визуализация обновлена. Число образующих: {ideal.Generators.Count}.", isError: false);
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message, isError: true);
        }
    }

    private void TrySyncTableFromTextInput()
    {
        var textInput = MonomialInputTextBox.Text;
        var canonicalTableText = MonomialParser.ToDisplayText(_generatorEntries.Select(entry => entry.ToGenerator()));

        if (NormalizeInput(textInput) == NormalizeInput(canonicalTableText))
        {
            return;
        }

        var parsedGenerators = MonomialParser.ParseMany(textInput);
        ReplaceGenerators(parsedGenerators);
    }

    private MonomialIdeal BuildIdealFromTable()
    {
        if (_generatorEntries.Count == 0)
        {
            throw new InvalidOperationException("Добавьте хотя бы одну мономиальную образующую.");
        }

        var generators = _generatorEntries.Select(entry =>
        {
            if (entry.XExponent < 0 || entry.YExponent < 0)
            {
                throw new InvalidOperationException("Степени должны быть неотрицательными целыми числами.");
            }

            return entry.ToGenerator();
        });

        return new MonomialIdeal(generators);
    }

    private void ReplaceGenerators(IEnumerable<MonomialGenerator> generators)
    {
        _generatorEntries.Clear();
        foreach (var generator in generators)
        {
            _generatorEntries.Add(new GeneratorEntry(generator));
        }

        RefreshTextInputFromTable();
    }

    private void RefreshTextInputFromTable()
    {
        MonomialInputTextBox.Text = MonomialParser.ToDisplayText(_generatorEntries.Select(entry => entry.ToGenerator()));
    }

    private void DrawPlane(
        Canvas canvas,
        MonomialIdeal ideal,
        IEnumerable<GridPoint> highlightedPoints,
        int maxX,
        int maxY,
        Brush fillBrush,
        Brush markerBrush)
    {
        canvas.Children.Clear();

        var width = MarginSize + (maxX + 1) * CellSize + 24;
        var height = MarginSize + (maxY + 1) * CellSize + 24;
        canvas.Width = width;
        canvas.Height = height;

        DrawGrid(canvas, maxX, maxY);

        foreach (var point in highlightedPoints)
        {
            var rectangle = new Rectangle
            {
                Width = CellSize - 2,
                Height = CellSize - 2,
                Fill = fillBrush,
                RadiusX = 4,
                RadiusY = 4,
                Opacity = 0.8
            };

            Canvas.SetLeft(rectangle, MarginSize + point.X * CellSize + 1);
            Canvas.SetTop(rectangle, MarginSize + (maxY - point.Y) * CellSize + 1);
            canvas.Children.Add(rectangle);
        }

        foreach (var generator in ideal.Generators)
        {
            DrawGeneratorMarker(canvas, generator, maxY, markerBrush);
        }
    }

    private void DrawGrid(Canvas canvas, int maxX, int maxY)
    {
        var axisBrush = new SolidColorBrush(Color.FromRgb(35, 48, 59));
        var gridBrush = new SolidColorBrush(Color.FromRgb(220, 213, 201));

        for (var x = 0; x <= maxX + 1; x++)
        {
            var line = new Line
            {
                X1 = MarginSize + x * CellSize,
                Y1 = MarginSize,
                X2 = MarginSize + x * CellSize,
                Y2 = MarginSize + (maxY + 1) * CellSize,
                Stroke = x == 0 ? axisBrush : gridBrush,
                StrokeThickness = x == 0 ? 2 : 1
            };
            canvas.Children.Add(line);
        }

        for (var y = 0; y <= maxY + 1; y++)
        {
            var line = new Line
            {
                X1 = MarginSize,
                Y1 = MarginSize + y * CellSize,
                X2 = MarginSize + (maxX + 1) * CellSize,
                Y2 = MarginSize + y * CellSize,
                Stroke = y == maxY + 1 ? axisBrush : gridBrush,
                StrokeThickness = y == maxY + 1 ? 2 : 1
            };
            canvas.Children.Add(line);
        }

        for (var x = 0; x <= maxX; x++)
        {
            canvas.Children.Add(CreateLabel(
                x.ToString(CultureInfo.InvariantCulture),
                MarginSize + x * CellSize + 8,
                MarginSize + (maxY + 1) * CellSize + 6));
        }

        for (var y = 0; y <= maxY; y++)
        {
            canvas.Children.Add(CreateLabel(
                y.ToString(CultureInfo.InvariantCulture),
                12,
                MarginSize + (maxY - y) * CellSize + 5));
        }

        canvas.Children.Add(CreateAxisName("m", MarginSize + (maxX + 1) * CellSize + 6, MarginSize + (maxY + 1) * CellSize + 2));
        canvas.Children.Add(CreateAxisName("n", 12, 10));
    }

    private void DrawGeneratorMarker(Canvas canvas, MonomialGenerator generator, int maxY, Brush markerBrush)
    {
        var ellipse = new Ellipse
        {
            Width = 14,
            Height = 14,
            Fill = Brushes.White,
            Stroke = markerBrush,
            StrokeThickness = 3
        };

        Canvas.SetLeft(ellipse, MarginSize + generator.XExponent * CellSize + 7);
        Canvas.SetTop(ellipse, MarginSize + (maxY - generator.YExponent) * CellSize + 7);
        canvas.Children.Add(ellipse);

        var labelBackground = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(230, 255, 252, 245)),
            BorderBrush = markerBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(4, 1, 4, 1)
        };

        var label = new TextBlock
        {
            Text = generator.ToString(),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(28, 33, 40))
        };

        labelBackground.Child = label;
        Canvas.SetLeft(labelBackground, MarginSize + generator.XExponent * CellSize + 18);
        Canvas.SetTop(labelBackground, MarginSize + (maxY - generator.YExponent) * CellSize - 4);
        canvas.Children.Add(labelBackground);
    }

    private static TextBlock CreateLabel(string text, double left, double top)
    {
        var label = new TextBlock
        {
            Text = text,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(76, 90, 100))
        };

        Canvas.SetLeft(label, left);
        Canvas.SetTop(label, top);
        return label;
    }

    private static TextBlock CreateAxisName(string text, double left, double top)
    {
        var label = new TextBlock
        {
            Text = text,
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(35, 48, 59))
        };

        Canvas.SetLeft(label, left);
        Canvas.SetTop(label, top);
        return label;
    }

    private void SetStatus(string message, bool isError)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = isError
            ? new SolidColorBrush(Color.FromRgb(123, 46, 46))
            : new SolidColorBrush(Color.FromRgb(45, 86, 62));
    }

    private static string NormalizeInput(string input)
    {
        return new string(input
            .Where(character => !char.IsWhiteSpace(character))
            .ToArray());
    }
}
