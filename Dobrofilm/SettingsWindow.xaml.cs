using System;
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
using System.Windows.Shapes;
using System.IO;

namespace Dobrofilm
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            CategotyPathTB.Text = Dobrofilm.Properties.Settings.Default.CategoryListXMLFile;
            FilmPathTB.Text = Dobrofilm.Properties.Settings.Default.FilmListXMLFile;
            LinkPath.Text = Dobrofilm.Properties.Settings.Default.LinksListXMLFile;
            MPCRath.Text = Dobrofilm.Properties.Settings.Default.MPCPath;
            DefBrowserTB.Text = Dobrofilm.Properties.Settings.Default.DefaultBrowser;
            ScreenPath.Text = Dobrofilm.Properties.Settings.Default.ScreenShotXMLFile;
            CategoryList categoryList = new CategoryList();
            CategoryNextIDTB.Text = categoryList.GetCategoryNextID();
            FilmFilesList filmFilesList = new FilmFilesList();
            FilmMaskTB.Text = filmFilesList.GetFilmMask();
            FilmNextIDTB.Text = filmFilesList.GetFileMaskNextID();
        }

        private void DelMarkedFilmsBtn_Click(object sender, RoutedEventArgs e)
        {
            bool DelogResult  = Utils.ShowYesNoDialog("DeleteFromDisk?");
            FilmFilesList filmFilesList = new FilmFilesList();
            ListCollectionView FilteredFilmCollection = filmFilesList.FilmFiles;
            FilteredFilmCollection.Filter = new Predicate<object>(MarkedForDelete);
            if (!Utils.ShowYesNoDialog(string.Format("{0} records will be deleted proceed?", FilteredFilmCollection.Count))) return;
            
            foreach (FilmFile filmFile in FilteredFilmCollection)
            {
                if (!!DelogResult && !filmFile.IsOnline)
                {
                    try
                    {
                        Utils.DeleteFile(filmFile.Path);                        
                    }
                    catch (IOException err)
                    {
                        Utils.ShowWarningDialog(err.Message);                        
                    }                    
                }
                filmFilesList.DeleteFilmItemFromXml(filmFile);
            }
        }

        public bool MarkedForDelete(object de)
        {
            FilmFile order = de as FilmFile;
            return order.Rate == -1;
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            int NewCategoryID;
            int NewFilmMaskID;
            if (!Utils.IsFileExists(CategotyPathTB.Text))
            {
                Utils.ShowWarningDialog(string.Format("Category Dictionary file {0} does not exists", CategotyPathTB.Text));             
            }
            else if(!Utils.IsFileExists(MPCRath.Text))
            {
                Utils.ShowWarningDialog(string.Format("Media Player Classic was not found by this path {0}", MPCRath.Text));             
            }
            else if (!Utils.IsFileExists(FilmPathTB.Text))
            {
                Utils.ShowWarningDialog(string.Format("Film list Dictionary file {0} does not exists", FilmPathTB.Text));
            }
            else if (!Utils.IsFileExists(LinkPath.Text))
            {
                Utils.ShowWarningDialog(string.Format("Links Dictionary file {0} does not exists", LinkPath.Text));
            }
            else if (!Utils.IsFileExists(ScreenPath.Text))
            {
                Utils.ShowWarningDialog(string.Format("ScreenShot  Dictionary file {0} does not exists", ScreenPath.Text));
            }
            else if (!int.TryParse(CategoryNextIDTB.Text, out NewCategoryID) && CategoryNextIDTB.Text == string.Empty)
            {
                Utils.ShowWarningDialog(string.Format("{0} is not valid ID", CategoryNextIDTB.Text));
            }
            else if (!int.TryParse(FilmNextIDTB.Text, out NewFilmMaskID) && FilmNextIDTB.Text == string.Empty)
            {
                Utils.ShowWarningDialog(string.Format("{0} is not valid increment", FilmNextIDTB.Text));
            }
            else
            {
                Dobrofilm.Properties.Settings.Default.CategoryListXMLFile = CategotyPathTB.Text;
                Dobrofilm.Properties.Settings.Default.FilmListXMLFile = FilmPathTB.Text;
                Dobrofilm.Properties.Settings.Default.MPCPath = MPCRath.Text;
                Dobrofilm.Properties.Settings.Default.LinksListXMLFile = LinkPath.Text;
                Dobrofilm.Properties.Settings.Default.DefaultBrowser = DefBrowserTB.Text;
                Dobrofilm.Properties.Settings.Default.ScreenShotXMLFile = ScreenPath.Text;
                Dobrofilm.Properties.Settings.Default.Save();
                CategoryList categoryList = new CategoryList();
                categoryList.SetCategoryID(NewCategoryID);
                FilmFilesList filmFilesList = new FilmFilesList();
                filmFilesList.SetFilmMaskAndCounter(FilmMaskTB.Text, NewFilmMaskID); 
                Close();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SettingsWindow1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        private void SecretButton_Click(object sender, RoutedEventArgs e)
        {
            if (!!FilmFilesList.ShowCryptFilms)
            {
                FilmFilesList.ShowCryptFilms = false;
            }
            else
            {
                PassWnd PasswordWindow = new PassWnd();
                PasswordWindow.ShowDialog();
            }
            Close();            
        }

        private void SecretButton_MouseEnter(object sender, MouseEventArgs e)
        {
            SecretButton.Content = new Image { Source = Utils.ConvertBitmapToBitmapImage(Dobrofilm.Properties.Resources.Key) };            
        }

        private void SecretButton_MouseLeave(object sender, MouseEventArgs e)
        {
            SecretButton.Content = null;
        }

        private void MoveAllFilmsToSelectedDirectory()
        {
            string NewFolderPath = Utils.SelectFolderDlg;
            string Drive = NewFolderPath.Substring(0, 1);
            DriveInfo DriveInf = new DriveInfo(Drive);
            long freeSpaceInBytes = DriveInf.AvailableFreeSpace;
            FilmFilesList filmFilesList = new FilmFilesList();
            ListCollectionView filmFiles = filmFilesList.FilmFiles;
            long TotalFilesLengthInBytes = Utils.TotalFilmsLength;
            if (TotalFilesLengthInBytes > freeSpaceInBytes)
            {
                Utils.ShowErrorDialog("Insufficient disc space");
                return;
            }
            foreach (FilmFile Film in filmFiles)
            {
                if (!Film.IsOnline)
                {
                    var Exten = System.IO.Path.GetExtension(Film.Path);
                    string NewPath = string.Concat(NewFolderPath, @"\", Film.Name, Exten);
                    File.Move(Film.Path, NewPath);
                    if (Utils.IsFileExists(NewPath))
                    {
                        Film.Path = NewPath;
                        filmFilesList.AddSaveFilmItemToXML(Film, false);
                    }
                }
            }
        }

        private void MoveAllFilms_Click(object sender, RoutedEventArgs e)
        {
            MoveAllFilmsToSelectedDirectory();
        }

        private void DelForgottenFilms_Click(object sender, RoutedEventArgs e)
        {
            FilmFilesList filmFilesList = new FilmFilesList();
            if (!FilmFilesList.ShowCryptFilms)
            {
                PassWnd passWnd = new PassWnd();
                passWnd.ShowDialog();
            }
            ListCollectionView filmFiles = filmFilesList.FilmFiles;
            filmFiles.Filter = new Predicate<object>(IsCrypted);
            foreach (FilmFile film in filmFiles)
            {
                string DecryptedTempName = Utils.GenerateTempFilePath(film.Path);
                if (Utils.IsFileExists(DecryptedTempName)) File.Delete(DecryptedTempName);
            }
            MainWindow.OpenedCryptedFiles = null;
        }

        public bool IsCrypted(object de)
        {
            FilmFile film = de as FilmFile;
            return film.IsCrypted;
        }

    }
}
