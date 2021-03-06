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
using System.Collections.ObjectModel;

namespace IDPAnalyser
{
    /// <summary>
    /// Interaktionslogik für IDPAnalyserQuickWatchPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("IDPAnalyser.Properties.Resources")]
    public partial class IDPAnalyserQuickWatchPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();
        public event EventHandler doppelClick;

        public IDPAnalyserQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
        }
        
        string entryToText(ResultEntry entry)
        {
            //string key = (entry.KeyArray.Length <= alphabet.Length)
            //    ? "Key (numeric): " + String.Join(" ", entry.KeyArray) + "\n" + "Key (alphabetic): " + entry.KeyPhrase
            //    : "Key: " + String.Join(" ", entry.KeyArray);

            return "Rank: " + entry.Ranking + "\n" +
                   "Value: " + entry.Value + "\n" +
                    "Key (numeric): " + String.Join(" ", entry.KeyArray) + "\n" +
                    "Key (alphabetic): " + entry.KeyPhrase + "\n" +
                   "Text: " + entry.Text;
        }

        public void ContextMenuHandler(Object sender, EventArgs eventArgs)
        {
            MenuItem menu = (MenuItem)((RoutedEventArgs)eventArgs).Source;
            ResultEntry entry = (ResultEntry)menu.CommandParameter;
            if (entry == null) return;

            if ((string)(menu.Tag) == "copy_text")
            {
                Clipboard.SetText(entry.Text);
            }
            else if ((string)(menu.Tag) == "copy_value")
            {
                Clipboard.SetText(entry.Value);
            }
            else if ((string)(menu.Tag) == "copy_key")
            {
                Clipboard.SetText(entry.Key);
            }
            else if ((string)(menu.Tag) == "copy_keyphrase")
            {
                Clipboard.SetText(entry.KeyPhrase);
            }
            else if ((string)(menu.Tag) == "copy_line")
            {
                Clipboard.SetText(entryToText(entry));
            }
            else if ((string)(menu.Tag) == "copy_all")
            {
                List<string> lines = new List<string>();
                foreach (var e in entries) lines.Add(entryToText(e));
                Clipboard.SetText(String.Join("\n\n",lines));
            }
        }

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            doppelClick(sender, eventArgs);
        }
    }
}
