using System;
using System.ComponentModel;
using System.Windows.Media;


namespace NetTrace
{
    public class DlgTagBinding : INotifyPropertyChanged
    {
        private bool _fOn;

        public bool FOn
        {
            get => _fOn;
            set
            {
                if (_fOn != value)
                {
                    _fOn = value;
                    NotifyPropertyChanged("FOn");
                }
            }
        }
        // ReSharper disable once UnusedMember.Global
        public string Name => ToString();

        public Brush Color
        {
            // ReSharper disable once UnusedMember.Global
            get => _color;
            set
            {
                if (((SolidColorBrush)_color).Color != ((SolidColorBrush)value).Color)
                {
                    _color = value;
                    NotifyPropertyChanged("Color");
                }
            }
        }

        private Brush _color;

        public string? StrDesc { get; }
        public Type TpEnum { get; }
        public string StrTag { get; }

        public DlgTagBinding(string strTag, bool fOn, string? strDesc, Type tpEnum)
        {
            FOn = fOn;
            StrDesc = strDesc;
            TpEnum = tpEnum;
            StrTag = strTag;
            _color = new SolidColorBrush(Colors.Black);
        }

        public override string ToString()
        {
            return StrDesc ?? StrTag;
        }

        private void NotifyPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
