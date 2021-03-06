﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
using KeySearcherPresentation.Controls;

namespace KeySearcherPresentation
{
    /// <summary>
    /// Interaction logic for QuickWatch.xaml
    /// </summary>
    [TabColor("pink")]
    [Cryptool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class QuickWatch : UserControl
    {
        public static readonly DependencyProperty IsP2PEnabledProperty =
            DependencyProperty.Register("IsP2PEnabled",
                                        typeof(
                                            Boolean),
                                        typeof(
                                            QuickWatch), new PropertyMetadata(false));

        public Boolean IsP2PEnabled
        {
            get { return (Boolean)GetValue(IsP2PEnabledProperty); }
            set { SetValue(IsP2PEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsOpenCLEnabledProperty =
            DependencyProperty.Register("IsOpenCLEnabled",
                                typeof(
                                    Boolean),
                                typeof(
                                    QuickWatch), new PropertyMetadata(false));

        public Boolean IsOpenCLEnabled
        {
            get { return (Boolean)GetValue(IsOpenCLEnabledProperty); }
            set
            {
                SetValue(IsOpenCLEnabledProperty, value);
                P2PQuickWatchPresentation.IsOpenCLEnabled = value;
                LocalQuickWatchPresentation.IsOpenCLEnabled = value;
            }
        }

        public OpenCLPresentation OpenCLPresentation
        {
            get
            {
                if (IsP2PEnabled)
                    return P2PQuickWatchPresentation.OpenCLPresentation;
                else
                    return LocalQuickWatchPresentation.OpenCLPresentation;
            }
        }

        public Boolean ShowStatistics
        {
            get { return (Boolean)GetValue(ShowStatisticsProperty); }
            set { SetValue(ShowStatisticsProperty, value); }
        }

        public static DependencyProperty ShowStatisticsProperty =
            DependencyProperty.Register("ShowStatistics",
                                        typeof(
                                            Boolean),
                                        typeof(
                                            QuickWatch), new PropertyMetadata(false));

        public CultureInfo CurrentCulture { get; private set; }

        public QuickWatch()
        {
            InitializeComponent();

            CurrentCulture = CultureInfo.CurrentCulture;
        }
    }
}
