﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace TemplateEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<TemplateInfo> _templates = new ObservableCollection<TemplateInfo>();
        private string _templateDir;

        public static readonly DependencyProperty IsOverviewProperty = DependencyProperty.Register("IsOverview",
                                                                                                   typeof (Boolean),
                                                                                                   typeof (MainWindow),
                                                                                                   new PropertyMetadata(
                                                                                                       true));

        public bool IsOverview
        {
            get { return (bool) GetValue(IsOverviewProperty); }
            set { SetValue(IsOverviewProperty, value); }
        }

        public MainWindow()
        {
            InitializeComponent();

            var templateFolderDialog = new FolderBrowserDialog();
            templateFolderDialog.Description = "Please select your template directory.";
            templateFolderDialog.SelectedPath = Directory.GetCurrentDirectory();
            if (templateFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _templateDir = templateFolderDialog.SelectedPath;
                LoadTemplates(".");
                AllTemplatesList.DataContext = _templates;
                AllTemplatesList2.DataContext = _templates;
            }
            else
            {
                Close();
            }
        }

        private Dictionary<string, List<string>> GetAllKeywords()
        {
            var result = new Dictionary<string, List<string>>();
            foreach (var template in _templates)
            {
                if (template.LocalizedTemplateData != null)
                {
                    foreach (var locTemp in template.LocalizedTemplateData)
                    {
                        if (locTemp.Value.Keywords != null)
                        {
                            if (!result.ContainsKey(locTemp.Key))
                            {
                                result.Add(locTemp.Key, new List<string>());
                            }
                            foreach (var keyword in locTemp.Value.Keywords)
                            {
                                result[locTemp.Key].Add(keyword);
                            }
                        }
                    }
                }
            }
            foreach (var res in result)
            {
                res.Value.Sort();
            }
            return result;
        }

        private void LoadTemplates(string dir)
        {
            var dirPath = Path.Combine(_templateDir, dir);

            foreach (var file in Directory.GetFiles(dirPath))
            {
                if (file.ToLower().EndsWith("cwm"))
                {
                    _templates.Add(new TemplateInfo(_templateDir, Path.Combine(dir, Path.GetFileName(file))));
                }
            }

            foreach (var subdir in Directory.GetDirectories(dirPath))
            {
                var subd = new DirectoryInfo(subdir);
                LoadTemplates(Path.Combine(dir, subd.Name));
            }
        }

        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            IsOverview = !IsOverview;
        }

        private void AllKeywordsButton_Click(object sender, RoutedEventArgs e)
        {
            var akw = new AllKeywordsWindow();
            akw.ShowAllKeywords(GetAllKeywords());
        }
    }
}