using System.ComponentModel;

namespace Toto
{
    public class ProgressData : INotifyPropertyChanged
    {
        int _progressValue;

        public int ProgressValue
        {
            get { return _progressValue; }
            set
            {
                if (value != _progressValue)
                {
                    _progressValue = value;
                    FirePropertyChanged("ProgressValue");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void FirePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
