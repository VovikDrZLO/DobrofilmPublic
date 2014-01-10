using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.ComponentModel;


namespace Dobrofilm
{
    /// <summary>
    /// Interaction logic for PassWnd.xaml
    /// </summary>
    public partial class PassWnd : Window
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private readonly BackgroundWorker Multworker = new BackgroundWorker();
        public PassWnd()
        {            
            InitializeComponent();
            this.Title = "Enter View Pass";
            ActionType = ActionTypeEnum.ShowCryptedFiles;
            CloseWindow = true;
            Password_Box.Focus();
        }

        public PassWnd(string FromFile, string ToFile)
        {
            InitializeComponent();
            this.Title = "Enter Film Decription Pass";
            FromFilePath = FromFile;
            ToFilePath = ToFile;
            ActionType = ActionTypeEnum.DecryptFile;
            CloseWindow = true;
            Password_Box.Focus();
        }

        public PassWnd(List<FilmFile> ChekedFiles)
        {
            InitializeComponent();
            this.Title = "Enter Film Decription Pass";
            chekedFiles = ChekedFiles;
            ActionType = ActionTypeEnum.DecryptFileList;
            CloseWindow = false;
            Password_Box.Focus();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run all background tasks here            
            Utils.DecryptFile(FromFilePath, ToFilePath, PassStr);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work            
            Window LoadingWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Title == "wnd_Loading");
            if (LoadingWindow != null) LoadingWindow.Close();          
            if (CloseWindow) this.Close();
        }

        private void Multworker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run all background tasks here
            List<FilmFile> args = (List<FilmFile>)e.Argument;
            foreach (FilmFile Film in args)
            {
                string TempFileName = Utils.GenerateTempFilePath(Film.Path);
                Utils.DecryptFile(Film.Path, TempFileName, PassStr);
            }
           
        }

        private enum ActionTypeEnum {DecryptFile, ShowCryptedFiles, DecryptFileList};

        private string FromFilePath { get; set; }
        private string ToFilePath { get; set; }
        private ActionTypeEnum ActionType { get; set; }
        private string PassStr { get; set; }
        private List<FilmFile> chekedFiles { get; set; }
        private bool CloseWindow { get; set; }



        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            Multworker.DoWork += Multworker_DoWork;
            Multworker.RunWorkerCompleted += worker_RunWorkerCompleted;
            if (ActionType == ActionTypeEnum.DecryptFile)
            {
                PassStr = Password_Box.Password;                                
                Utils.ShowLoadingWindow("Decoding", FromFilePath);
                worker.RunWorkerAsync();
            }
            else if (ActionType == ActionTypeEnum.ShowCryptedFiles)
            {
                var Password = Password_Box.Password.GetHashCode();
                if (Password == -842352753)
                {                
                    Window MainWin = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Title == "Dobrofilm");
                    LinearGradientBrush myLinearGradientBrush =
                        new LinearGradientBrush();
                    myLinearGradientBrush.StartPoint = new Point(0,0);
                    myLinearGradientBrush.EndPoint = new Point(1,1);
                    myLinearGradientBrush.GradientStops.Add(
                        new GradientStop(Colors.Red, 0.0));                 
                    myLinearGradientBrush.GradientStops.Add(
                        new GradientStop(Colors.DarkRed, 1.0));                
                    MainWin.Background = myLinearGradientBrush;
                    var MenuBar = MainWin.FindName("MenuBar");
                    var MainCanvas = MainWin.FindName("MainCanvas");
                    FilmFilesList.ShowCryptFilms = true;                              
                    if (MenuBar is Menu)
                    {
                        Menu MainMenuBar = (Menu)MenuBar;
                        MainMenuBar.Background = myLinearGradientBrush;                    
                    }

                    if (MainCanvas is Canvas)
                    {
                        Canvas CanvasBar = (Canvas)MainCanvas;
                        CanvasBar.Background = myLinearGradientBrush;                    
                    }
                    Close();
                }
            }
            else if (ActionType == ActionTypeEnum.DecryptFileList)
            {
                PassStr = Password_Box.Password;
                Utils.ShowLoadingWindow("Decoding", "Selected Films");
                Multworker.RunWorkerAsync(chekedFiles);
                CloseWindow = true;                
            }
        }

        private void CancelBtn_Click_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Password_Box_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) OkBtn_Click(sender, e);
        }

        private void PswWnd_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }
    }
}
