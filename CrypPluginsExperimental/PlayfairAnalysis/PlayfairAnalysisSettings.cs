﻿/* 
   Copyright 2010 Christoph Hartmann, Johannes Gutenberg-Universität Mainz

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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.Windows;

namespace Cryptool.Plugins.PlayfairAnalysis
{
    public class PlayfairAnalysisSettings : ISettings
    {
        #region Private Variables

        private enum supportedMatrixSizes {MatrixSize5x5, MatrixSize6x6}
        private int heapSize = 5000;
        private int matrixSize = (int)supportedMatrixSizes.MatrixSize5x5;
        private int language;
        private int useCustomStatistic;
        
        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane( "HeapSizeCaption", "HeapSizeTooltip", null, 1, false, ControlType.TextBox, ValidationType.RangeInteger, 1, Int32.MaxValue)]
        public int HeapSize
        {
            get
            {
                return heapSize;
            }
            set
            {                
                if (heapSize != value)
                {                    
                    heapSize = value;
                    OnPropertyChanged("HeapSize");
                }
            }
        }

        [TaskPane( "UseCustomStatisticCaption", "UseCustomStatisticTooltip", null, 2, false, ControlType.RadioButton, new string[] { "Default", "Custom" })]
        public int UseCustomStatistic
        {
            get
            {
                return useCustomStatistic;
            }
            set
            {
                if (useCustomStatistic != value)
                {
                    if (value == 0)
                    {
                        if (TaskPaneAttributeChanged != null)
                        {
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MatrixSize", Visibility.Visible)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Language", Visibility.Visible)));
                        }
                    }
                    else
                    {
                        if (TaskPaneAttributeChanged != null)
                        {
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MatrixSize", Visibility.Collapsed)));
                            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Language", Visibility.Collapsed)));
                        }
                    }

                    useCustomStatistic = value;
                    OnPropertyChanged("UseCustomStatistic");
                }
            }
        }

        [TaskPane( "MatrixSizeCaption", "MatrixSizeTooltip", null, 3, false, ControlType.RadioButton, new string[] { "5 x 5", "6 x 6" })]
        public int MatrixSize
        {
            get
            {
                return matrixSize;
            }
            set
            {
                if (matrixSize != value)
                {
                    matrixSize = value;
                    OnPropertyChanged("MatrixSize");                    
                }
            }
        }

        [TaskPane( "LanguageCaption", "LanguageTooltip", null, 4, false, ControlType.RadioButton, new string[] { "German", "English" })]
        public int Language
        {
            get
            {
                return language;
            }
            set
            {
                if (language != value)
                {
                    language = value;
                    OnPropertyChanged("Language");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
                
    }
}
