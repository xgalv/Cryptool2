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
using System.ComponentModel;
using System.Linq;
using System.Text;
using Cryptool;
using Cryptool.PluginBase;


namespace Cryptool.Plugins.BooleanOperators
{
    class BooleanBinaryOperatorsSettings : ISettings
    {
        private int operatorType = 0;
        /* 0 = AND
         * 1 = OR
         * 2 = NAND
         * 3 = NOR
         * 4 = XOR
         */

        private bool hasChanges = false;

        #region ISettings Members
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

        [ContextMenu("Operator Type", "Operator Type", 0, DisplayLevel.Beginner, ContextMenuControlType.ComboBox, null, new string[] { "AND", "OR", "NAND", "NOR", "XOR" })]
        [TaskPane("Operator Type", "Operator Type", null, 2, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "AND", "OR", "NAND", "NOR", "XOR" })]
        public int OperatorType
        {
            get { return this.operatorType; }
            set
            {
                if (operatorType != value)
                    hasChanges = true;

                this.operatorType = value;
                OnPropertyChanged("OperatorType");
                ChangePluginIcon(value);                
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(int Icon)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, Icon));
        }
    }
}
