using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AmadeusW.Ambilight.DataClasses
{
    public class PresetModel : INotifyPropertyChanged
    {
        #region Fields

        private ObservableCollection<Preset> _presets;

        public ObservableCollection<Preset> Presets
        {
            get { return _presets; }
            set
            {
                _presets = value;
                SendPropertyChanged("Presets");
            }
        }

        #endregion

        #region Public methods

        public PresetModel()
        {
            Presets = new ObservableCollection<Preset>(new List<Preset>());
        }

        #endregion

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private methods

        private void SendPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion
    }
}