﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Cryptool.P2PEditor.Converters;
using Cryptool.P2PEditor.Distributed;
using Cryptool.P2PEditor.Worker;
using Cryptool.PluginBase;
using Timer = System.Timers.Timer;
using Cryptool.PluginBase.Attributes;

namespace Cryptool.P2PEditor.GUI.Controls
{
    /// <summary>
    /// Interaction logic for JobDisplay.xaml
    /// </summary>
    [Localization("Cryptool.P2PEditor.Properties.Resources")]
    public partial class JobDisplay : INotifyPropertyChanged
    {
        public static readonly DependencyProperty JobsProperty = DependencyProperty.Register("Jobs",
                                                                                    typeof (List<DistributedJob>),
                                                                                    typeof (JobDisplay));
        public ICollection<DistributedJob> Jobs
        {
            get { return (List<DistributedJob>)GetValue(JobsProperty); }
            set { SetValue(JobsProperty, value); }
        }

        private Timer refreshListTimer;
        private readonly DispatcherTimer updateJobDetailsTimer;
        private JobListDetailsUpdateWorker updateTask;

        private bool participating;
        public bool Participating
        {
            get { return participating; }
            set
            {
                participating = value;
                OnPropertyChanged("Participating");
            }
        }

        public JobDisplay()
        {
            InitializeComponent();
          

            updateJobDetailsTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(5)};
            updateJobDetailsTimer.Tick += UpdateJobDetailsTimerElapsed;
            updateJobDetailsTimer.Start();

            AttachLoadingAdorner();
        }

        private void AttachLoadingAdorner()
        {
            var participateAdorner = new ParticipateAdorner(mainPane);
            var bind = new Binding("Participating");
            bind.Source = this;
            bind.Converter = new TrueToVisibleOrCollapsedConverter();
            participateAdorner.SetBinding(VisibilityProperty, bind);
            AdornerLayer.GetAdornerLayer(mainPane).Add(participateAdorner);
        }

        void UpdateJobDetailsTimerElapsed(object sender, EventArgs eventArgs)
        {
           if (updateTask != null) return;

            updateTask = new JobListDetailsUpdateWorker(Jobs, JobListManager);
            updateTask.RunWorkerCompleted += UpdateTaskRunWorkerCompleted;
            updateTask.RunWorkerAsync();
            P2PEditor.GuiLogMessage(Properties.Resources.Running_update_task_, NotificationLevel.Debug);
        }

        void UpdateTaskRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            updateTask = null;
        }

        void P2PManager_OnP2PConnectionStateChangeOccurred(object sender, bool newState)
        {
            UpdateRefreshTimerSettings(newState);
        }

        private void UpdateRefreshTimerSettings(bool isConnected)
        {
        }

        void RefreshListTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!IsVisible) return;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateJobList));
        }

        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateJobList();
        }

        public void UpdateJobList()
        {
      
            P2PEditor.GuiLogMessage(Properties.Resources.Requesting_new_job_list___, NotificationLevel.Debug);
            var updateWorker = new JobListUpdateWorker(JobListManager);
            updateWorker.RunWorkerCompleted += HandleRefreshedJobList;
            updateWorker.RunWorkerAsync();
        }

        void HandleRefreshedJobList(object sender, RunWorkerCompletedEventArgs e)
        {
            var updateWorker = sender as JobListUpdateWorker;
            if (updateWorker == null)
            {
                return;
            }

            P2PEditor.GuiLogMessage(Properties.Resources.Received_new_job_list___, NotificationLevel.Debug);
            Jobs = updateWorker.RefreshedJobList;

            UpdateJobDetailsTimerElapsed(null, null);
        }

        private void ParticipateButtonClick(object sender, RoutedEventArgs e)
        {
            ParticipateInSelectedJob();
        }

        private void ParticipateItemHandler(object sender, MouseButtonEventArgs e)
        {
            ParticipateInSelectedJob();
        }

        private void ParticipateInSelectedJob()
        {
            var jobToParticipateIn = (DistributedJob) JobListBox.SelectedItem;
            if (jobToParticipateIn == null) return;

            Participating = true;

            P2PEditor.GuiLogMessage(
                string.Format(Properties.Resources.Preparing_to_participate_in_job__0____1___, jobToParticipateIn.Name,
                              jobToParticipateIn.Guid),
                NotificationLevel.Info);
            var jobParticipationWorker = new JobParticipationWorker(P2PEditor, JobListManager, jobToParticipateIn, Dispatcher);
            jobParticipationWorker.RunWorkerCompleted +=JobParticipationWorkerRunWorkerCompleted;
            jobParticipationWorker.RunWorkerAsync();
        }

        private void JobParticipationWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Participating = false;
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            DeleteSelectedJob();
        }

        private void JobListBoxKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ParticipateInSelectedJob();
                    break;
                case Key.Delete:
                    DeleteSelectedJob();
                    break;
            }
            
        }

        private void DeleteSelectedJob()
        {
            var jobToDelete = (DistributedJob)JobListBox.SelectedItem;


            P2PEditor.GuiLogMessage(
                string.Format(Properties.Resources.Deleting_job__0____1___, jobToDelete.Name, jobToDelete.Guid),
                NotificationLevel.Info);

            var backgroundCreationWorker = new JobDeletionWorker(JobListManager, jobToDelete);
            backgroundCreationWorker.RunWorkerCompleted += BackgroundDeletionWorkerCompleted;
            backgroundCreationWorker.RunWorkerAsync();
        }

        private void BackgroundDeletionWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            P2PEditorPresentation.ShowActiveJobsView();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            this.P2PEditorPresentation.ShowJobCreationView();
        }
    }
}