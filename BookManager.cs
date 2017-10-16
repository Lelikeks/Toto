using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;

namespace Toto
{
    public class BookManager
    {
        private readonly string _address;
        private readonly long _kuponsCount;
        private readonly long _tirazhsCount;
        private readonly double _prize;
        private readonly bool _isFon;
        private readonly IdiotKuponsProvider _idiotKuponsProvider;
        static readonly Random Rnd = new Random();

        public BookManager(string address, long kuponsCount, long tirazhsCount, bool isFon, IdiotKuponsProvider idiotKuponsProvider)
        {
            _address = address;
            _kuponsCount = kuponsCount;
            _tirazhsCount = tirazhsCount;
            _isFon = isFon;
            _prize = _kuponsCount * 50 * .9;
            _idiotKuponsProvider = idiotKuponsProvider;
        }

        public Kupon[] GetKupons(BackgroundWorker backgroundWorker, DoWorkEventArgs e, Kupon[][] superKupons, out SuperWinStorage[] winStorage, out double[][][] idiotKefs, out double[][][] kefs, out int idiotKuponsCount)
        {
            winStorage = new SuperWinStorage[5];
            for (var i = 0; i < 5; i++)
            {
                if (superKupons[i].Length > 0)
                {
                    winStorage[i] = new SuperWinStorage(_tirazhsCount, superKupons[i]);
                }
            }
            var successes = new List<Kupon>[7];
            for (var i = 0; i < 7; i++)
            {
                successes[i] = new List<Kupon>();
            }

            kefs = GetKefs();
            var kupons = GetKupons(superKupons, kefs, _idiotKuponsProvider, out bool needSupers, out idiotKefs, out idiotKuponsCount);

            var oldPercent = 0;
            for (var i = 0; i < _tirazhsCount; i++)
            {
                var tirazh = GetRndArray(kefs, 0);
                EvaluateTirazh(kupons, tirazh, successes);

                if (needSupers)
                {
                    ProcessSupers(superKupons, winStorage);
                }

                if (_isFon)
                {
                    SetPrizes(9, .32, successes);
                    SetPrizes(10, .18, successes);
                    SetPrizes(11, .1, successes);
                    SetPrizes(12, .1, successes);
                    SetPrizes(13, .1, successes);
                }
                else
                {
                    SetPrizes(9, .4, successes);
                    SetPrizes(10, .2, successes);
                    SetPrizes(11, .1, successes);
                    SetPrizes(12, .05, successes);
                    SetPrizes(13, .05, successes);
                }

                if (backgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return null;
                }
                var newPercent = (int)((double)i / _tirazhsCount * 100);
                if (newPercent > oldPercent)
                {
                    oldPercent = newPercent;
                    backgroundWorker.ReportProgress(newPercent);
                }
            }

            return kupons;
        }

        private static void ProcessSupers(Kupon[][] superKupons, SuperWinStorage[] winStorage)
        {
            for (var j = 0; j < 5; j++)
            {
                if (superKupons[j].Length > 0)
                {
                    var max = superKupons[j].Max(k => k.Successes);
                    if (max > 8)
                    {
                        winStorage[j].Add(max);
                    }
                }
            }
        }

        private Kupon[] GetKupons(Kupon[][] superKupons, double[][][] kefs, IdiotKuponsProvider idiotsProvider, out bool needSupers, out double[][][] idiotKefs, out int idiotKuponsCount)
        {
            idiotKefs = null;
            idiotKuponsCount = 0;

            var kupons = new Kupon[_kuponsCount];

            var supers = superKupons.SelectMany(i => i).ToArray();
            foreach (var super in supers)
            {
                super.MoneyWin = 0;
            }
            supers.CopyTo(kupons, 0);

            long start = supers.Length;
            needSupers = supers.Length > 0;

            if (idiotsProvider != null)
            {
                IdiotsKupon[] idiotsKupons = GetIdiotsKupons(idiotsProvider, kupons, ref start);
                if (idiotsKupons != null)
                {
                    idiotKefs = kefs = ChangeKefsForIdiots(kefs, idiotsKupons, out idiotKuponsCount);
                }
            }

            for (var i = start; i < _kuponsCount; i++)
            {
                kupons[i] = new Kupon
                {
                    Cells = GetRndArray(kefs, 1)
                };
            }
            return kupons;
        }

        private static IdiotsKupon[] GetIdiotsKupons(IdiotKuponsProvider idiotsProvider, Kupon[] kupons, ref long start)
        {
            IdiotsKupon[] idiotsKupons = null;
            while (idiotsKupons == null)
            {
                try
                {
                    idiotsKupons = idiotsProvider.GetKupons();
                }
                catch
                {
                    var res = MessageBox.Show("Не удалось загрузить идиотские купоны. Попробовать еще раз?\r\nДа - еще одна попытка.\r\nНет - пропустить загрузку и продолжить работу без учета идиотских купонов.", "Ошибка", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (res == MessageBoxResult.No)
                    {
                        return null;
                    }
                }
            }

            foreach (var idiot in idiotsKupons)
            {
                for (int i = 0; i < idiot.Count; i++)
                {
                    kupons[start] = new Kupon { Cells = idiot.Cells };
                    start++;
                }
            }
            return idiotsKupons;
        }

        private double[][][] ChangeKefsForIdiots(double[][][] kefs, IdiotsKupon[] idiotsKupons, out int totalIdiots)
        {
            var res = new double[15][][];
            totalIdiots = idiotsKupons.Sum(k => k.Count);

            for (int i = 0; i < 15; i++)
            {
                var bit = (short)(1 << i);
                res[i] = new double[3][];
                for (int j = 0; j < 3; j++)
                {
                    res[i][j] = new double[2];
                }

                var p1 = idiotsKupons.Where(k => (k.Cells[0] & bit) == bit).Sum(k => k.Count);
                var newPercent = (kefs[i][0][1] * _kuponsCount - p1) / (_kuponsCount - totalIdiots);
                res[i][0][0] = kefs[i][0][0];
                res[i][0][1] = newPercent;

                var px = idiotsKupons.Where(k => (k.Cells[1] & bit) == bit).Sum(k => k.Count);
                newPercent = (kefs[i][1][1] * _kuponsCount - px) / (_kuponsCount - totalIdiots);
                res[i][1][0] = kefs[i][1][0];
                res[i][1][1] = newPercent;

                var p2 = idiotsKupons.Where(k => (k.Cells[2] & bit) == bit).Sum(k => k.Count);
                newPercent = (kefs[i][2][1] * _kuponsCount - p2) / (_kuponsCount - totalIdiots);
                res[i][2][0] = kefs[i][2][0];
                res[i][2][1] = newPercent;

                if (Math.Abs(res[i][0][1] + res[i][1][1] + res[i][2][1] - 1.0) > .001)
                {
                    throw new Exception("Сумма коэффициентов не равна единице.");
                }
            }

            return res;
        }

        void SetPrizes(int level, double percentOfPrize, List<Kupon>[] successes)
        {
            var count = 0;
            for (var i = level; i < 16; i++)
            {
                count += successes[i - 9].Count;
            }

            var prizePerWinner = _prize * percentOfPrize / count;
            for (var i = level; i < 16; i++)
            {
                foreach (var kupon in successes[i - 9])
                {
                    kupon.MoneyWin += prizePerWinner;
                }
            }
        }

        static int NumberOfSetBits(int i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        private void EvaluateTirazh(Kupon[] kupons, short[] tirazh, List<Kupon>[] successes)
        {
            for (var i = 0; i < 7; i++)
            {
                successes[i].Clear();
            }
            for (var i = 0; i < _kuponsCount; i++)
            {
                var union = (kupons[i].Cells[0] & tirazh[0]) | (kupons[i].Cells[1] & tirazh[1]) | (kupons[i].Cells[2] & tirazh[2]);
                kupons[i].Successes = NumberOfSetBits(union);
                if (kupons[i].Successes > 8)
                {
                    successes[kupons[i].Successes - 9].Add(kupons[i]);
                }
            }
        }

        private static short[] GetRndArray(double[][][] kefs, long type)
        {
            var res = new short[3];
            for (short i = 0; i < 15; i++)
            {
                byte issue;
                var rnd = Rnd.NextDouble();

                if (rnd < kefs[i][0][type])
                {
                    issue = 0;
                }
                else if (rnd < kefs[i][0][type] + kefs[i][1][type])
                {
                    issue = 1;
                }
                else
                {
                    issue = 2;
                }

                res[issue] |= (short)(1 << i);
            }

            return res;
        }

        double[][][] GetKefs()
        {
            var page = (new WebClient()).DownloadString(_address);
            var res = new double[15][][];

            var pattern = @"(?<buk>\d+\.\d+)\s/\s(?<narod>\d+\.\d+)";
            if (!_isFon)
            {
                pattern = @"\>(?<narod>\d\d)%/(?<buk>\d\d)%\s\<";
            }

            var rx = new Regex(pattern);
            var nfi = CultureInfo.CurrentCulture.NumberFormat.Clone() as NumberFormatInfo;
            nfi.NumberDecimalSeparator = ".";

            var i = 0;
            foreach (Match match in rx.Matches(page))
            {
                if (i % 3 == 0)
                {
                    res[i / 3] = new double[3][];
                }
                res[i / 3][i % 3] = new double[2];
                res[i / 3][i % 3][0] = double.Parse(match.Groups["buk"].Value, nfi) / 100.0;
                res[i / 3][i % 3][1] = double.Parse(match.Groups["narod"].Value, nfi) / 100.0;

                i++;
            }
            return res;
        }

        void PrintKefs(double[][][] kefs)
        {
            for (int i = 0; i < 15; i++)
            {
                Debug.WriteLine("{0} / {1}    {2} / {3}   {4} / {5}", kefs[i][0][0], kefs[i][0][1], kefs[i][1][0], kefs[i][1][1], kefs[i][2][0], kefs[i][2][1]);
            }
        }
    }
}
