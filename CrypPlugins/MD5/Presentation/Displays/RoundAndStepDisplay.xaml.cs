﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cryptool.MD5.Presentation.Displays
{
    /// <summary>
    /// Interaktionslogik für RoundAndStepDisplay.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Cryptool.Plugins.MD5.Properties.Resources")]
    public partial class RoundAndStepDisplay : UserControl
    {
        public RoundAndStepDisplay()
        {
            InitializeComponent();

            Width = double.NaN;
            Height = double.NaN;
        }
    }
}
