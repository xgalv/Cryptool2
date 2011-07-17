﻿/* HOWTO: Change year, author name and organization.
   Copyright 2010 Your Name, University of Duckburg

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
using Cryptool.PluginBase.Validation;
using System.IO;
using System.Collections;
using System.ComponentModel;

namespace Solitaire
{
    public class SolitaireSettings : ISettings
    {
        #region Private Variables

        private bool hasChanges = false;
        private int numberOfCards = 54;
        private int generationType = 0;
        private int streamType = 0;
        private int actionType = 0;



        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [PropertySaveOrder(0)]
        [TaskPane( "ActionTypeCaption", "ActionTypeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public int ActionType
        {
            get
            {
                return actionType;
            }
            set
            {
                if (actionType != value)
                {
                    actionType = value;
                    hasChanges = true;
                }
            }
        }

        [PropertySaveOrder(1)]
        [TaskPane( "NumberOfCardsCaption", "NumberOfCardsTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 3, 54)]
        public int NumberOfCards
        {
            get
            {
                return numberOfCards;
            }
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (numberOfCards != value)
                {
                    numberOfCards = value;
                    hasChanges = true;
                }
            }
        }

        [PropertySaveOrder(2)]
        [TaskPane( "GenerationTypeCaption", "GenerationTypeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Ascending", "Descending", "Given State", "Password", "Random" })]
        public int GenerationType
        {
            get
            {
                return generationType;
            }
            set
            {
                if (generationType != value) 
                {
                    generationType = value;
                    hasChanges = true;
                }
            }
        }

        [PropertySaveOrder(3)]
        [TaskPane( "StreamTypeCaption", "StreamTypeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Automatic", "Manual" })]
        public int StreamType
        {
            get
            {
                return streamType;
            }
            set
            {
                if (streamType != value)
                {
                    streamType = value;
                    hasChanges = true;
                }
            }
        }

        #endregion

        #region ISettings Members

        /// <summary>
        /// HOWTO: This flags indicates whether some setting has been changed since the last save.
        /// If a property was changed, this becomes true, hence CrypTool will ask automatically if you want to save your changes.
        /// </summary>
        [PropertySaveOrder(4)]
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

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}