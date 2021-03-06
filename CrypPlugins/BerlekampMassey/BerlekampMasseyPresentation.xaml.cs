﻿/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
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
using System.Windows.Threading;
using System.Threading;

namespace Cryptool.BerlekampMassey
{
  /// <summary>
    /// Interaction logic for BerlekampMasseyPresentation.xaml
  /// </summary>
  [Cryptool.PluginBase.Attributes.Localization("Cryptool.BerlekampMassey.Properties.Resources")]
  public partial class BerlekampMasseyPresentation : UserControl
  {
      public BerlekampMasseyPresentation()
      {
          InitializeComponent();
          Height = double.NaN;
          Width = double.NaN;
      }

      public void setLength(string length)
      {
          Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
          {
              lText.Text = length;
          }, null);
      }

      public void setPolynomial(string polynommial)
      {
          Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
          {
              cdText.Text = polynommial;
          }, null);
      }
  }
}
