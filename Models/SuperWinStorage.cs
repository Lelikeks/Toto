using System.Linq;

namespace Toto
{
    public class SuperWinStorage
    {
        private readonly long _tirazhsCount;
        private readonly int[] _storage = new int[7];
        private readonly Kupon[] _kupons;

        public SuperWinStorage(long tirazhsCount, Kupon[] kupons)
        {
            _tirazhsCount = tirazhsCount;
            _kupons = kupons;
        }

        public void Add(int successes)
        {
            _storage[successes - 9]++;
        }

        public double[] Results
        {
            get { return _storage.Select(i => (double)i / _tirazhsCount).ToArray(); }
        }

        public double ROI
        {
            get
            {
                if (_kupons.Length == 0)
                {
                    return 0;
                }
                return _kupons.Sum(i => i.MoneyWin) / (_tirazhsCount * 50 * _kupons.Length);
            }
        }
    }
}
