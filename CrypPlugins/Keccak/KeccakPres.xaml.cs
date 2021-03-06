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
using System.Threading;

namespace Cryptool.Plugins.Keccak
{
    /// <summary>
    /// Interaktionslogik für KeccakPres.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Keccak.Properties.Resources")]
    public partial class KeccakPres : UserControl
    {
        public AutoResetEvent buttonNextClickedEvent;
        public bool autostep, skipPermutation, skipStep, skipPresentation, skipIntro;
        public int autostepSpeed;

        public KeccakPres()
        {
            InitializeComponent();

            this.Width = 800;
            this.Height = 500;
            buttonNextClickedEvent = new AutoResetEvent(false);
            skipStep = false;
            autostep = false;
            skipPresentation = false;
            skipPermutation = false;
            skipIntro = false;
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            buttonNextClickedEvent.Set();
        }

        private void buttonSkipIntro_Click(object sender, RoutedEventArgs e)
        {
            buttonNextClickedEvent.Set();
            skipIntro = true;
        }

        private void buttonAutostep_Click(object sender, RoutedEventArgs e)
        {
            autostep = !autostep;
            if (autostep)
            {
                buttonNextClickedEvent.Set();
            }
        }

        private void buttonSkip_Click(object sender, RoutedEventArgs e)
        {
            skipStep = true;
            if (!autostep)
            {
                buttonNextClickedEvent.Set();
            }
        }

        private void buttonSkipPermutation_Click(object sender, RoutedEventArgs e)
        {
            skipPermutation = true;
            if (!autostep)
            {
                buttonNextClickedEvent.Set();
            }
        }

        private void autostepSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            autostepSpeed = 10 + 10 * (40 - (int)autostepSpeedSlider.Value);
        }

    }
}
