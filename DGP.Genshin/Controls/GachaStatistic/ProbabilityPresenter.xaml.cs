using DGP.Genshin.Models.MiHoYo;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls.GachaStatistic
{
    /// <summary>
    /// ProbabilityPresenter.xaml 的交互逻辑
    /// </summary>
    public partial class ProbabilityPresenter : UserControl, INotifyPropertyChanged
    {
        public ProbabilityPresenter()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        public IEnumerable<GachaLogItem> ProbabilitySource
        {
            get => (IEnumerable<GachaLogItem>)this.GetValue(ProbabilitySourceProperty);
            set => this.SetValue(ProbabilitySourceProperty, value);
        }
        public static readonly DependencyProperty ProbabilitySourceProperty =
            DependencyProperty.Register("ProbabilitySource", typeof(IEnumerable<GachaLogItem>), typeof(ProbabilityPresenter), new PropertyMetadata(null, OnProbabilitySourceChanged));

        private static void OnProbabilitySourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProbabilityPresenter current = (ProbabilityPresenter)d;
            if (current.ProbabilitySource != null)
            {
                IEnumerable<GachaLogItem> items = current.ProbabilitySource;
                current.Rank5BaseProb = (double)items.Count(i => i.Rank == "5") / items.Count();
                current.Rank4BaseProb = (double)items.Count(i => i.Rank == "4") / items.Count();
                current.Rank3BaseProb = (double)items.Count(i => i.Rank == "3") / items.Count();

                IEnumerable<GachaLogItem> compRank5 = items.SkipWhile(i => i.Rank == "3" || i.Rank == "4");
                IEnumerable<GachaLogItem> compRank4 = items.SkipWhile(i => i.Rank == "3" || i.Rank == "5");
                current.Rank5CompProb = (double)compRank5.Count(i => i.Rank == "5") / compRank5.Count();
                current.Rank4CompProb = (double)compRank4.Count(i => i.Rank == "4") / compRank4.Count();

                current.TotalCount = items.Count();
                current.LastRank5 = items.TakeWhile(i => i.Rank == "3" || i.Rank == "4").Count();
                current.LastRank4 = items.TakeWhile(i => i.Rank == "3" || i.Rank == "5").Count();
            }
        }

        #region total
        private int totalCount;
        public int TotalCount { get => this.totalCount; set => this.Set(ref this.totalCount, value); }
        #endregion

        #region rank5
        private double rank5BaseProb;
        public double Rank5BaseProb { get => this.rank5BaseProb; set => this.Set(ref this.rank5BaseProb, value); }

        private double rank5CompProb;
        public double Rank5CompProb { get => this.rank5CompProb; set => this.Set(ref this.rank5CompProb, value); }

        private int lastRank5;
        public int LastRank5 { get => this.lastRank5; set => this.Set(ref this.lastRank5, value); }
        #endregion

        #region rank4
        private double rank4BaseProb;
        public double Rank4BaseProb { get => this.rank4BaseProb; set => this.Set(ref this.rank4BaseProb, value); }

        private double rank4CompProb;
        public double Rank4CompProb { get => this.rank4CompProb; set => this.Set(ref this.rank4CompProb, value); }

        private int lastRank4;
        public int LastRank4 { get => this.lastRank4; set => this.Set(ref this.lastRank4, value); }
        #endregion

        #region rank3
        private double rank3BaseProb;
        public double Rank3BaseProb { get => this.rank3BaseProb; set => this.Set(ref this.rank3BaseProb, value); }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
