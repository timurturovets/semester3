using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using SummationApp.Models;

namespace SummationApp.Utilities
{
    public static class PlotBuilder
    {
        public static PlotModel BuildOperationsVsNPlot(int p, int maxN = 100)
        {
            var plotModel = new PlotModel { Title = "Зависимость числа операций от n (p = " + p + ")" };
            
            var directSeries = new LineSeries { Title = "Прямой метод" };
            var asymptoticSeries = new LineSeries { Title = "Асимптотический метод" };
            var exactSeries = new LineSeries { Title = "Точный метод" };
            
            for (int n = 1; n <= maxN; n++)
            {
                directSeries.Points.Add(new DataPoint(n, OperationsCounter.CountDirectOperations(n, p)));
                asymptoticSeries.Points.Add(new DataPoint(n, OperationsCounter.CountAsymptoticOperations(n, p)));
                exactSeries.Points.Add(new DataPoint(n, OperationsCounter.CountExactOperations(n, p)));
            }
            
            plotModel.Series.Add(directSeries);
            plotModel.Series.Add(asymptoticSeries);
            plotModel.Series.Add(exactSeries);
            
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "n" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Число операций" });
            
            return plotModel;
        }
        
        public static PlotModel BuildOperationsVsPPlot(int n, int maxP = 10)
        {
            var plotModel = new PlotModel { Title = "Зависимость числа операций от p (n = " + n + ")" };
            
            var directSeries = new LineSeries { Title = "Прямой метод" };
            var asymptoticSeries = new LineSeries { Title = "Асимптотический метод" };
            var exactSeries = new LineSeries { Title = "Точный метод" };
            
            for (int p = 1; p <= maxP; p++)
            {
                directSeries.Points.Add(new DataPoint(p, OperationsCounter.CountDirectOperations(n, p)));
                asymptoticSeries.Points.Add(new DataPoint(p, OperationsCounter.CountAsymptoticOperations(n, p)));
                exactSeries.Points.Add(new DataPoint(p, OperationsCounter.CountExactOperations(n, p)));
            }
            
            plotModel.Series.Add(directSeries);
            plotModel.Series.Add(asymptoticSeries);
            plotModel.Series.Add(exactSeries);
            
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "p" });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Число операций" });
            
            return plotModel;
        }
    }
}