using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Toto
{
    /// <summary>
    /// Interaction logic for Progress.xaml
    /// </summary>
    public partial class Progress
    {
	    private readonly BookManager _bookManager;
		private readonly Kupon[][] _superKupons;
	    readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();

	    public Kupon[] Kupons;
	    public SuperWinStorage[] WinStorage;
        public double[][][] IdiotKefs;
        public double[][][] Kefs;
        public int IdiotKuponsCount;

		public Progress(BookManager bookManager, Kupon[][] superKupons)
        {
            InitializeComponent();

            DataContext = new ProgressData();
			_bookManager = bookManager;
			_superKupons = superKupons;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync();
        }

        void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Kupons = _bookManager.GetKupons(_backgroundWorker, e, _superKupons, out WinStorage, out IdiotKefs, out Kefs, out IdiotKuponsCount);
        }

        void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
			DialogResult = !e.Cancelled;
			Owner.TaskbarItemInfo.ProgressValue = e.Cancelled ? 0 : 1;

			Close();
        }

	    void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var progressData = (ProgressData)DataContext;
            progressData.ProgressValue = e.ProgressPercentage;
	        Owner.TaskbarItemInfo.ProgressValue = e.ProgressPercentage / 100.0;
        }

        private void Break_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            _backgroundWorker.CancelAsync();
        }
    }

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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
