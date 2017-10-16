using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Windows.Controls;

namespace Toto.Controls
{
	/// <summary>
	/// Interaction logic for NazhorGrid.xaml
	/// </summary>
	public partial class NazhorGrid : INotifyPropertyChanged
	{
        bool _IsCheckedPacket;

		public NazhorGrid()
		{
			InitializeComponent();
		}

        public void CheckForMe2(bool[][] checkedItems)
        {
            _IsCheckedPacket = true;

            for (var i = 0; i < 15; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Nazhor[j].Checked[i] = checkedItems[i][j];
                }
            }

            _IsCheckedPacket = false;
            ToggleButton_OnChecked(null, null);
        }

		public Nazhor[] Nazhor
		{
			get { return (Nazhor[])GetValue(NazhorProperty); }
			set { SetValue(NazhorProperty, value); }
		}

		public static readonly DependencyProperty NazhorProperty = DependencyProperty.Register("Nazhor", typeof(Nazhor[]), typeof(NazhorGrid), new PropertyMetadata());

		public int CellsSelected
		{
			get
			{
				if (Nazhor == null)
				{
					return 0;
				}
				return Nazhor.Sum(i => i.Checked.Count(j => j));
			}
		}

		public Kupon[] GeneratedKupons
		{
			get { return (Kupon[])GetValue(GeneratedKuponsProperty); }
			set { SetValue(GeneratedKuponsProperty, value); }
		}

		public static readonly DependencyProperty GeneratedKuponsProperty = DependencyProperty.Register("GeneratedKupons", typeof(Kupon[]), typeof(NazhorGrid), new PropertyMetadata(GeneratedKuponsChanged));

		private static void GeneratedKuponsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var kupons = dependencyPropertyChangedEventArgs.NewValue as Kupon[];
			if (kupons != null && kupons.Length == 0)
			{
				((NazhorGrid)dependencyObject).FirePropertyChanged("CellsSelected");
			}
		}

		public static readonly RoutedEvent KuponsGeneratedEvent = EventManager.RegisterRoutedEvent("KuponsGenerated", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NazhorGrid));

        public event RoutedEventHandler KuponsGenerated
        {
            add { AddHandler(KuponsGeneratedEvent, value); }
            remove { RemoveHandler(KuponsGeneratedEvent, value); }
        }

        public static readonly RoutedEvent SendToDBEvent = EventManager.RegisterRoutedEvent("SendToDB", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NazhorGrid));

        public event RoutedEventHandler SendToDB
        {
            add { AddHandler(SendToDBEvent, value); }
            remove { RemoveHandler(SendToDBEvent, value); }
        }

        public bool CanSendToDB
        {
            get
            {
                if (Nazhor == null)
                {
                    return false;
                }

                for (int i = 0; i < 15; i++)
                {
                    if (Nazhor[0].Checked[i] == false && Nazhor[1].Checked[i] == false && Nazhor[2].Checked[i] == false)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
		{
            if (!_IsCheckedPacket)
            {
                GeneratedKupons = Func(new short[3], 0).ToArray();

                FirePropertyChanged("CellsSelected");
                FirePropertyChanged("GeneratedKupons");
                FirePropertyChanged("CanSendToDB");

                RaiseEvent(new RoutedEventArgs(KuponsGeneratedEvent));
            }
		}

        private void CheckForMe_OnClick(object sender, RoutedEventArgs e)
        {
            if (Nazhor == null)
            {
                return;
            }
            var tops = Nazhor.SelectMany(i => i.ROI).OrderByDescending(i => i).Take(20);

            _IsCheckedPacket = true;
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 15; j++)
                {
					Nazhor[i].Checked[j] = tops.Contains(Nazhor[i].ROI[j]);
                }
            }
            _IsCheckedPacket = false;
            ToggleButton_OnChecked(null, null);
        }

        private void ClearNazhorChecks_OnClick(object sender, RoutedEventArgs e)
        {
            if (Nazhor == null)
            {
                return;
            }

            _IsCheckedPacket = true;
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 15; j++)
                {
                    Nazhor[i].Checked[j] = false;
                }
            }
            _IsCheckedPacket = false;
            ToggleButton_OnChecked(null, null);
        }

        private void SendToDB_OnClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SendToDBEvent));
        }

		List<Kupon> Func(short[] parent, int current)
		{
			var res = new List<Kupon>();
			for (byte i = 0; i < 3; i++)
			{
				if (!Nazhor[i].Checked[current])
				{
					continue;
				}

				parent[i] |= (short)(1 << current);
				for (var j = 0; j < 3; j++)
				{
					if (i != j)
					{
						parent[j] &= (short)(short.MaxValue - (1 << current));
					}
				}

				if (current < 14)
				{
					res.AddRange(Func(parent, current + 1));
				}
				else
				{
					var kp = new Kupon
						{
							Cells = new short[3]
						};
					parent.CopyTo(kp.Cells, 0);
					res.Add(kp);
				}
			}
			return res;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void FirePropertyChanged(string propertyName)
		{
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
	}
}
