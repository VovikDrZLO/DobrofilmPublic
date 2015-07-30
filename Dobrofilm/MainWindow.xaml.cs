using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Security.Cryptography;
using System.Text;
using BuckSoft.Controls.FtpBrowseDialog;
using System.Windows.Threading;
using System.ComponentModel;

namespace Dobrofilm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            /*TODO              
           * Upload files (Modify "Move To" dialog)
           * Waiting circle while working with FTP moving files 
           * FilmItems window
           * Button in Grid 
           */
            InitializeComponent(); 
            //LoadSubNodes();
            XMLConverter xMLConverter = new XMLConverter();
            if (xMLConverter.IsNeedConvert()) xMLConverter.MakeConversion();            
            FilmFilesList.ShowCryptFilms = false;
            HomeFolders homeFolders = new HomeFolders();
            homeFolders.CheckHomeFolders();
            homeFolders.CheckFTP();
            ProfilesComboBox.DataContext = new XMLEdit();
            ProfilesComboBox.SelectedIndex = 0;
            MainGridData.DataContext = new FilmFilesList();
            CategoryListBox.DataContext = new CategoryList();
            //SaltedHash saltedHash = new SaltedHash();
            //string test = saltedHash.GetHashedString("P@ssw0rd");
            //bool test2 = saltedHash.VerifyHashString("P@ssw0rd", test);
            
            //XMLEdit xMLEdit = new XMLEdit();
            //xMLEdit.GetFilmFileFromXML(FilmFilesList.ShowCryptFilms);
        }


        private void LoadSubNodes()
        {
            FtpWebRequest req = (FtpWebRequest)FtpWebRequest.Create("ftp://" + Dobrofilm.Properties.Settings.Default.FTPURL);
            req.Credentials = new NetworkCredential(Dobrofilm.Properties.Settings.Default.FTPUser, Dobrofilm.Properties.Settings.Default.FTPPass);
            req.UsePassive = true;
            req.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            try
            {
                FtpWebResponse rsp = (FtpWebResponse)req.GetResponse();
                StreamReader rsprdr = new StreamReader(rsp.GetResponseStream());
                String[] rsptokens = rsprdr.ReadToEnd().Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String str in rsptokens)
                {
                    if (str.Contains("<DIR>") == true)
                    {
                        String[] directorytokens = str.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        string test = directorytokens[directorytokens.Length - 1].Trim();
                    }
                    else
                    {
                        string TestStr = "-rw-r--r--   1 givc     playgod     68161 Jun  3 12:59 Вопрос по НЭКу 200115.docx";                        
                        string Test2 = TestStr.Substring(TestStr.IndexOf(":") + 3);
                        string Test3 = str.Substring(str.IndexOf(":") + 3);
                        //String[] filetokens = str.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        //string test = filetokens[filetokens.Length - 1].Trim();
                    }
                }
            }
            catch (WebException wEx)
            {

            }
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

        private readonly BackgroundWorker FTPWorker = new BackgroundWorker();

        private void FTPWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run all background tasks here                 
            try
            {                
                string FTPDownloadedFilePath = Utils.DownloadFromFTP((String)e.Argument, "");
                e.Result = FTPDownloadedFilePath;
            }
            catch (OutOfMemoryException err)
            {
                Utils.ShowErrorDialog(string.Concat("FTP Error", err.Source));
                return;
            }
        }

        private void FTPWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work            
            Window LoadingWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Title == "wnd_Loading");
            if (LoadingWindow != null)
            {
                LoadingWindow.Close();
            }
            string FTPDownloadedFilePath = (String)e.Result;
            if (FTPDownloadedFilePath != null)
            {
                OpenedCryptedFiles.Add(FTPDownloadedFilePath);
                System.Diagnostics.Process.Start(FTPDownloadedFilePath);
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
                else if (SelectedFilm.IsFTP)
                {
                    string FilePath = SelectedFilm.Path;
                    string TempDirectory = Dobrofilm.Properties.Settings.Default.TempFilePathFTP;
                    string FTPDownloadedFilePath = TempDirectory + "\\" + Path.GetFileName(SelectedFilm.Path); 
                    if (OpenedCryptedFiles == null) OpenedCryptedFiles = new List<string>();
                    if (OpenedCryptedFiles.Contains(FTPDownloadedFilePath))
                    {
                        System.Diagnostics.Process.Start(FTPDownloadedFilePath);
                    }
                    else if (Utils.ShowYesNoDialog("It's FTP link, want download?"))                     
                    {
                        FTPWorker.DoWork += FTPWorker_DoWork;
                        FTPWorker.RunWorkerCompleted += FTPWorker_RunWorkerCompleted;                        
                        Utils.ShowLoadingWindow("Downloading from FTP...", FilePath);
                        FTPWorker.RunWorkerAsync(FilePath);          
                        
                        //DownloadedFilePath = Utils.DownloadFromFTP(FilePath, "");
                        
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
                                OpenedCryptedFiles.Add(NewFilePath);
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

        private readonly BackgroundWorker FTPMassiveWorker = new BackgroundWorker();

        private void FTPMassiveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run all background tasks here                 
            try
            {
                string FTPDownloadedFilePath = Utils.DownloadFromFTP((String)e.Argument, "");
                e.Result = FTPDownloadedFilePath;
            }
            catch (OutOfMemoryException err)
            {
                Utils.ShowErrorDialog(string.Concat("FTP Error", err.Source));
                return;
            }
        }

        private void FTPMassiveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work            
            Window LoadingWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Title == "wnd_Loading");
            if (LoadingWindow != null)
            {
                LoadingWindow.Close();
            }
            string FTPDownloadedFilePath = (String)e.Result;
            if (FTPDownloadedFilePath != null)
            {
                OpenedCryptedFiles.Add(FTPDownloadedFilePath);
                System.Diagnostics.Process.Start(FTPDownloadedFilePath);
            }
        }

        private void MoveChekedFilms_Click(object sender, RoutedEventArgs e)
        {         
            bool IsAllFilmsOnFTP = true;
            bool IsAllFilmsLocal = true;
            IEnumerable<FilmFile> CheckList = ListOfChekedFilms();
            CheckList = CheckList.Where(u => u.IsOnline == false).ToList(); // Do Not Move Online Links
            foreach (FilmFile Film in CheckList)
            {
                if (Film.IsFTP)
                {
                    IsAllFilmsLocal = false;
                }
                else 
                {
                    IsAllFilmsOnFTP = false;
                }
            }
            if (IsAllFilmsOnFTP && !IsAllFilmsLocal) //Move All From FTP
            {
                string NewFolder = Utils.SelectFolderDlg;
                foreach (FilmFile Film in CheckList)
                {
                    FTPMassiveWorker.RunWorkerAsync(new {Film = Film, Folder = NewFolder});
                    string DownloadedFilePath = Utils.DownloadFromFTP(Film.Path, NewFolder);
                    if (DownloadedFilePath != null)
                    {
                        Film.IsFTP = false;
                        Film.Path = DownloadedFilePath;
                        XMLEdit xMLEdit = new XMLEdit();
                        xMLEdit.AddFilmToXML(Film, false);
                    }                    
                }
            }
            else if (!IsAllFilmsOnFTP && IsAllFilmsLocal) //All Files Are Local
            {
                if (Utils.ShowYesNoDialog("Move to FTP?")) //Move All Files To FTP
                {
                    foreach (FilmFile Film in CheckList)
                    {                        
                        FtpWebResponse response = Utils.GetFtpResponse(FTPMethod.Upload, Film.Path);
                        if (OpenedCryptedFiles == null) OpenedCryptedFiles = new List<string>();
                        OpenedCryptedFiles.Add(Film.Path);
                        Film.IsFTP = true;
                        Film.Path = response.ResponseUri.OriginalString;
                        XMLEdit xMLEdit = new XMLEdit();
                        xMLEdit.AddFilmToXML(Film, false);                        
                    }

                }
                else //Move All Files Local
                {
                    string NewFolder = Utils.SelectFolderDlg;
                    foreach (FilmFile Film in CheckList)
                    {                        
                        string Exten = System.IO.Path.GetExtension(Film.Path);
                        string NewPath = string.Concat(NewFolder, @"\", Film.Name, Exten);
                        Utils.MoveFile(Film.Path, NewPath);
                        Film.Path = NewPath;
                        XMLEdit xMLEdit = new XMLEdit();
                        xMLEdit.AddFilmToXML(Film, false);                        
                    }
                }
            }
            else //Files Are Local and FTP, Ask Separately
            {
                foreach (FilmFile Film in CheckList)
                {
                    if (Film.IsFTP)
                    {
                        string NewFolder = Utils.SelectFolderDlg;
                        string DownloadedFilePath = Utils.DownloadFromFTP(Film.Path, NewFolder);
                        if (DownloadedFilePath != null)
                        {
                            Film.IsFTP = false;
                            Film.Path = DownloadedFilePath;
                            XMLEdit xMLEdit = new XMLEdit();
                            xMLEdit.AddFilmToXML(Film, false);
                        }     
                    }
                    else
                    {
                        if (Utils.ShowYesNoDialog("Move to FTP File " + Film.Name + "?")) //Move All Files To FTP
                        {                            
                            FtpWebResponse response = Utils.GetFtpResponse(FTPMethod.Upload, Film.Path);
                            if (OpenedCryptedFiles == null) OpenedCryptedFiles = new List<string>();
                            OpenedCryptedFiles.Add(Film.Path);
                            Film.IsFTP = true;
                            Film.Path = response.ResponseUri.OriginalString;
                            XMLEdit xMLEdit = new XMLEdit();
                            xMLEdit.AddFilmToXML(Film, false);                            

                        }
                        else //Move All Files Local
                        {
                            string NewFolder = Utils.SelectFolderDlg;                            
                            string Exten = System.IO.Path.GetExtension(Film.Path);
                            string NewPath = string.Concat(NewFolder, @"\", Film.Name, Exten);
                            Utils.MoveFile(Film.Path, NewPath);
                            Film.Path = NewPath;
                            XMLEdit xMLEdit = new XMLEdit();
                            xMLEdit.AddFilmToXML(Film, false);                            
                        }
                    }                    
                }
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

        private void OpenAddFTPLinkBtn_Click(object FTPsender, RoutedEventArgs e)
        {          

            string FTPURL = Dobrofilm.Properties.Settings.Default.FTPURL; //@"playgod.pro";
            string FTPUsr = Dobrofilm.Properties.Settings.Default.FTPUser;
            string FTPPass = Dobrofilm.Properties.Settings.Default.FTPPass;
            FtpBrowseDialog ftpBrowseDialog = new FtpBrowseDialog(FTPURL, "", 21, FTPUsr, FTPPass, false);            
            ftpBrowseDialog.ShowDialog();
            if (ftpBrowseDialog.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                timer.Start();
                timer.Tick += (sender, args) =>
                {
                    timer.Stop();
                    Utils.DownloadFileNames(ftpBrowseDialog.SelectedFile);
                };                
                
            }            
        }
    }
}			

