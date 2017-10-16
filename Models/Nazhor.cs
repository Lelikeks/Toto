using System.Collections.ObjectModel;

namespace Toto
{
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
}
