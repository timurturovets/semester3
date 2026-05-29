using System.ComponentModel;
using System.Runtime.CompilerServices;
using application.Models;

namespace application.ViewModels;

public sealed class GeneratorEntry : INotifyPropertyChanged
{
    private int _xExponent;
    private int _yExponent;

    public GeneratorEntry()
    {
    }

    public GeneratorEntry(MonomialGenerator generator)
    {
        _xExponent = generator.XExponent;
        _yExponent = generator.YExponent;
    }

    public int XExponent
    {
        get => _xExponent;
        set
        {
            if (_xExponent == value)
            {
                return;
            }

            _xExponent = value;
            OnPropertyChanged();
        }
    }

    public int YExponent
    {
        get => _yExponent;
        set
        {
            if (_yExponent == value)
            {
                return;
            }

            _yExponent = value;
            OnPropertyChanged();
        }
    }

    public MonomialGenerator ToGenerator()
    {
        return new MonomialGenerator(XExponent, YExponent);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
