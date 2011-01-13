/*
   Copyright 2008 Thomas Schmid, University of Siegen

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
using System.ComponentModel;

namespace FileOutput
{
  public class FileOutputSettings : ISettings
  {
    private string saveAndRestoreState;

    public string SaveAndRestoreState
    {
      get { return saveAndRestoreState; }
      set { saveAndRestoreState = value; }
    }

    private string targetFilename;
    [TaskPane("Target FileName", "File to write data into.", null, 1, false, ControlType.SaveFileDialog, "All Files (*.*)|*.*")]
    public string TargetFilename
    {
      get { return targetFilename; }
      set 
      { 
        targetFilename = value;
        HasChanges = true;
        OnPropertyChanged("TargetFilename");
      }
    }

    [TaskPane("Clear file name", "Forget the output file name", null, 2, false, ControlType.Button)]
    public void ClearFileName()
    {
        TargetFilename = null;
    }

    #region INotifyPropertyChanged Members

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(name));
      }
    }

    #endregion

    #region ISettings Members

    private bool hasChanges;
    public bool HasChanges
    {
      get { return hasChanges; }
      set { hasChanges = value; }
    }

    #endregion
  }
}
