using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using OxyPlot;
using SummationApp.Models;
using SummationApp.Utilities;

namespace SummationApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private int _n = 10;
        private int _p = 2;
        private double _directResult;
        private double _asymptoticResult;
        private double _exactResult;
        private long _directOperations;
        private long _asymptoticOperations;
        private long _exactOperations;
        private PlotModel _operationsVsNPlot = new PlotModel();
        private PlotModel _operationsVsPPlot = new PlotModel();
        
        public ICommand UpdateCommand { get; }
        
        public int N
        {
            get => _n;
            set
            {
                if (_n != value)
                {
                    _n = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public int P
        {
            get => _p;
            set
            {
                if (_p != value)
                {
                    _p = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public double DirectResult
        {
            get => _directResult;
            set
            {
                if (_directResult != value)
                {
                    _directResult = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public double AsymptoticResult
        {
            get => _asymptoticResult;
            set
            {
                if (_asymptoticResult != value)
                {
                    _asymptoticResult = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public double ExactResult
        {
            get => _exactResult;
            set
            {
                if (_exactResult != value)
                {
                    _exactResult = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public long DirectOperations
        {
            get => _directOperations;
            set
            {
                if (_directOperations != value)
                {
                    _directOperations = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public long AsymptoticOperations
        {
            get => _asymptoticOperations;
            set
            {
                if (_asymptoticOperations != value)
                {
                    _asymptoticOperations = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public long ExactOperations
        {
            get => _exactOperations;
            set
            {
                if (_exactOperations != value)
                {
                    _exactOperations = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public PlotModel OperationsVsNPlot
        {
            get => _operationsVsNPlot;
            set
            {
                if (_operationsVsNPlot != value)
                {
                    _operationsVsNPlot = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public PlotModel OperationsVsPPlot
        {
            get => _operationsVsPPlot;
            set
            {
                if (_operationsVsPPlot != value)
                {
                    _operationsVsPPlot = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public MainViewModel()
        {
            UpdateCommand = new RelayCommand(_ =>
            {
                CalculateResults();
                UpdatePlots();
            });
            CalculateResults();
            UpdatePlots();
        }
        
        private void CalculateResults()
        {
            DirectResult = SummationMethods.DirectSummation(N, P, out long directOps);
            DirectOperations = directOps;
            
            AsymptoticResult = SummationMethods.AsymptoticSummation(N, P, out long asymptoticOps);
            AsymptoticOperations = asymptoticOps;
            
            ExactResult = SummationMethods.ExactSummation(N, P, out long exactOps);
            ExactOperations = exactOps;
        }
        
        private void UpdatePlots()
        {
            OperationsVsNPlot = PlotBuilder.BuildOperationsVsNPlot(P);
            OperationsVsPPlot = PlotBuilder.BuildOperationsVsPPlot(N);
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}