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
using System.Windows.Threading;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.BinVisual
{
    /// <summary>
    /// Interaction logic for BinDataVisual.xaml
    /// </summary>
    public partial class BinDataVisual : UserControl, INotifyPropertyChanged
    {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Fields
        private DispatcherTimer timer = new DispatcherTimer(){ Interval = new TimeSpan(0, 0, 2) };
        #endregion

        #region Properties
        private IPluginInformation lastActiveConnector;
        public IPluginInformation LastActiveConnector { get { return lastActiveConnector; } } 
        #endregion

        #region DependencyProperties
        public static readonly DependencyProperty ConnectorCollectionProperty = DependencyProperty.Register("ConnectorCollection",
            typeof(ObservableCollection<IPluginInformation>), typeof(BinDataVisual), new FrameworkPropertyMetadata(null, null));

        public ObservableCollection<IPluginInformation> ConnectorCollection
        {
            get { return (ObservableCollection<IPluginInformation>)base.GetValue(ConnectorCollectionProperty); }
            set { base.SetValue(ConnectorCollectionProperty, value); }
        }

        public static readonly DependencyProperty ActiveConnectorProperty = DependencyProperty.Register("ActiveConnector",
            typeof(IPluginInformation), typeof(BinDataVisual), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnActiveConnectorChanged)));

        public IPluginInformation ActiveConnector
        {
            get { return (IPluginInformation)base.GetValue(ActiveConnectorProperty); }
            set { base.SetValue(ActiveConnectorProperty, value); }
        }
        #endregion

        #region Constructors
        public BinDataVisual(ObservableCollection<BinConnectorVisual> e)
        {
            ConnectorCollection = new ObservableCollection<IPluginInformation>();
            timer.Tick += new EventHandler(TickHandler);
            e.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(CollectionChangedHandler);
            InitializeComponent();
        }
        #endregion

        #region protected
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            if (oldParent != null)
                ActiveConnector = null;
            else
                ActiveConnector = lastActiveConnector;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Handler

        private void CollectionChangedHandler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
                ConnectorCollection.Add(new IPluginInformation(((BinConnectorVisual)e.NewItems[0]).Model));
        }

        private void TickHandler(object sender, EventArgs e)
        {
            if (ActiveConnector != null)
                ActiveConnector.Update();
        }

        private static void OnActiveConnectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BinDataVisual b = (BinDataVisual)d;
            IPluginInformation newInfo = (IPluginInformation)e.NewValue;
            IPluginInformation oldInfo = (IPluginInformation)e.OldValue;
            b.lastActiveConnector = oldInfo;
            if (newInfo == null)
            {
                b.timer.Stop();
            }
            else
            {
                newInfo.Update();
                b.timer.Start();
            }
        }
        #endregion
    }

    #region Custom Class
    public class IPluginInformation : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Fields
        private ConnectorModel model; 
        #endregion

        #region Properties
        public string Data 
        { 
            get 
            { 
                if(model.Data == null) 
                    return "No Data";

                if (model.Data is Byte[])
                {
                    StringBuilder builder = new StringBuilder();
                    Byte[] b = (Byte[])model.Data;
                    foreach (var e in b)
                        builder.Append(e);
                    return builder.ToString();
                }

                return model.Data.ToString();
            } 
        }
        public string ConnectorName { get; private set; }
        public string TypeName { get; private set; }
        public bool IsOutgoing { get; private set; }
        public bool IsMandatory { get; private set; }
        public SolidColorBrush Color { get; private set; }
        #endregion

        #region Contructors
        public IPluginInformation(ConnectorModel model)
        {
            this.model = model;
            Color = new SolidColorBrush(ColorHelper.GetLineColor(model.ConnectorType));
            ConnectorName = model.GetName();
            IsOutgoing = model.Outgoing;
            IsMandatory = model.IsMandatory;
            TypeName = model.ConnectorType != null ? model.ConnectorType.Name : "Class Not Found";
        } 
        #endregion

        #region protected
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region public
        public void Update()
        {
            OnPropertyChanged("Data");
        } 
        #endregion
    }
    #endregion
}