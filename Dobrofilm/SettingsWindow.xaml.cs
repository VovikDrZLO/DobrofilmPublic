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
            SettingsFilePath.Text = Dobrofilm.Properties.Settings.Default.SettingsPath;            
            MPCRath.Text = Dobrofilm.Properties.Settings.Default.MPCPath;
            DefBrowserTB.Text = Dobrofilm.Properties.Settings.Default.DefaultBrowser;                        
            XMLEdit xMLEdit = new XMLEdit();
            FilmMaskTB.Text = xMLEdit.GetFilmMask();
            FilmNextIDTB.Text = xMLEdit.GetFileMaskNextID();
            CategoryNextIDTB.Text = xMLEdit.GetCategoryNextID();
            //CategoryList categoryList = new CategoryList();
            //CategoryNextIDTB.Text = categoryList.GetCategoryNextID();
            //FilmFilesList filmFilesList = new FilmFilesList();
            //FilmMaskTB.Text = filmFilesList.GetFilmMask();
            //FilmNextIDTB.Text = filmFilesList.GetFileMaskNextID();
            HomeFolders homeFolders = new HomeFolders();
            HomeFoldersDataGrid.ItemsSource = homeFolders.HomeFoldersList;                        
            ProfileDataGrid.ItemsSource = xMLEdit.GetProfilesList;

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
                XMLEdit xMLEdit = new XMLEdit();
                xMLEdit.DeleteFilmItemFromXml(filmFile);
                //filmFilesList.DeleteFilmItemFromXml(filmFile);
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
            if (!Utils.IsFileExists(SettingsFilePath.Text))
            {
                Utils.ShowWarningDialog(string.Format("Settings file {0} does not exists", SettingsFilePath.Text));             
            }
            else if(!Utils.IsFileExists(MPCRath.Text))
            {
                Utils.ShowWarningDialog(string.Format("Media Player Classic was not found by this path {0}", MPCRath.Text));             
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
                Dobrofilm.Properties.Settings.Default.SettingsPath = SettingsFilePath.Text;                
                Dobrofilm.Properties.Settings.Default.MPCPath = MPCRath.Text;                
                Dobrofilm.Properties.Settings.Default.DefaultBrowser = DefBrowserTB.Text;                
                Dobrofilm.Properties.Settings.Default.Save();
                CategoryList categoryList = new CategoryList();
                XMLEdit xMLEdit = new XMLEdit();
                xMLEdit.SetCategoryID(NewCategoryID);                
                xMLEdit.SetFilmMaskAndCounter(FilmMaskTB.Text, NewFilmMaskID); 
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
            FilmFilesList filmFilesList = new FilmFilesList();
            XMLEdit xMLEdit = new XMLEdit();
            ListCollectionView filmFiles = filmFilesList.FilmFiles;
            long TotalFilesLengthInBytes = Utils.TotalFilmsLength;
            int TotalFilesLengthInMBytes = (int)TotalFilesLengthInBytes / 1024 / 1024;
            if (!Utils.ShowYesNoDialog(string.Format("Need {0} MBytes, proceed", TotalFilesLengthInMBytes))) return;
            string NewFolderPath = Utils.SelectFolderDlg;
            if (NewFolderPath == string.Empty) return;
            string Drive = NewFolderPath.Substring(0, 1);
            DriveInfo DriveInf = new DriveInfo(Drive);
            long freeSpaceInBytes = DriveInf.AvailableFreeSpace;            
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
                    Utils.MoveFile(Film.Path, NewPath);
                    if (Utils.IsFileExists(NewPath))
                    {
                        Film.Path = NewPath;
                        xMLEdit.AddFilmToXML(Film, false);
                        //filmFilesList.AddSaveFilmItemToXML(Film, false);
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
                if (Utils.IsFileExists(DecryptedTempName))
                {
                    try
                    {
                        Utils.DeleteFile(DecryptedTempName);
                    }
                    catch (IOException err)
                    {
                        Utils.ShowErrorDialog(err.Message);
                    }
                }
            }
            MainWindow.OpenedCryptedFiles = null;
        }

        public bool IsCrypted(object de)
        {
            FilmFile film = de as FilmFile;
            return film.IsCrypted;
        }

        private void AddHomeFolder_Click(object sender, RoutedEventArgs e)
        {            
            HomeFolders homeFolders = new HomeFolders();
            homeFolders.AddHomeFolder(new DirectoryInfo(Utils.SelectFolderDlg));
            HomeFoldersDataGrid.ItemsSource = homeFolders.HomeFoldersList;        
        }

        private void RemHomeFolder_Click(object sender, RoutedEventArgs e)
        {
            HomeFolders homeFolders = new HomeFolders();
            homeFolders.RemHomeFolder((DirectoryInfo)HomeFoldersDataGrid.SelectedItem);
            HomeFoldersDataGrid.ItemsSource = homeFolders.HomeFoldersList;        
        }

        private void AddProfile_Click(object sender, RoutedEventArgs e)
        {
            ProfilesList profilesList = new ProfilesList();
            profilesList.ShowNewProfileWindow();
            XMLEdit xMLEdit = new XMLEdit();
            ProfileDataGrid.ItemsSource = xMLEdit.GetProfilesList;
        }

        private void DelProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileDataGrid.SelectedItem != null)
            {
                ProfileClass Profile = (ProfileClass)ProfileDataGrid.SelectedItem;
                if (Profile == null) return;
                XMLEdit xMLEdit = new XMLEdit();
                xMLEdit.DeleteProfile(Profile);
                ProfileDataGrid.ItemsSource = xMLEdit.GetProfilesList;
            }
        }

        private void ProfileBtn_DbClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid)
            {
                DataGrid ProfilesDataGrid = (DataGrid)sender;
                ProfileClass Profile = (ProfileClass)ProfilesDataGrid.SelectedItem;
               
                ProfilesList profilesList = new ProfilesList();
                profilesList.ShowNewProfileWindow(Profile);
                XMLEdit xMLEdit = new XMLEdit();
                ProfileDataGrid.ItemsSource = xMLEdit.GetProfilesList;
            }
            
        }
    }
}
