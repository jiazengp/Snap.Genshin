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
                current.Rank5Probability = (double)current.ProbabilitySource.Count(i => i.RankType == "5") / current.ProbabilitySource.Count();
                current.Rank4Probability = (double)current.ProbabilitySource.Count(i => i.RankType == "4") / current.ProbabilitySource.Count();
                current.Rank3Probability = (double)current.ProbabilitySource.Count(i => i.RankType == "3") / current.ProbabilitySource.Count();
            }
        }

        #region rank5
        private double rank5Probability;
        public double Rank5Probability { get => this.rank5Probability; set => this.Set(ref this.rank5Probability, value); }
        #endregion

        #region rank4
        private double rank4Probability;
        public double Rank4Probability { get => this.rank4Probability; set => this.Set(ref this.rank4Probability, value); }
        #endregion

        #region rank3
        private double rank3Probability;
        public double Rank3Probability { get => this.rank3Probability; set => this.Set(ref this.rank3Probability, value); }
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
