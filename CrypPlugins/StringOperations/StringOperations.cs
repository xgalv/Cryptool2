﻿/*                              
   Copyright 2011 Nils Kopal, Uni Duisburg-Essen

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
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using StringOperations.Properties;
using System.Collections.Generic;

namespace StringOperations
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("StringOperations.Properties.Resources", "PluginCaption", "PluginTooltip", "StringOperations/DetailedDescription/doc.xml", "StringOperations/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class StringOperations : ICrypComponent
    {
        private StringOperationsSettings _settings = null;

        private string _string1;
        private string _string2;
        private string _string3;
        private int _value1;
        private int _value2;
        private string _outputString;
        private int _outputValue;
        private string[] _outputStringArray;

        public StringOperations()
        {
            _settings = new StringOperationsSettings();            
        }

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { return _settings; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            _string1 = null;
            _string2 = null;
            _string3 = null;
            _value1 = int.MinValue;
            _value2 = int.MinValue;

        }

        public void Execute()
        {

            //If connector values are not set, maybe the user set these values in the settings
            //So we replace the connector values with the setting values:
            if (string.IsNullOrEmpty(_string1))
            {
                _string1 = _settings.String1;
            }
            if (string.IsNullOrEmpty(_string2))
            {
                _string2 = _settings.String2;
            }
            if (string.IsNullOrEmpty(_string3))
            {
                _string3 = _settings.String3;
            }
            if (_value1 == int.MaxValue)
            {
                _value1 = _settings.Value1;
            }
            if (_value2 == int.MaxValue)
            {
                _value2 = _settings.Value2;
            }

            try
            {
                switch (_settings.Operation)
                {
                    case StringOperationType.Concatenate:
                        _outputString = String.Concat(_string1,_string2);
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.Substring:                        
                        if (_string1.Length > 0 && _value1<0 && _value1>=-_string1.Length) _value1 = (_value1 + _string1.Length) % _string1.Length;
                        _outputString = _string1.Substring(_value1, _value2);
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.ToUppercase:
                        _outputString = _string1.ToUpper();
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.ToLowercase:
                        _outputString = _string1.ToLower();
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.Length:
                        _outputValue = _string1.Length;                        
                        OnPropertyChanged("OutputValue");
                        break;
                    case StringOperationType.CompareTo:
                        _outputValue = _string1.CompareTo(_string2);
                        OnPropertyChanged("OutputValue");
                        break;
                    case StringOperationType.Trim:
                        _outputString = _string1.Trim();
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.IndexOf:
                        _outputValue = _string1.IndexOf(_string2);
                        OnPropertyChanged("OutputValue");
                        break;
                    case StringOperationType.Equals:
                        _outputValue = (_string1.Equals(_string2) ? 1 : 0);                        
                        OnPropertyChanged("OutputValue");
                        break;
                    case StringOperationType.Replace:
                        _outputString = _string1.Replace(_string2, _string3);
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.RegexReplace:
                        _outputString = Regex.Replace(_string1, _string2, _string3);
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.Split:
                        if (_string2 != null && _string2.Length == 0)
                        {
                            var tmp = Regex.Split(_string1, string.Empty);
                            _outputStringArray = new string[tmp.Length - 2];
                            Array.Copy(tmp, 1, _outputStringArray, 0, _outputStringArray.Length);  // remove empty entries at beginning and end
                        }
                        else
                            _outputStringArray = _string1.Split((_string2 == null) ? "\r\n".ToCharArray() : _string2.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        OnPropertyChanged("OutputStringArray");
                        _outputValue = (_outputStringArray == null) ? 0 :_outputStringArray.Length;
                        OnPropertyChanged("OutputValue");
                        break;
                    case StringOperationType.Block:
                        if (_settings.Blocksize == 0)
                        {
                            GuiLogMessage("Blocksize is '0'. Set blocksize to '1'", NotificationLevel.Warning);
                            _settings.Blocksize = 1;                            
                        }
                        var counter = 0;
                        var str = Regex.Replace(_string1, @"\s*", "");
                        var strbuilder = new StringBuilder();
                        foreach (char c in str)
                        {
                            strbuilder.Append(c);
                            counter++;
                            if(counter == ((StringOperationsSettings)Settings).Blocksize)
                            {
                                counter = 0;
                                strbuilder.Append(" ");
                            }
                        }
                        _outputString = strbuilder.ToString().TrimEnd();
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.Reverse:
                        char[] arr = _string1.ToCharArray();
	                    Array.Reverse(arr);
                        _outputString = new string(arr);
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.Sort:
                        char[] sortarr = _string1.ToCharArray();
                        Array.Sort(sortarr);
                        if (_settings.Order == 1)
                        {
                            Array.Reverse(sortarr);
                        }
                        _outputString = new string(sortarr);
                        OnPropertyChanged("OutputString");
                        break;
                    case StringOperationType.Distinct:
                        StringBuilder builder = new StringBuilder();
                        HashSet<char> chars = new HashSet<char>();

                        foreach (char c in _string1)
                        {
                            if (!chars.Contains(c))
                            {
                                chars.Add(c);
                                builder.Append(c);
                            }
                        }
                        _outputString = builder.ToString();
                        OnPropertyChanged("OutputString");
                        break;
                }
                ProgressChanged(1, 1);
            }
            catch(Exception ex)
            {
                GuiLogMessage(String.Format(Resources.StringOperations_Execute_Could_not_execute_operation___0______1_, ((StringOperationType)(_settings).Operation), ex.Message), NotificationLevel.Error);
            }
        }

        public void PostExecution()
        {
            
        }

        public void Stop()
        {
            
        }

        public void Initialize()
        {
            _settings.UpdateTaskPaneVisibility();
        }

        public void Dispose()
        {
            
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        [PropertyInfo(Direction.InputData, "String1Caption", "String1Tooltip", false)]
        public string String1
        {
            get { return _string1; }
            set { _string1 = value; }
        }

        [PropertyInfo(Direction.InputData, "String2Caption", "String2Tooltip", false)]
        public string String2
        {
            get { return _string2; }
            set { _string2 = value; }
        }

        [PropertyInfo(Direction.InputData, "String3Caption", "String3Tooltip", false)]
        public string String3
        {
            get { return _string3; }
            set { _string3 = value; }
        }


        [PropertyInfo(Direction.InputData, "Value1Caption", "Value1Tooltip", false)]
        public int Value1
        {
            get { return _value1; }
            set { _value1 = value; }
        }

        [PropertyInfo(Direction.InputData, "Value2Caption", "Value2Tooltip", false)]
        public int Value2
        {
            get { return _value2; }
            set { _value2 = value; }
        }      

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get { return _outputString; }
        }

        [PropertyInfo(Direction.OutputData, "OutputValueCaption", "OutputValueTooltip", false)]
        public int OutputValue
        {
            get { return _outputValue; }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringArrayCaption", "OutputStringArrayTooltip", false)]
        public String[] OutputStringArray
        {
            get { return _outputStringArray; }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }
    }
}
