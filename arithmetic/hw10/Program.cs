using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Windows.Forms;
using System.ComponentModel;

namespace hw10;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private readonly NumericUpDown _rInput = new() { Minimum = 1, Maximum = 1000, Value = 2, Width = 90 };
    private readonly NumericUpDown _maxDigitsInput = new() { Minimum = 10, Maximum = 200000, Value = 200, Increment = 10, Width = 90 };
    private readonly NumericUpDown _stepInput = new() { Minimum = 5, Maximum = 200, Value = 20, Increment = 5, Width = 90 };
    private readonly Button _computeButton = new() { Text = "Вычислить", AutoSize = true };
    private readonly TextBox _eResultText = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly TextBox _piResultText = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private readonly PlotPanel _opsPlot = new() { Dock = DockStyle.Fill, Metric = PlotMetric.Operations };
    private readonly PlotPanel _termsPlot = new() { Dock = DockStyle.Fill, Metric = PlotMetric.Terms };
    private readonly Label _statusLabel = new() { AutoSize = true, Text = "Готово" };

    public MainForm()
    {
        Text = "e^r и \u03C0";
        Width = 1400;
        Height = 900;
        MinimumSize = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 28));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 30));

        root.Controls.Add(BuildControlPanel(), 0, 0);
        root.Controls.Add(BuildResultPanel(), 0, 1);
        root.Controls.Add(BuildPlotsPanel(), 0, 2);
        root.Controls.Add(_grid, 0, 3);

        Controls.Add(root);

        ConfigureGrid();
        _computeButton.Click += (_, _) => ComputeAndRender();
        Shown += (_, _) => ComputeAndRender();
    }

    private Control BuildControlPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(12),
            WrapContents = true
        };

        panel.Controls.Add(new Label { Text = "r:", AutoSize = true, Margin = new Padding(0, 8, 6, 0) });
        panel.Controls.Add(_rInput);
        panel.Controls.Add(new Label { Text = "Цифр:", AutoSize = true, Margin = new Padding(18, 8, 6, 0) });
        panel.Controls.Add(_maxDigitsInput);
        panel.Controls.Add(new Label { Text = "Шаг:", AutoSize = true, Margin = new Padding(18, 8, 6, 0) });
        panel.Controls.Add(_stepInput);
        panel.Controls.Add(_computeButton);
        panel.Controls.Add(_statusLabel);

        return panel;
    }

    private Control BuildResultPanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(12, 0, 12, 0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        layout.Controls.Add(BuildGroup("e^r", _eResultText), 0, 0);
        layout.Controls.Add(BuildGroup("\u03C0", _piResultText), 1, 0);

        return layout;
    }

    private Control BuildPlotsPanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(12, 8, 12, 8)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        layout.Controls.Add(BuildGroup("Арифметических операций", _opsPlot), 0, 0);
        layout.Controls.Add(BuildGroup("Термы", _termsPlot), 1, 0);

        return layout;
    }

    private GroupBox BuildGroup(string title, Control content)
    {
        var box = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = title,
            Padding = new Padding(10)
        };
        content.Dock = DockStyle.Fill;
        box.Controls.Add(content);
        return box;
    }

    private void ConfigureGrid()
    {
        _grid.Columns.Add("Цифры", "Цифры");
        _grid.Columns.Add("Значение экспоненты", "e^r значение");
        _grid.Columns.Add("Термы", "e^r термы");
        _grid.Columns.Add("Операции", "e^r операции");
        _grid.Columns.Add("мс", "e^r мс");
        _grid.Columns.Add("Значение \u03C0", "\u03C0 значение");
        _grid.Columns.Add("Термы \u03C0", "\u03C0 термы");
        _grid.Columns.Add("\u03C0 операций", "\u03C0 операций");
        _grid.Columns.Add("\u03C0 мс", "\u03C0 мс");
    }

    private void ComputeAndRender()
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            _computeButton.Enabled = false;
            _statusLabel.Text = "Вычисление...";
            Application.DoEvents();

            var r = (int)_rInput.Value;
            var maxDigits = (int)_maxDigitsInput.Value;
            var step = (int)_stepInput.Value;

            var precisionPoints = BuildPrecisionPoints(maxDigits, step);
            var samples = new List<PrecisionSample>(precisionPoints.Count);

            foreach (var digits in precisionPoints)
            {
                var eResult = ArbitraryPrecisionMath.ComputeExpOfNatural(r, digits);
                var piResult = ArbitraryPrecisionMath.ComputePi(digits);
                samples.Add(new PrecisionSample(digits, eResult, piResult));
            }

            BindResults(samples, r, maxDigits);
            _statusLabel.Text = $"Готово: {samples.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _statusLabel.Text = "Ошибка";
        }
        finally
        {
            _computeButton.Enabled = true;
            Cursor = Cursors.Default;
        }
    }

    private static List<int> BuildPrecisionPoints(int maxDigits, int step)
    {
        var points = new List<int>();
        for (var digits = step; digits <= maxDigits; digits += step)
        {
            points.Add(digits);
        }

        if (points.Count == 0 || points[^1] != maxDigits)
        {
            points.Add(maxDigits);
        }

        return points;
    }

    private void BindResults(List<PrecisionSample> samples, int r, int maxDigits)
    {
        var maxSample = samples.MaxBy(sample => sample.Digits) ?? throw new InvalidOperationException("Нет данных для отображения.");

        _eResultText.Text = $"e^{r} с {maxDigits} цифрами после запятой:{Environment.NewLine}{maxSample.Exp.ValueText}";
        _piResultText.Text = $"\u03C0 с {maxDigits} цифрами после запятой:{Environment.NewLine}{maxSample.Pi.ValueText}";

        _grid.Rows.Clear();
        foreach (var sample in samples)
        {
            _grid.Rows.Add(
                sample.Digits,
                Shorten(sample.Exp.ValueText),
                sample.Exp.Terms,
                sample.Exp.OperationCount,
                sample.Exp.ElapsedMilliseconds,
                Shorten(sample.Pi.ValueText),
                sample.Pi.Terms,
                sample.Pi.OperationCount,
                sample.Pi.ElapsedMilliseconds);
        }

        _opsPlot.SetSamples(samples);
        _termsPlot.SetSamples(samples);
    }

    private static string Shorten(string value)
    {
        const int limit = 48;
        return value.Length <= limit ? value : value[..limit] + "...";
    }
}

internal sealed class PlotPanel : Panel
{
    private List<PrecisionSample> _samples = [];

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PlotMetric Metric { get; init; }

    public PlotPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
    }

    public void SetSamples(List<PrecisionSample> samples)
    {
        _samples = samples;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);

        if (_samples.Count == 0)
        {
            using var emptyBrush = new SolidBrush(Color.FromArgb(80, 80, 80));
            g.DrawString("Нет данных", Font, emptyBrush, new PointF(12, 12));
            return;
        }

        const int left = 60;
        const int top = 20;
        const int right = 20;
        const int bottom = 45;
        var plot = new Rectangle(left, top, Math.Max(10, Width - left - right), Math.Max(10, Height - top - bottom));

        using var axisPen = new Pen(Color.FromArgb(70, 70, 70), 1.3f);
        using var gridPen = new Pen(Color.FromArgb(220, 220, 220), 1f);
        using var ePen = new Pen(Color.FromArgb(34, 102, 204), 2.2f);
        using var piPen = new Pen(Color.FromArgb(208, 70, 32), 2.2f);
        using var labelBrush = new SolidBrush(Color.FromArgb(40, 40, 40));

        var maxY = Math.Max(
            1,
            _samples.Max(sample => Math.Max(
                GetMetricValue(sample, true, Metric),
                GetMetricValue(sample, false, Metric))));
        var maxX = Math.Max(1, _samples.Max(sample => sample.Digits));

        for (var i = 0; i <= 5; i++)
        {
            var y = plot.Bottom - i * plot.Height / 5f;
            g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
            var value = maxY * i / 5;
            var text = value.ToString();
            var size = g.MeasureString(text, Font);
            g.DrawString(text, Font, labelBrush, plot.Left - size.Width - 6, y - size.Height / 2);
        }

        for (var i = 0; i <= 5; i++)
        {
            var x = plot.Left + i * plot.Width / 5f;
            g.DrawLine(gridPen, x, plot.Top, x, plot.Bottom);
            var value = maxX * i / 5;
            var text = value.ToString();
            var size = g.MeasureString(text, Font);
            g.DrawString(text, Font, labelBrush, x - size.Width / 2, plot.Bottom + 4);
        }

        g.DrawRectangle(axisPen, plot);
        DrawSeries(g, plot, _samples, Metric, true, maxX, maxY, ePen, Color.FromArgb(34, 102, 204));
        DrawSeries(g, plot, _samples, Metric, false, maxX, maxY, piPen, Color.FromArgb(208, 70, 32));

        DrawLegend(g);
        g.DrawString("Точность, цифр", Font, labelBrush, plot.Left + plot.Width / 2f - 55, Height - 24);
    }

    private void DrawLegend(Graphics g)
    {
        using var eBrush = new SolidBrush(Color.FromArgb(34, 102, 204));
        using var piBrush = new SolidBrush(Color.FromArgb(208, 70, 32));
        using var labelBrush = new SolidBrush(Color.FromArgb(40, 40, 40));

        g.FillEllipse(eBrush, 10, 10, 10, 10);
        g.DrawString("e^r", Font, labelBrush, 26, 6);
        g.FillEllipse(piBrush, 70, 10, 10, 10);
        g.DrawString("\u03C0", Font, labelBrush, 86, 6);
    }

    private static void DrawSeries(Graphics g, Rectangle plot, List<PrecisionSample> samples, PlotMetric metric, bool expSeries, int maxX, long maxY, Pen pen, Color markerColor)
    {
        using var brush = new SolidBrush(markerColor);
        var points = samples
            .Select(sample => new PointF(
                plot.Left + (float)sample.Digits / maxX * plot.Width,
                plot.Bottom - (float)GetMetricValue(sample, expSeries, metric) / maxY * plot.Height))
            .ToArray();

        if (points.Length > 1)
        {
            g.DrawLines(pen, points);
        }

        foreach (var point in points)
        {
            g.FillEllipse(brush, point.X - 3.5f, point.Y - 3.5f, 7, 7);
        }
    }

    private static long GetMetricValue(PrecisionSample sample, bool expSeries, PlotMetric metric)
    {
        var result = expSeries ? sample.Exp : sample.Pi;
        return metric == PlotMetric.Operations ? result.OperationCount : result.Terms;
    }
}

internal enum PlotMetric
{
    Operations,
    Terms
}

internal static class ArbitraryPrecisionMath
{
    public static ComputationResult ComputeExpOfNatural(int r, int digits)
    {
        var scale = digits + 10;
        var factor = Pow10(scale);
        var sum = factor;
        var term = factor;
        long terms = 1;
        long ops = 0;

        var watch = Stopwatch.StartNew();

        for (var k = 1; ; k++)
        {
            term = term * r;
            ops++;
            term /= k;
            ops++;

            if (term.IsZero)
            {
                break;
            }

            sum += term;
            ops++;
            terms++;
        }

        watch.Stop();

        return new ComputationResult(
            FormatScaled(sum, scale, digits),
            terms,
            ops,
            watch.ElapsedMilliseconds);
    }

    public static ComputationResult ComputePi(int digits)
    {
        var scale = digits + 10;
        long totalTerms = 0;
        long totalOps = 0;
        var watch = Stopwatch.StartNew();

        var a = ArctanInverse(5, scale, ref totalTerms, ref totalOps);
        var b = ArctanInverse(239, scale, ref totalTerms, ref totalOps);

        var pi = 16 * a;
        totalOps++;
        pi -= 4 * b;
        totalOps += 2;

        watch.Stop();

        return new ComputationResult(
            FormatScaled(pi, scale, digits),
            totalTerms,
            totalOps,
            watch.ElapsedMilliseconds);
    }

    private static BigInteger ArctanInverse(int q, int scale, ref long totalTerms, ref long totalOps)
    {
        var factor = Pow10(scale);
        var x = factor / q;
        totalOps++;
        var xSquared = factor / (q * q);
        totalOps++;

        var sum = x;
        var termPower = x;
        long localTerms = 1;

        for (var n = 1; ; n++)
        {
            termPower = (termPower * xSquared) / factor;
            totalOps += 2;

            if (termPower.IsZero)
            {
                break;
            }

            var next = termPower / (2 * n + 1);
            totalOps++;

            if (next.IsZero)
            {
                break;
            }

            if ((n & 1) == 1)
            {
                sum -= next;
            }
            else
            {
                sum += next;
            }

            totalOps++;
            localTerms++;
        }

        totalTerms += localTerms;
        return sum;
    }

    private static string FormatScaled(BigInteger value, int scale, int digits)
    {
        var factor = Pow10(scale);
        var integerPart = BigInteger.DivRem(value, factor, out var fractionalRaw);
        if (fractionalRaw.Sign < 0)
        {
            fractionalRaw = BigInteger.Abs(fractionalRaw);
        }

        var fractionalFull = fractionalRaw.ToString().PadLeft(scale, '0');
        var fractional = digits == 0 ? string.Empty : fractionalFull[..digits];

        return digits == 0 ? integerPart.ToString() : $"{integerPart}.{fractional}";
    }

    private static BigInteger Pow10(int power)
    {
        return BigInteger.Pow(10, power);
    }
}

internal sealed record ComputationResult(string ValueText, long Terms, long OperationCount, long ElapsedMilliseconds);

internal sealed record PrecisionSample(int Digits, ComputationResult Exp, ComputationResult Pi);
