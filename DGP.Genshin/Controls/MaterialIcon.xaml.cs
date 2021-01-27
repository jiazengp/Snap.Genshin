﻿using DGP.Genshin.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DGP.Genshin.Controls
{
    /// <summary>
    /// MaterialIcon.xaml 的交互逻辑
    /// </summary>
    public partial class MaterialIcon : UserControl
    {
        public MaterialIcon()
        {
            DataContext = this;
            InitializeComponent();
        }
        public Material Material
        {
            get { return (Material)GetValue(MaterialProperty); }
            set { SetValue(MaterialProperty, value); }
        }
        public static readonly DependencyProperty MaterialProperty =
            DependencyProperty.Register("Material", typeof(Material), typeof(MaterialIcon), new PropertyMetadata(null));
    }
}