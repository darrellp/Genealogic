using System;
using System.ComponentModel;
using System.Windows.Media;

namespace NetTrace
{
    // This class lives just to provide a binding to the NetTrace dialog for each tag entry
    public class DlgTagBinding : INotifyPropertyChanged
    {
        #region Private variables
        // Whther the tag is enabled for tracing or not
        private bool _fOn;
        // Description of tag
        public string? StrDesc { get; }
        // The enum this tag belongs to
        public Type TpEnum { get; }
        // The string to use in the dialog
        public string StrTag { get; }
        // Color of the tag
        private Brush _color;
        #endregion

        #region Properties
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
        // It IS used in the binding to the dialog so DO NOT delete this!
        public string Name => ToString();

        public Brush Color
        {
            // ReSharper disable once UnusedMember.Global
            // As in Name, this IS used by the dialog binding.  DO NOT DELETE!
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

        // Not a property but used by Name
        public override string ToString()
        {
            return StrDesc ?? StrTag;
        }
        #endregion

        #region Constructor
        public DlgTagBinding(string strTag, bool fOn, string? strDesc, Type tpEnum)
        {
            FOn = fOn;
            StrDesc = strDesc;
            TpEnum = tpEnum;
            StrTag = strTag;
            _color = new SolidColorBrush(Colors.Black);
        }
        #endregion

        #region Event handling
        private void NotifyPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion
    }
}
