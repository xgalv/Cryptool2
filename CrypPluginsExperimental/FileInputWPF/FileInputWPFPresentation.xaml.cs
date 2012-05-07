﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.ComponentModel;

namespace FileInputWPF
{
    /// <summary>
    /// Interaction logic for FileInputWPFPresentation.xaml
    /// </summary>
    public partial class FileInputWPFPresentation : UserControl
    {
        public HexBox.HexBox hexBox;

        private Cryptool.Plugins.FileInputWPF.FileInputWPF exp;

        public FileInputWPFPresentation(HexBox.HexBox hexbox,Cryptool.Plugins.FileInputWPF.FileInputWPF exp )
        {
            InitializeComponent();

            

            this.exp = exp;
            

            SizeChanged += sizeChanged;
            this.hexBox = hexbox;
            this.hexBox.OnFileChanged += fileChanged; 
            MainMain.Children.Add(hexbox);

        }

        


        public void CloseFileToGetFileStreamForExecution()
        {
            
            hexBox.saveData(true,false);

            hexBox.closeFile(false);
            hexBox.openFile((exp.Settings as Cryptool.Plugins.FileInputWPF.ExamplePluginCT2Settings).OpenFilename, true);

            hexBox.IsEnabled = false;
            //hexBox.openFile(exp.settings.OpenFilename);
        }

        public void ReopenClosedFile()
        {
            //closedForExecution = false;

            if (File.Exists((exp.Settings as Cryptool.Plugins.FileInputWPF.ExamplePluginCT2Settings).OpenFilename) )
            {
                // tbFileClosedWhileRunning.Visibility = Visibility.Collapsed;
                // windowsFormsHost.Visibility = Visibility.Visible;
                hexBox.closeFile(false);
                hexBox.openFile(
                        (exp.Settings as Cryptool.Plugins.FileInputWPF.ExamplePluginCT2Settings).OpenFilename, false);
               
            }
            hexBox.IsEnabled = true;
        }


        internal void OpenFile(String fileName)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate {
                                                                                                hexBox.openFile(fileName,false);
            }, null);
        }

        internal void dispose()
        {
            hexBox.dispose();
        }

        private void fileChanged(Object sender, EventArgs eventArgs)
        {
            exp.settings.OpenFilename = hexBox.Pfad;
        }

        private void sizeChanged(Object sender, EventArgs eventArgs)
        {
            if (this.ActualWidth / 2.25 < this.ActualHeight)
                this.MainMain.RenderTransform = new ScaleTransform(this.ActualWidth / this.MainMain.ActualWidth, this.ActualWidth / this.MainMain.ActualWidth);
            else
                this.MainMain.RenderTransform = new ScaleTransform(this.ActualHeight / this.MainMain.ActualHeight, this.ActualHeight / this.MainMain.ActualHeight);
        }
        
        internal void CloseFile()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                hexBox.closeFile(true);
            }, null);
        }
    }
}