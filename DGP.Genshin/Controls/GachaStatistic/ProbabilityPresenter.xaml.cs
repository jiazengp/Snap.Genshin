using DGP.Genshin.Models.MiHoYo;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DGP.Genshin.Controls.GachaStatistic
{
    /// <summary>
    /// ProbabilityPresenter.xaml 的交互逻辑
    /// </summary>
    public partial class ProbabilityPresenter : UserControl, INotifyPropertyChanged
    {
        public ProbabilityPresenter()
        {
            DataContext = this;
            InitializeComponent();
        }

        public IEnumerable<GachaLogItem> ProbabilitySource
        {
            get { return (IEnumerable<GachaLogItem>)GetValue(ProbabilitySourceProperty); }
            set { SetValue(ProbabilitySourceProperty, value); }
        }
        public static readonly DependencyProperty ProbabilitySourceProperty =
            DependencyProperty.Register("ProbabilitySource", typeof(IEnumerable<GachaLogItem>), typeof(ProbabilityPresenter), new PropertyMetadata(null, OnProbabilitySourceChanged));
        
        private static void OnProbabilitySourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProbabilityPresenter current = (ProbabilityPresenter)d;

        }

        #region rank5
        private double rank5Probability;
        public double Rank5Probability { get => rank5Probability; set => Set(ref rank5Probability, value); }
        #endregion

        #region rank4
        private double rank4Probability;
        public double Rank4Probability { get => rank4Probability; set => Set(ref rank4Probability, value); }
        #endregion

        #region rank3
        private double rank3Probability;
        public double Rank3Probability { get => rank3Probability; set => Set(ref rank3Probability, value); }
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(ProbabilitySource.Count());
            Rank5Probability = (double)ProbabilitySource.Count(i => i.RankType == "5") / ProbabilitySource.Count();
            Rank4Probability = (double)ProbabilitySource.Count(i => i.RankType == "4") / ProbabilitySource.Count();
            Rank3Probability = (double)ProbabilitySource.Count(i => i.RankType == "3") / ProbabilitySource.Count();
        }
    }
}
