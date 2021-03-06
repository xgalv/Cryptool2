﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Cryptool.PluginBase.Attributes;



namespace SigabaBruteforce
{
    /// <summary>
    /// Interaction logic for SigabaBruteforceQuickWatchPresentation.xaml
    /// </summary>
    [global::Cryptool.PluginBase.Attributes.Localization("SigabaBruteforce.Properties.Resources")]
    public partial class SigabaBruteforceQuickWatchPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();
        public event EventHandler doppelClick;

        

        public SigabaBruteforceQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
        }
        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            if(sender!=null)
               doppelClick(sender,eventArgs);
        }
    }
}
