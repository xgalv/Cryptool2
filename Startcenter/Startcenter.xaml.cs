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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Editor;
using StartCenter;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Startcenter.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class Startcenter : UserControl
    {
        private Panels _panelsObj;

        public string TemplatesDir
        {
            set 
            {
                ((Panels)panels.Children[0]).TemplatesDir = value;
            }
        }

        public event OpenEditorHandler OnOpenEditor;
        public event OpenTabHandler OnOpenTab;
        public event StartcenterEditor.StartupBehaviourChangedHandler StartupBehaviourChanged;
        public event EventHandler<TemplateOpenEventArgs> TemplateLoaded;

        public Startcenter()
        {
            InitializeComponent();
            ((Buttons)buttons.Content).OnOpenEditor += (content, info) => OnOpenEditor(content, info);
            _panelsObj = ((Panels)panels.Children[0]);
            _panelsObj.OnOpenEditor += (content, info) => OnOpenEditor(content, info);
            _panelsObj.OnOpenTab += (content, info, parent) => OnOpenTab(content, info, parent);
            _panelsObj.TemplateLoaded += new EventHandler<TemplateOpenEventArgs>(templateLoaded);
        }

        void templateLoaded(object sender, TemplateOpenEventArgs e)
        {
            if (TemplateLoaded != null)
            {
                TemplateLoaded.Invoke(sender, e);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (StartupBehaviourChanged != null && StartupCheckbox.IsChecked.HasValue)
                StartupBehaviourChanged(StartupCheckbox.IsChecked.Value);
        }

        public void ShowHelp()
        {
            _panelsObj.ShowHelp();
        }
    }
}
