using System;
using System.ComponentModel;
using System.Windows.Media;

namespace WPFTrace
{
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Per tag info that is passed to the SetTraceTags dialog. </summary>
    ///
    /// <remarks>	Darrellp, 10/5/2012. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    public class TagInfo : INotifyPropertyChanged
    {
        public TagInfo(string strTag, bool fOn, string strDesc, Type tpEnum)
        {
            FOn = fOn;
            StrDesc = strDesc;
            TpEnum = tpEnum;
            StrTag = strTag;
            Color = null;
        }

        private bool _fOn;

        public bool FOn
        {
            get => _fOn;
            set
            {
                if (_fOn == value)
                {
                    return;
                }

                _fOn = value;
                NotifyPropertyChanged("FOn");
            }
        }

        public Brush Color
        {
            get { return _color; }
            set
            {
                if (_color == null || ((SolidColorBrush)_color).Color != ((SolidColorBrush)value).Color)
                {
                    _color = value;
                    NotifyPropertyChanged("Color");
                }
            }
        }

        private Brush _color;

        public string StrDesc { get; set; }
        public Type TpEnum { get; set; }
        public string StrTag { get; private set; }
        public string Name => ToString();

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