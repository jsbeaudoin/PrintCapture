using System.Collections.Generic;
using System.Windows.Media;
using PrintsCapture.Prints.Extension;
using PrintsCapture.Prints.Language;

namespace PrintsCapture.Livescan.View
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows;
    using System.Windows.Forms;
    using ViewModel;

    /// <summary>
    /// Interaction logic for QuickFlatProcessWindow.xaml
    /// </summary>
    public partial class WizardCaptureWindow
    {
        private PictureBox previewBox;

        public WizardProcessViewModel ViewModel { get; private set; }

        public WizardCaptureWindow(WizardProcessViewModel viewModel)
        {
            this.ViewModel = viewModel;
            
            this.InitializeComponent();
            //this.ViewModel.SetControl(); //this.HandQualityControl);
            this.ViewModel.DisplayImage = this.DisplayImage;
            this.previewBox = new PictureBox { BackColor = Color.Gray };

            this.Win32Window.Child = this.previewBox;
            this.previewBox.Location = new System.Drawing.Point(0, 0);
            this.previewBox.BackColor = Color.LightGray;

            
            //this.HandQualityControl.Initialize(viewModel.Endorsement.LinkedPrint.PrintList);

            this.Loaded += (sender, args) => OnWindowLoaded();

            this.ViewModel.CloseWindowRequired += (sender, args) => this.Close();
            this.ViewModel.HideWindow += (sender, args) => this.WindowState = WindowState.Normal;
            this.ViewModel.ShowWindow += (sender, args) => this.WindowState = WindowState.Maximized;
            this.ViewModel.ConfigurationCompleted += (o, args) => this.StartCapture();

            this.ViewModel.InstructionHandImage = null;

            //Setting window apparence for login
            if (this.ViewModel.IsLoginMode)
            {
                //this.WindowState = WindowState.Normal;
            }

        }

        void OnWindowLoaded()
        {
            var bgw = new BackgroundWorker();
            bgw.DoWork += (o, eventArgs) =>
            {
                Thread.Sleep(1000);
                if (this.previewBox.IsHandleCreated)
                {
                    this.StartCapture();
                }
                else
                {
                    this.previewBox.HandleCreated += (s, a) => this.StartCapture();
                }
            };

            bgw.RunWorkerAsync();
        }

        void StartCapture()
        {
            var actions = new Action(
                () =>
                {
                    //this.ViewModel.PreviewHandle = this.previewBox.Handle.ToInt32();
                    this.ViewModel.TriggerCaptureStart();
                    this.ViewModel.PrintCaptureVisibility = Visibility.Hidden;
                });            

            var disp = System.Windows.Application.Current.Dispatcher;

            if (disp.CheckAccess())
            {
                actions.Invoke();
            }
            else
            {
                disp.Invoke(actions);
            }

        }

        

        private void ResumeButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.TriggerResumeCapture();        
        
        }

        //private void AcceptButtonClick(object sender, RoutedEventArgs e)
        //{
        //    this.ViewModel.AcceptPrint = true;
        //    this.ViewModel.TriggerCloseWindowRequired();
        //}

        private void EndButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.AcceptPrint = false;            
            this.ViewModel.TriggerCloseWindowRequired();
        }

        private void FingerStatusButtonClicked(object sender, RoutedEventArgs e)
        {

        }

        private void VersionLabelDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.MessageListBox.Visibility = MessageListBox.Visibility == Visibility.Collapsed
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ConfigureMenuClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.TriggerConfigureDevice();            
        }

        
    }
}
