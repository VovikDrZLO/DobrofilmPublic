using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Dobrofilm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();           
            XMLConverter xMLConverter = new XMLConverter();
            if (xMLConverter.IsNeedConvert()) xMLConverter.MakeConversion();            
            FilmFilesList.ShowCryptFilms = false;
            HomeFolders homeFolders = new HomeFolders();
            homeFolders.CheckHomeFolders();
            ProfilesComboBox.DataContext = new XMLEdit();
            ProfilesComboBox.SelectedIndex = 0;
            MainGridData.DataContext = new FilmFilesList();
            CategoryListBox.DataContext = new CategoryList();            
            //XMLEdit xMLEdit = new XMLEdit();
            //xMLEdit.GetFilmFileFromXML(FilmFilesList.ShowCryptFilms);
        }               

        public static List<string> OpenedCryptedFiles { get; set; }
        public static ProfileClass CurrentProfile { get; set; }

        private void AddFilesClick(object sender, RoutedEventArgs e)
        {
            FilmFilesList filmFilesList = new FilmFilesList();
            filmFilesList.AddFilmsToFilmFiles();
            UpdateMainGridData();
            //MainGridData.ItemsSource = filmFilesList.FilmFiles;            
        }

        private void OpenCategoryWindow_Click(object sender, RoutedEventArgs e)
        {            
            CategoryItem categoryWindow = new CategoryItem();
            categoryWindow.ShowDialog();
            CategoryList categoryList = new CategoryList();
            CategoryListBox.ItemsSource = categoryList.Category;            
        }

        private void ListBox1_MouseDoubleClick(object sender, RoutedEventArgs e)        {
            if (CategoryListBox.SelectedItem == null)
            {
                return;
            }
            if (CategoryListBox.SelectedItem.GetType() == typeof(CategoryClass))
            {
                CategoryClass SelectedCategory = (CategoryClass)CategoryListBox.SelectedItem;
                CategoryItem categoryWindow = new CategoryItem(SelectedCategory);
                categoryWindow.ShowDialog();
                CategoryListBox.DataContext = new CategoryList();
                CategoryListBox.Items.Refresh();
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            FilmCategoryFilterWindow filmCategoryFilterWindow = new FilmCategoryFilterWindow();
            filmCategoryFilterWindow.ShowDialog();
        }

        private void CategoryListBox_SelectionChange(object sender, RoutedEventArgs e)
        {
            if (CategoryListBox.SelectedItem == null)
            {
                return;
            }
            if (CategoryListBox.SelectedItem.GetType() == typeof(CategoryClass))
            {
                FilmGridFilerByCategory((CategoryClass)CategoryListBox.SelectedItem);
            }            
        }

        private void FilmGridFilerByCategory(CategoryClass SelectedCategory)
        {            
            FilmFilesList filmFilesList = new FilmFilesList();
            if (SelectedCategory.ID == -1)
            {
                var FilteredSource = filmFilesList.GetFilmListByCategory(new int[] { }, AndOrEnum.And);
                MainGridData.ItemsSource = FilteredSource;
            }
            else
            {
                var FilteredSource = filmFilesList.GetFilmListByCategory(new int[1] { SelectedCategory.ID }, AndOrEnum.And);
                MainGridData.ItemsSource = FilteredSource;
            }
        }

        private void FileBtn_DbClick(object sender, MouseButtonEventArgs e)
        {
            if (MainGridData.SelectedItem == null)
            {
                return;
            }
            FilmFile SelectedFilm = (FilmFile)MainGridData.SelectedItem;
            if (e.ChangedButton == MouseButton.Left)
            {                
                if (SelectedFilm.IsOnline)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(Dobrofilm.Properties.Settings.Default.DefaultBrowser, SelectedFilm.Path);
                    }
                    catch
                    {
                        Utils.ShowErrorDialog("Error occurred while trying to open browser, default browser may be not correct.");
                    }
                }
                else if (Utils.IsFileExists(SelectedFilm.Path))
                {
                    if (!SelectedFilm.IsCrypted)
                    {                                           
                        System.Diagnostics.Process.Start(SelectedFilm.Path);
                    }
                    else
                    {
                        string NewFilePath = Utils.GenerateTempFilePath(SelectedFilm.Path);                            
                        if (OpenedCryptedFiles == null) OpenedCryptedFiles = new List<string>();
                        if (OpenedCryptedFiles.Contains(NewFilePath))
                        {
                            System.Diagnostics.Process.Start(NewFilePath);
                        }
                        else
                        {                            
                            PassWnd passWnd = new PassWnd(SelectedFilm.Path, NewFilePath);
                            passWnd.ShowDialog();                            
                            if (Utils.IsFileExists(NewFilePath))
                            {
                                //OpenedCryptedFiles.Add(NewFilePath);
                                System.Diagnostics.Process.Start(NewFilePath);
                            }
                        }
                    }                    
                }
                else
                {
                    Utils.ShowWarningDialog("Link is corrupted");
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                string TempFilePath = Utils.GenerateTempFilePath(SelectedFilm.Path);                
                FilmItem FilmItemWindow = new FilmItem(SelectedFilm);
                FilmItemWindow.ShowDialog();
                UpdateMainGridData();
            }
        }
            

        private void MainWindow_Resize(object sender, SizeChangedEventArgs e)
        {
            MainWindow mainWindow = sender as MainWindow;            
            double WinHeight = e.NewSize.Height;
            double WinWidth = e.NewSize.Width;
            CategoryListBox.Height = WinHeight - 179;             
            MainGridData.Height = WinHeight - 150;
            MainGridData.Width = WinWidth - 240;
            MenuBar.Width = WinWidth;
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {            
            CloseApplication();            
        }

        private void DelFilm_Click(object sender, RoutedEventArgs e)
        {
            if (MainGridData.SelectedItem == null)
            {
                return;
            }            
            FilmFile filmFile = MainGridData.SelectedItem as FilmFile;
            //FilmFilesList filmFilesList = new FilmFilesList();
            //filmFilesList.DeleteFilmItemFromXml(filmFile);
            XMLEdit xMLEdit = new XMLEdit();
            xMLEdit.DeleteFilmItemFromXml(filmFile);
            UpdateMainGridData();
            //MainGridData.ItemsSource = filmFilesList.FilmFiles;
        }

        private void DelCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryListBox.SelectedItem == null)
            {
                return;
            }
            CategoryList categoryList = new CategoryList();
            CategoryClass categoryClass = CategoryListBox.SelectedItem as CategoryClass;
            FilmFilesList filmFilesList = new FilmFilesList();
            //GetFilmListByCategory(int[] CategotisLocalArray, AndOrEnum LogicOperation)
            ListCollectionView FilteredFileList = 
                filmFilesList.GetFilmListByCategory(new int[1] { categoryClass.ID}, AndOrEnum.And);
            if (FilteredFileList.Count > 0)
            {
                Utils.ShowWarningDialog("Can't delete linked Categoris");
                return;
            }
            XMLEdit xMLEdit = new XMLEdit();
            //categoryList.DelCategory(categoryClass);
            xMLEdit.DelCategory(categoryClass);
            CategoryListBox.DataContext = new CategoryList();
            CategoryListBox.Items.Refresh();            
        }

        private void BadFilePathBtn_Click(object sender, RoutedEventArgs e)
        {
            FilmFilesList filmFilesList = new FilmFilesList();
            ListCollectionView filmFiles = filmFilesList.FilmFiles;
            ListCollectionView CorruptedFiles = filmFiles;
            CorruptedFiles.Filter = new Predicate<object>(IsBadFile);
            MainGridData.ItemsSource = CorruptedFiles;                      
        }
        
        public bool IsBadFile(object de)
        {
            FilmFile File = de as FilmFile;
            return !Utils.IsFileExists(File.Path) && !File.IsOnline;            
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
            CategoryList categoryList = new CategoryList();            
            CategoryListBox.ItemsSource = categoryList.Category;
            UpdateMainGridData();
            XMLEdit xMLEdit = new XMLEdit();
            int SelIndex = ProfilesComboBox.SelectedIndex;
            ProfilesComboBox.ItemsSource = xMLEdit.GetProfilesList;
            ProfilesComboBox.SelectedIndex = SelIndex;            
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void AddToPL_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<FilmFile> CheckList = ListOfChekedFilms();
            string SelectedFilmPath = string.Empty;            
            //if (MainGridData.SelectedItem != null)
            //{
            //     FilmFile SelectedFilm = (FilmFile)MainGridData.SelectedItem;
            //     SelectedFilmPath = SelectedFilm.Path;                
            //     if (!CheckList.Contains(SelectedFilm))
            //     {
                     
            //     }
            //}            
            string ParamString = string.Empty;            
            foreach (FilmFile filmFile in CheckList)
            {
                if (Utils.IsFileExists(filmFile.Path))
                {
                    if (filmFile.IsCrypted)
                    {
                        string TempFilePath = Utils.GenerateTempFilePath(filmFile.Path);
                        Window passwnd = new PassWnd(filmFile.Path, TempFilePath);
                        passwnd.ShowDialog();
                        ParamString = string.Concat(ParamString," /add ", TempFilePath);
                        if (OpenedCryptedFiles == null) OpenedCryptedFiles = new List<string>();
                        OpenedCryptedFiles.Add(TempFilePath);                        
                    }
                    else
                    {
                        ParamString = string.Concat(ParamString + " /add " + filmFile.Path);
                    }                    
                }                
            }
            System.Diagnostics.ProcessStartInfo MpcPalyer = new System.Diagnostics.ProcessStartInfo(Dobrofilm.Properties.Settings.Default.MPCPath);
            MpcPalyer.Arguments = ParamString;
            System.Diagnostics.Process.Start(MpcPalyer);            
        }

        private void FilmBtn_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).ContextMenu.IsEnabled = true;
            (sender as Button).ContextMenu.PlacementTarget = (sender as Button);
            (sender as Button).ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            (sender as Button).ContextMenu.IsOpen = true;
        }

        private void MainWindow1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            CloseApplication();            
        }

        private void CloseApplication()
        {
            if (Utils.ShowYesNoDialog("Realy Want Exit Dobrofilm"))
            {
                DeleteTempDecryptedFiles();
                System.Environment.Exit(0); 
            } 
        }

        private IEnumerable<FilmFile> ListOfChekedFilms()
        {
            IEnumerable<FilmFile> CheckList =
                            (from FilmFile p in MainGridData.ItemsSource
                             where p.IsCheked == true
                             select p);
            return CheckList;
        }

        private void LinkBtn_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<FilmFile> CheckList = ListOfChekedFilms();
            LinksList linksList = new LinksList();
            foreach (FilmFile filmFile in CheckList)
            {
                foreach (FilmFile innerfilmFile in CheckList)
                {
                    if (innerfilmFile.ID != filmFile.ID)
                    {
                        LinksClass linkClass = new LinksClass { From = filmFile.ID, To = innerfilmFile.ID };
                        XMLEdit xMLEdit = new XMLEdit();
                        xMLEdit.AddLinkToXML(linkClass);
                        //linksList.AddLink(linkClass);
                    }                    
                }                
            }
        }        

        private void DeleteTempDecryptedFiles()
        {
            if (OpenedCryptedFiles == null) return;
            foreach (string FilePath in OpenedCryptedFiles) 
            {
                try
                {
                    Utils.DeleteFile(FilePath);                    
                }
                catch (IOException err)
                {
                    Utils.ShowErrorDialog(string.Concat("File ", FilePath, " couldn't be deleted ", err.Message));
                }
                catch (UnauthorizedAccessException err)
                {
                    Utils.ShowErrorDialog(string.Concat("File ", FilePath, " couldn't be deleted ", err.Message));
                }
            }
        }

        private void DecodeAllSelectedFiles_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<FilmFile> CheckList = ListOfChekedFilms();
            List<FilmFile> SelectedCryptedFilms = new List<FilmFile> { };           
            foreach (FilmFile CryptedFilm in CheckList) 
            {
                if (CryptedFilm.IsCrypted)
                {                 
                    SelectedCryptedFilms.Add(CryptedFilm);                
                } 
            }
            if (SelectedCryptedFilms.Count > 0)
            {
                Window passwnd = new PassWnd(SelectedCryptedFilms);
                passwnd.ShowDialog();
            }
        }

        private void OpenAddLinkBtn_Click(object sender, RoutedEventArgs e)
        {
            Window filmItems = new FilmItem();
            filmItems.ShowDialog();
            UpdateMainGridData();
        }

        private void UpdateMainGridData()
        {
            if (CategoryListBox.SelectedItem != null)
            {
                if (CategoryListBox.SelectedItem.GetType() == typeof(CategoryClass))
                {
                    FilmGridFilerByCategory((CategoryClass)CategoryListBox.SelectedItem);
                }
            }
            else
            {
                FilmFilesList filmFilesList = new FilmFilesList();
                MainGridData.ItemsSource = filmFilesList.FilmFiles;
            }
        }

        private void EncodeAllSelectedFiles_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<FilmFile> CheckList = ListOfChekedFilms();
            foreach (FilmFile Film in CheckList)
            {
                string CryptFileName =  Utils.GenerateCryptFilePath(Film.Path);
                Mouse.OverrideCursor = Cursors.Wait;
                if (!Film.IsCrypted && !Film.IsOnline) Utils.EncryptFile(Film.Path, CryptFileName);
                if (OpenedCryptedFiles == null) OpenedCryptedFiles = new List<string>();
                OpenedCryptedFiles.Add(Film.Path);
                Film.IsCrypted = true;
                Film.Path = CryptFileName;
                XMLEdit xMLEdit = new XMLEdit();
                xMLEdit.AddFilmToXML(Film, false);
                //FilmFilesList FileList = new FilmFilesList();
                //FileList.AddSaveFilmItemToXML(Film, false);                
            }
            Mouse.OverrideCursor = null;
            UpdateMainGridData();
        }

        private void MainGridData_LoadingRowDetails(object sender, DataGridRowDetailsEventArgs e)
        {
            DataGridRow row = e.Row as DataGridRow;
            Canvas DetailCanvas = (Canvas)e.DetailsElement.FindName("DetailCanvas");
            
            FilmFile SelectedFilm = (FilmFile)row.Item;
            //Guid FilmID = SelectedFilm.ID;
            //FilmScreenShot ScreenClass = new FilmScreenShot();
            XMLEdit xMLEdit = new XMLEdit();
            IList<ScreenShotItem> ScreenShotItems = xMLEdit.GetScreenShootsByFilmFile(SelectedFilm);//ScreenClass.GetScreenShotsByFilmID(FilmID);
            if (ScreenShotItems.Count > 0)
            {
                DetailCanvas.Height = 100;
            }

            for (int i = 0; i <= ScreenShotItems.Count - 1 && i < 4; i++)
            {
                Image img = Utils.GetImageByBase64Str(ScreenShotItems[i].Base64String);
                DetailCanvas.Children.Add(img);
                img.Margin = new Thickness(0, 5, 1, 1);
                Canvas.SetLeft(img, 130 * i);                
            }                                    
        }

        private void ShowEncriptedFilms_Click(object sender, RoutedEventArgs e)
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
            UpdateMainGridData();
        }

        private void MoveChekedFilms_Click(object sender, RoutedEventArgs e)
        {
            string NewFolder = Utils.SelectFolderDlg;
            IEnumerable<FilmFile> CheckList = ListOfChekedFilms();
            foreach (FilmFile Film in CheckList)
            {
                string Exten = System.IO.Path.GetExtension(Film.Path);
                string NewPath = string.Concat(NewFolder, @"\", Film.Name, Exten);
                Utils.MoveFile(Film.Path, NewPath);
                Film.Path = NewPath;
                XMLEdit xMLEdit = new XMLEdit();
                xMLEdit.AddFilmToXML(Film, false);
                //FilmFilesList filmFilesList = new FilmFilesList();
                //filmFilesList.AddSaveFilmItemToXML(Film, false);
            }
        }

        private void EncrFlsInFolder_Click(object sender, RoutedEventArgs e)
        {
            string FilesFolder = Utils.SelectFolderDlg;
            if (FilesFolder == string.Empty) return;
            DirectoryInfo dInfo = new DirectoryInfo(FilesFolder);
            DirectoryInfo[] subdirs = dInfo.GetDirectories();
            var allfiles = Directory.GetFiles(FilesFolder, "*.*", SearchOption.AllDirectories);
            string[] AllFilesArray = allfiles.ToArray();
            foreach (string ImagePath in AllFilesArray)
            {
                string NewImagePath = Utils.GenerateCryptFilePath(ImagePath);
                Utils.EncryptFile(ImagePath, NewImagePath);
                if (Utils.IsFileExists(NewImagePath)) Utils.DeleteFile(ImagePath);
            }
        }


        private void DecrFlsInFolder_Click(object sender, RoutedEventArgs e)
        {
            string FilesFolder = Utils.SelectFolderDlg;
            if (FilesFolder == string.Empty) return;
            DirectoryInfo dInfo = new DirectoryInfo(FilesFolder);
            var allfiles = Directory.GetFiles(FilesFolder, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith("CrypDobFilm"));
            string[] AllFilesArray = allfiles.ToArray();
            List<FilmFile> FileList = new List<FilmFile>();
            foreach (string ImagePath in AllFilesArray)
            {
                FileList.Add(new FilmFile { Path = ImagePath });
                string NewImagePath = Utils.GenerateTempFilePath(ImagePath);
                if (Utils.IsFileExists(NewImagePath)) Utils.DeleteFile(ImagePath);
            }
            PassWnd passWnd = new PassWnd(FileList);
            passWnd.ShowDialog();

            if (Utils.ShowYesNoDialog("Left files decrypted after close app?"))
            {
                foreach (FilmFile filmFile in FileList)
                {
                    if (Utils.IsFileExists(filmFile.Path)) Utils.DeleteFile(filmFile.Path);
                    MainWindow.OpenedCryptedFiles.Remove(Utils.GenerateTempFilePath(filmFile.Path));
                } 
            }
        }

        private void SelectChange(object sender, SelectionChangedEventArgs e)
        {
            if (ProfilesComboBox.SelectedItem == null) return;

            if (ProfilesComboBox.SelectedItem.GetType() == typeof(ProfileClass))
            {
                ProfileClass SelProfile = (ProfileClass)ProfilesComboBox.SelectedItem;
                CurrentProfile = SelProfile;
                UpdateMainGridData();
                CategoryList categoryList = new CategoryList();
                CategoryListBox.ItemsSource = categoryList.Category;
                //FilmFilesList filmFilesList = new FilmFilesList();
                //ListCollectionView FilteredSource = filmFilesList.GetFilmListByProfile(SelProfile);
                //MainGridData.ItemsSource = FilteredSource;
            }            
        }
    }
}
