using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Toto
{
    public class Data : INotifyPropertyChanged
    {
        public bool HasGeneratedKupons
        {
            get
            {
                return GeneratedKuponsCount > 0;
            }
        }

        private SuperWinStorage[][] _superWinStorage;

        public SuperWinStorage[][] SuperWinStorage
        {
            get { return _superWinStorage; }
            set
            {
                _superWinStorage = value;
                FirePropertyChanged("SuperWinStorage");
            }
        }

        public int GeneratedKuponsCount
        {
            get { return GeneratedKupons.Sum(i => i.Length); }
        }

        private Kupon[][] _generatedKupons = new[]
            {
                new Kupon[0], new Kupon[0], new Kupon[0], new Kupon[0], new Kupon[0]
            };

        public Kupon[][] GeneratedKupons
        {
            get { return _generatedKupons; }
            set
            {
                if (value == null)
                {
                    _generatedKupons = new[]
                        {
                            new Kupon[0], new Kupon[0], new Kupon[0], new Kupon[0], new Kupon[0]
                        };
                }
                else
                {
                    _generatedKupons = value;
                }
                FirePropertyChanged("GeneratedKupons");
            }
        }

        private bool _isFon;

        public bool IsFon
        {
            get { return _isFon; }
            set
            {
                _isFon = value;
                FirePropertyChanged("IsFon");
                FirePropertyChanged("Address");
            }
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set
            {
                if (value != _message)
                {
                    _message = value;
                    FirePropertyChanged("Message");
                }
            }
        }

        private string _output;

        public string Output
        {
            get { return _output; }
            set
            {
                if (value != _output)
                {
                    _output = value;
                    FirePropertyChanged("Output");
                }
            }
        }

        private ObservableCollection<Nazhor[]> _nazhor;

        public ObservableCollection<Nazhor[]> Nazhor
        {
            get { return _nazhor; }
            set
            {
                if (!Equals(value, _nazhor))
                {
                    _nazhor = value;
                    FirePropertyChanged("Nazhor");
                }
            }
        }

        private IEnumerable<KuponForShow> _kupons;

        public IEnumerable<KuponForShow> Kupons
        {
            get { return _kupons; }
            set
            {
                if (!Equals(value, _kupons))
                {
                    _kupons = value;
                    FirePropertyChanged("Kupons");
                }
            }
        }

        public string Address
        {
            get { return IsFon ? AddressFon : AddressBolt; }
            set
            {
                if (IsFon)
                {
                    AddressFon = value;
                }
                else
                {
                    AddressBolt = value;
                }
            }
        }

        public string AddressFon;
        public string AddressBolt;

        long _kuponsCount;

        public long KuponsCount
        {
            get { return _kuponsCount; }
            set
            {
                if (value != _kuponsCount)
                {
                    _kuponsCount = value;
                    FirePropertyChanged("KuponsCount");
                }
            }
        }

        bool _useIdiotKupons;

        public bool UseIdiotKupons
        {
            get { return _useIdiotKupons; }
            set
            {
                if (value != _useIdiotKupons)
                {
                    _useIdiotKupons = value;
                    FirePropertyChanged("UseIdiotKupons");
                }
            }
        }

        long _tirazhCount;

        public long TirazhsCount
        {
            get { return _tirazhCount; }
            set
            {
                if (value != _tirazhCount)
                {
                    _tirazhCount = value;
                    FirePropertyChanged("TirazhsCount");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void FirePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double[][][] Kefs { get; set; }
    }
}
