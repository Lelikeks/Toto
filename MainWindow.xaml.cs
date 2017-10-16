using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Text;

namespace Toto
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
	    private BookManager _bookManager;

        public MainWindow()
        {
            InitializeComponent();

            SetCaption();
            DataContext = new Data();
        }

        private void SetCaption()
        {
            Title += Properties.Resources.BuildDate;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var data = (Data)DataContext;
            data.AddressFon = Properties.Settings.Default.AddressFon;
            data.AddressBolt = Properties.Settings.Default.AddressBolt;
            data.KuponsCount = Properties.Settings.Default.KuponsCount;
            data.TirazhsCount = Properties.Settings.Default.TirazhsCount;
            data.IsFon = Properties.Settings.Default.IsFon;
            data.UseIdiotKupons = Properties.Settings.Default.UseIdiotKupons;

            if (!data.IsFon)
            {
                IsBolt.IsChecked = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var data = (Data)DataContext;
            Properties.Settings.Default.AddressFon = data.AddressFon;
            Properties.Settings.Default.AddressBolt = data.AddressBolt;
            Properties.Settings.Default.KuponsCount = data.KuponsCount;
            Properties.Settings.Default.TirazhsCount = data.TirazhsCount;
            Properties.Settings.Default.IsFon = data.IsFon;
            Properties.Settings.Default.UseIdiotKupons = data.UseIdiotKupons;
            Properties.Settings.Default.Save();
        }

		private void Calculate_Click(object sender, RoutedEventArgs e)
		{
			TaskbarItemInfo.ProgressValue = 0;

			var data = (Data)DataContext;
			if (data.GeneratedKuponsCount > data.KuponsCount)
			{
				MessageBox.Show("Количество сгенерированных купонов больше чем общее количество купонов, расчет невозможен.");
				return;
			}

            IdiotKuponsProvider idiotsProvider = null;
            if (data.UseIdiotKupons)
            {
                var drawingId = GetDrawingId(data.AddressFon);
                idiotsProvider = new IdiotKuponsProvider(drawingId);
            }

			_bookManager = new BookManager(data.Address, data.KuponsCount, data.TirazhsCount, data.IsFon, idiotsProvider);

			var sw = Stopwatch.StartNew();
			var wnd = new Progress(_bookManager, data.GeneratedKupons) { Owner = this };
			if (wnd.ShowDialog() == true)
			{
				if (data.GeneratedKuponsCount == 0)
				{
					data.Kupons = wnd.Kupons.Where(i => i.MoneyWin > 0.0).OrderByDescending(i => i.MoneyWin).Take(30).Select(i => new KuponForShow
					{
						MoneyWin = i.MoneyWin,
						ROI = i.MoneyWin / (data.TirazhsCount * 50),
						Cells = KuponToShow(i.Cells)
					});
					data.SuperWinStorage = null;
				}
				else
				{
					data.SuperWinStorage = wnd.WinStorage.Select(i => i == null ? null : new[] { i }).ToArray();
				}
				data.Nazhor = GetNazhorCollection(wnd, data);
                data.Kefs = wnd.Kefs;

				sw.Stop();
				data.Message = string.Format("Выполнено за {0}", sw.Elapsed.ToString(@"h\:m\:ss"));
                data.Output = PrintIdiotKefs(wnd.IdiotKefs, wnd.Kefs);
			}
			else
			{
				sw.Stop();
				data.Kupons = null;
				data.Nazhor = null;
				data.GeneratedKupons = null;
                data.Kefs = null;
				data.Message = "Выполнение отменено";
				TaskbarItemInfo.ProgressValue = 0;
                data.Output = null;
			}
		}

        private string PrintIdiotKefs(double[][][] idiotKefs, double[][][] kefs)
        {
            if (idiotKefs == null)
            {
                return null;
            }

            var res = new StringBuilder();
            for (int i = 0; i < 15; i++)
            {
                res.AppendFormat("{0}:\t", i + 1);
                for (int j = 0; j < 3; j++)
                {
                    res.AppendFormat("{0:0.00}/{1:0.00}\t", kefs[i][j][1] * 100, idiotKefs[i][j][1] * 100);
                }
                res.AppendLine();
            }
            return res.ToString();
        }

        private int GetDrawingId(string address)
        {
            var match = Regex.Match(address, @"\d+");
            if (!match.Success)
            {
                throw new Exception(string.Format("Не удается получить номера тиража из адреса: {0}.", address));
            }
            return int.Parse(match.Value);
        }

	    private static ObservableCollection<Nazhor[]> GetNazhorCollection(Progress wnd, Data data)
	    {
		    var nazhor = new Nazhor[3];
		    for (var j = 0; j < 3; j++)
		    {
			    nazhor[j] = new Nazhor
				    {
					    Number = j == 0 ? "п1" : (j == 1 ? " х" : "п2")
				    };
			    for (var i = 0; i < 15; i++)
			    {
				    var i1 = i;
				    var j1 = j;
				    var kups = wnd.Kupons.Skip(wnd.IdiotKuponsCount).Where(k => (k.Cells[j1] & (1 << i1)) > 0);
                    var count = kups.Count();
				    nazhor[j].ROI[i] = count == 0 ? 0 : kups.Sum(k => k.MoneyWin)/(count*50.0*data.TirazhsCount);
			    }
		    }

			var nazhorCollection = new ObservableCollection<Nazhor[]>();
		    for (var i = 0; i < 5; i++)
		    {
			    var nz = new Nazhor[3];
			    for (var j = 0; j < 3; j++)
			    {
				    nz[j] = new Nazhor
					    {
						    Number = nazhor[j].Number,
						    ROI = nazhor[j].ROI,
						    Checked = data.Nazhor == null
							              ? new ObservableCollection<bool>(nazhor[j].Checked)
							              : new ObservableCollection<bool>(data.Nazhor[i][j].Checked)
					    };
			    }
			    nazhorCollection.Add(nz);
		    }
		    return nazhorCollection;
	    }

	    private string[] KuponToShow(short[] cells)
		{
			var res = new string[15];
			for (var i = 0; i < 15; i++)
			{
				if ((cells[0] & (1 << i)) > 0)
				{
					res[i] = "1";
				}
				else if ((cells[1] & (1 << i)) > 0)
				{
					res[i] = "x";
				}
				else if ((cells[2] & (1 << i)) > 0)
				{
					res[i] = "2";
				}
			}
			return res;
		}

        private void NazhorGrid_KuponsGenerated(object sender, RoutedEventArgs e)
        {
            var data = (Data)DataContext;
            data.FirePropertyChanged("HasGeneratedKupons");
        }

        private void NazhorGrid_SendToDB1(object sender, RoutedEventArgs e)
        {
            SendToDB(1);
        }

        private void NazhorGrid_SendToDB2(object sender, RoutedEventArgs e)
        {
            SendToDB(2);
        }
        private void NazhorGrid_SendToDB3(object sender, RoutedEventArgs e)
        {
            SendToDB(3);
        }
        private void NazhorGrid_SendToDB4(object sender, RoutedEventArgs e)
        {
            SendToDB(4);
        }
        private void NazhorGrid_SendToDB5(object sender, RoutedEventArgs e)
        {
            SendToDB(5);
        }

        private void SendToDB(int nazhorNum)
        {
            if (MessageBox.Show("Вы уверены, что хотите отправить шаблон в базу данных?", "Вопрос", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            var betString = GetBetString(nazhorNum);
            var data = (Data)DataContext;
            var drawingId = GetDrawingId(data.AddressFon);

            try
            {
                RemoteDBManager.Send(drawingId, betString);
                MessageBox.Show("Шаблон был успешно сохранен в базе данных.", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка:\r\n" + ex.Message + "\r\n\r\nПопробуйте отправить шаблон заново.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetBetString(int nazhorNum)
        {
            var res = "";

            var data = (Data)DataContext;
            var nazhor = data.Nazhor[nazhorNum - 1];

            for (int i = 0; i < 15; i++)
            {
                var cur = nazhor[0].Checked[i] ? "1" : "";
                cur += nazhor[1].Checked[i] ? "0" : "";
                cur += nazhor[2].Checked[i] ? "2" : "";

                if (cur == string.Empty)
                {
                    throw new Exception(string.Format("Не отмечен исход события {0} в шаблоне №{1}.", i + 1, nazhorNum));
                }

                res += cur;
                if (i < 14)
                {
                    res += " ";
                }
            }

            return res;
        }

        private void CheckForMe2_Click(object sender, RoutedEventArgs e)
        {
            nazhor1.CheckForMe2(GetCheckForMe2Items(0, 0.01));
            nazhor2.CheckForMe2(GetCheckForMe2Items(1, 2));
            nazhor3.CheckForMe2(GetCheckForMe2Items(2, 3));
            nazhor4.CheckForMe2(GetCheckForMe2Items(3, 5));
            nazhor5.CheckForMe2(GetCheckForMe2Items(4, 100));
        }

        private bool[][] GetCheckForMe2Items(int nazhorIndex, double A)
        {
            var result = new bool[15][];
            var data = (Data)DataContext;
            var nazhor = data.Nazhor[nazhorIndex];

            var collection = Enumerable.Range(0, 15).SelectMany(i => Enumerable.Range(0, 3)
                .Select(j => new { I = i, J = j, ROI2 = CalcROI2(nazhor[j].ROI[i], data.Kefs[i][j][0], A) })).ToList();

            for (int i = 0; i < 15; i++)
            {
                var max = collection.Where(k => k.I == i).OrderByDescending(k => k.ROI2).First();

                result[i] = new bool[3];
                result[i][max.J] = true;

                collection.Remove(max);
            }

            var rest = collection.OrderByDescending(k => k.ROI2).Take(8);
            foreach (var item in rest)
            {
                result[item.I][item.J] = true;
            }

            return result;
        }

        private double CalcROI2(double roi, double kef, double A)
        {
            return roi * A + kef;
        }
    }

	public class KuponForShow
	{
		public double MoneyWin { get; set; }
		public double ROI { get; set; }
		public string[] Cells { get; set; }
	}

	public class Nazhor
	{
		public Nazhor()
		{
			ROI = new double[15];
			Checked = new ObservableCollection<bool>();
			for (var i = 0; i < 15; i++)
			{
				Checked.Add(false);
			}
		}

		public string Number { get; set; }
		public double[] ROI { get; set; }
		public ObservableCollection<bool> Checked { get; set; }
	}

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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public double[][][] Kefs { get; set; }
    }
}
