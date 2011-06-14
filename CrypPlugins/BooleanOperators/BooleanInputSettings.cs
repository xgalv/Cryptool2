﻿/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using Cryptool.PluginBase;
using System.IO;
using System.ComponentModel;
using Cryptool.PluginBase.Cryptography;

namespace Cryptool.Plugins.BooleanOperators

{
    class BooleanInputSettings : ISettings
    {

        #region ISettings Members

        private bool hasChanges = false;
        private int bool_value = 0; //0 false; 1 true

        [ContextMenu("BI_ValueCaption", "BI_ValueTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 0, 1 }, "BI_ValueList1", "BI_ValueList2")]
        [TaskPane("BI_ValueCaption", "BI_ValueTooltip", null, 1, false, ControlType.ComboBox, new string[] { "BI_ValueList1", "BI_ValueList2" })]
        public int Value
        {
            get { return this.bool_value; }
            set
            {
                if ((value) != bool_value) hasChanges = true;
                this.bool_value = value;
                OnPropertyChanged("Value");
            }
        }

        public bool HasChanges
        {
            get
            {
                return hasChanges;
            }
            set
            {
                hasChanges = value;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
    }
}
