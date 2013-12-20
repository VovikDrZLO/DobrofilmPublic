using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;
using System.IO;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Resources;


namespace Dobrofilm
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class FilmItem : Window
    {
        DispatcherTimer timer;
        public Guid FilmID;
        private bool IsCrypted {get; set;}
        private string FilePath {get; set;}   
        private string NewFileName{get;set;}
        private bool IsOnline { get; set; }
        private FilmFile FilmPublic { get; set; }

        public FilmItem(FilmFile SelectedFilm)
        {
            FilmPublic = SelectedFilm;
            InitializeComponent();
            FilmID = SelectedFilm.ID;
            FilmName.Text = SelectedFilm.Name;
            FilmHint.Text = SelectedFilm.Hint;
            IsCrypted = SelectedFilm.IsCrypted;
            FilePath = SelectedFilm.Path;
            IsOnline = false;
            DrawCategoryButtons(SelectedFilm);
            FilmRate.Value = SelectedFilm.Rate;
            if (SelectedFilm.IsOnline)
            {
                MainTabControl.Margin = new Thickness(1, 1, 1, 1);
                //TODO: Add online film UI trasform
                FilmPlayer.Visibility = System.Windows.Visibility.Collapsed;
                
                WebBrowser FilmBrowser = new WebBrowser();
                FilmBrowser.Height = 329; //FilmPlayer.Height + 46;
                FilmBrowser.Width = 373; //FilmPlayer.Width + 45;                
                FilmBrowser.Source = new Uri(SelectedFilm.Path);
                FilmBrowser.Name = "FilmBrowser";
                MainTabCanvas.Children.Add(FilmBrowser);
                MainTabCanvas.RegisterName(FilmBrowser.Name, FilmBrowser);
                Canvas.SetLeft(FilmBrowser, 506);
                Canvas.SetTop(FilmBrowser, 49);                

                ChangePathBtn.Visibility = 
                    CryptFile.Visibility = 
                    Btn_Play.Visibility = 
                    SeekBar.Visibility = 
                    Btn_Screen.Visibility = 
                    MoveToBtn.Visibility = System.Windows.Visibility.Collapsed;               
            }
            else
            {                
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(200);
                timer.Tick += new EventHandler(timer_tick);
                FileStatus(SelectedFilm.Path);
                IsLinkTabSelectedFirstTime = true;
                string TempFileName = Utils.GenerateTempFilePath(FilePath);
                Btn_Play.IsEnabled = (!IsCrypted || MainWindow.OpenedCryptedFiles.Contains(TempFileName));
            }
        }

        public FilmItem()
        {
            InitializeComponent();
            FilmID = System.Guid.Empty;
            IsCrypted = false;
            FilmName.Text = "Type web addresse here";
            FilmHint.Text = "Короткая характеристика";
            IsOnline = true;            
            DrawCategoryButtons(new FilmFile { Categoris = new int[1] {0} });
            Btn_Play.Visibility = 
                SeekBar.Visibility = 
                MoveToBtn.Visibility =
                CryptFile.Visibility =
                ChangePathBtn.Visibility = 
                System.Windows.Visibility.Collapsed;  
        }

        public void FileStatus(string FilmFilePath)
        {
            FilePath = FilmFilePath;
            FilmItemStatusBar.Items.Clear();
            TextBlock FilePathTB = new TextBlock();
            FilePathTB.Text = FilmFilePath;
            FilePathTB.Width = 650;
            FilmItemStatusBar.Items.Add(FilePathTB);
            FilmItemStatusBar.Items.Add(new Separator());

            Image CryptImg = new Image();
            CryptImg.Name = "CryptImage";
            TextBlock CryptStatus = new TextBlock();
            CryptStatus.Name = "CryptStatus";
            CryptStatus.Width = 80;
            System.Drawing.Bitmap CryptBitmap;
            if (IsCrypted)
            {
                CryptFile.Visibility = System.Windows.Visibility.Hidden;
                CryptBitmap = Dobrofilm.Properties.Resources.Crypted;
                CryptStatus.Text = "Crypted";
            }
            else
            {
                CryptBitmap = Dobrofilm.Properties.Resources.NotCrypted;
                CryptStatus.Text = "NotCrypted";
            }
            CryptImg.Source = Utils.ConvertBitmapToBitmapImage(CryptBitmap);
            FilmItemStatusBar.Items.Add(CryptStatus);
            FilmItemStatusBar.Items.Add(CryptImg);
            FilmItemStatusBar.Items.Add(new Separator());
            Image img = new Image();
            img.Name = "StatusImage";
            TextBlock StatusTextBox = new TextBlock();
            StatusTextBox.Name = "StatusText";
            //Uri uri;
            System.Drawing.Bitmap bitmap;
            
            if (Utils.IsFileExists(FilmFilePath))
            {
                ChangePathBtn.Visibility = System.Windows.Visibility.Hidden;
                bitmap = Dobrofilm.Properties.Resources.GreenOk;                
                StatusTextBox.Text = "FileOk";
                string TempFilmPath = Utils.GenerateTempFilePath(FilmFilePath);
                if (MainWindow.OpenedCryptedFiles == null) MainWindow.OpenedCryptedFiles = new List<string>();
                if (IsCrypted && MainWindow.OpenedCryptedFiles.Contains(TempFilmPath) && Utils.IsFileExists(TempFilmPath))
                {
                    FilmPlayer.Source = new Uri(TempFilmPath);
                }
                else
                {
                    FilmPlayer.Source = new Uri(FilmFilePath);
                }

                Btn_Play.IsEnabled = true;
            }
            else
            {
                ChangePathBtn.Visibility = System.Windows.Visibility.Visible;
                bitmap = Dobrofilm.Properties.Resources.RedX;                
                StatusTextBox.Text = "FileMissing";
                Btn_Play.IsEnabled = false;
            }
            
            img.Source = Utils.ConvertBitmapToBitmapImage(bitmap);                        
            FilmItemStatusBar.Items.Add(img);              
            FilmItemStatusBar.Items.Add(StatusTextBox);
        }

        public void DrawCategoryButtons(FilmFile SelectedFilm)
        {
            CategoryList categoryList = new CategoryList();
            int RowCount = 0;
            int ColumnCount = 0;                        
            CategoryGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(65)});   
            foreach (CategoryClass categoryClass in categoryList.Category)
            {
                if (categoryClass.ID != -1)
                {
                    if (categoryClass.Icon.Length != 0)
                    {
                        Button CategoryButton = new Button();
                        CategoryButton.ToolTip = categoryClass.Hint;
                        Image img = new Image();
                        var grayBitmap = categoryList.GetGrayCategoryBitmapImageByID(categoryClass.ID);
                        var OriginBitmap = categoryList.GetCategoryBitmapImageByID(categoryClass.ID);
                        var isClicked = false;
                        if (Array.IndexOf(SelectedFilm.Categoris, categoryClass.ID) >= 0)
                        {
                            img.Source = OriginBitmap;
                            isClicked = true;
                            AddItemToSelectedCategoris(categoryClass);
                        }
                        else
                        {
                            img.Source = grayBitmap;                            
                        }
                        CategoryButton.Tag = new CategoryTag { IsClicked = isClicked, GrayIcon = grayBitmap, OriginIcon = OriginBitmap, categoryClass = categoryClass };
                        CategoryButton.Content = img;
                        CategoryButton.Click += new RoutedEventHandler(CategoryButtomClick);                        
                        
                        CategoryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });                                             

                        Grid.SetRow(CategoryButton, RowCount);
                        Grid.SetColumn(CategoryButton, ColumnCount);                        
                        CategoryGrid.Children.Add(CategoryButton);
                        ColumnCount++;
                        if (ColumnCount > 4)
                        {                            
                            CategoryGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(65) });   
                            ColumnCount = 0;
                            RowCount++;
                        }
                    }
                }
            }
        }

        void CategoryButtomClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                CategoryList categoryList = new CategoryList();
                Button butt = (Button)sender;
                Image img = (Image)butt.Content;
                CategoryTag categoryTag = butt.Tag as CategoryTag;                
                CategoryClass PressedCategoryBtn = categoryTag.categoryClass;
                bool IsClicked = categoryTag.IsClicked;                
                var GrayIcon = categoryTag.GrayIcon;
                IsClicked = !IsClicked;
                if (IsClicked)
                {
                    if (categoryTag.OriginIcon != null)
                    {
                        img.Source = categoryTag.OriginIcon;
                    }
                    else
                    {
                        BitmapImage OriginCategoryIcon = categoryList.GetCategoryBitmapImageByID(PressedCategoryBtn.ID);
                        img.Source = OriginCategoryIcon;
                        categoryTag.OriginIcon = OriginCategoryIcon;
                    }
                    AddItemToSelectedCategoris(PressedCategoryBtn);                    
                }
                else
                {
                    img.Source = GrayIcon;
                    SelectedCategoris.Remove(PressedCategoryBtn);
                    SellCategoris.ItemsSource = SelectedCategoris;
                    SellCategoris.Items.Refresh();
                }
                categoryTag.IsClicked = IsClicked;
            }
        }
        
        public List<CategoryClass> SelectedCategoris {get; set;}

        public void AddItemToSelectedCategoris(CategoryClass NewCategoryItem)
        {
            List<CategoryClass> selectedCategoris = SelectedCategoris;
            if (selectedCategoris == null)
            {
                selectedCategoris = new List<CategoryClass> { };
            }
            selectedCategoris.Add(NewCategoryItem);
            SelectedCategoris = selectedCategoris;
            SellCategoris.ItemsSource = SelectedCategoris;
            SellCategoris.Items.Refresh();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void FilmName_GotFocus(object sender, RoutedEventArgs e)
        {
            FilmName.SelectAll();
        }

        private void FilmName_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            FilmName.SelectAll();
        }

        private void FilmName_GotFocus(object sender, MouseEventArgs e)
        {
            FilmName.SelectAll();
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Btn_Play.Content.ToString() == "Play")
            {
                FilmPlayer.Play();
                Btn_Play.Content = "Pause";
            }
            else
            {
                FilmPlayer.Pause();
                Btn_Play.Content = "Play";
            }
            
        }

        private void Film_Opened(object sender, RoutedEventArgs e)
        {
            if (FilmPlayer.NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = FilmPlayer.NaturalDuration.TimeSpan;
                SeekBar.Maximum = ts.TotalMilliseconds;
                SeekBar.SmallChange = 1;
                SeekBar.LargeChange = Math.Min(10, ts.Seconds / 10);
            }
            timer.Start();
        }
        bool IsDragging = false;

        void timer_tick(object sender, EventArgs e)
        {
            if (!IsDragging)
            {
                SeekBar.Value = FilmPlayer.Position.TotalMilliseconds;
            }
        }

        private void seekBar_DragStarted(object sender, DragStartedEventArgs e)
        {
            IsDragging = true;
        }

        private void seekBar_DragComplite(object sender, DragCompletedEventArgs e)
        {
            IsDragging = false;
            FilmPlayer.Position = TimeSpan.FromMilliseconds(SeekBar.Value);            
            
        }

        private void MoveToBtn_Click(object sender, RoutedEventArgs e)
        {
            string NewFolder = Utils.SelectFolderDlg;                        
            var Exten = System.IO.Path.GetExtension(FilePath);
            string NewPath = string.Concat(NewFolder, @"\", FilmName.Text, Exten);
            Utils.MoveFile(FilePath, NewPath);
            FileStatus(NewPath);
        }

        

        private void ChangePathBtn_Click(object sender, RoutedEventArgs e)
        {
            string NewFileName = FilePathArray;
            if (NewFileName != null)
            {                
                FilmName.Text = System.IO.Path.GetFileNameWithoutExtension(NewFileName);                
                FileStatus(NewFileName);                
            }
        }

        public string FilePathArray
        {
            get
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();                
                dlg.Filter = "Video Files|*.avi;*.mpg;*.flv;*.wmv;*.mp4;*.mov|All Files|*.*";                
                dlg.ShowDialog();
                return dlg.FileName;
            }
        }

        private void FilmItemOk_Click(object sender, RoutedEventArgs e)
        {
            FilmFile NewFilm = new FilmFile();
            NewFilm.ID = FilmID;            
            NewFilm.IsCrypted = IsCrypted;
            NewFilm.IsOnline = IsOnline;
            if (IsOnline)
            {
                if (!Utils.IsStringUriValid(FilmName.Text))
                {
                    Utils.ShowErrorDialog("URl s not valid, type enother one");
                    return;
                }
                NewFilm.Path = FilmName.Text;
                NewFilm.Name = FilmName.Text.Substring(FilmName.Text.LastIndexOf("/") + 1);                
            }
            else
            {
                NewFilm.Name = FilmName.Text;
                NewFilm.Path = FilePath;
            }                        
            NewFilm.Hint = FilmHint.Text;
            NewFilm.Rate = Convert.ToInt16(FilmRate.Value);
            if ((bool)DelChb.IsChecked)
            {
                NewFilm.Rate = -1;
            }
            if (SelectedCategoris != null)
            {
                int[] CategoryIDsArray = new int[SelectedCategoris.Count];
                int cnt = 0;
                foreach (CategoryClass categoryClass in SelectedCategoris)
                {
                    CategoryIDsArray[cnt] = categoryClass.ID;
                    cnt++;
                }
                NewFilm.Categoris = CategoryIDsArray;
            }
            FilmFilesList filmFilesList = new FilmFilesList();
            filmFilesList.AddSaveFilmItemToXML(NewFilm, false);
            //Window win = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Title == "FilmItem");
            Close();
        }

        private void FilmItemCancel_Click(object sender, RoutedEventArgs e)
        {            
            Close();
        }

        private void FilmItemWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        } 
       
        private void LinkedFilmDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LinkedFilmDataGrid.SelectedItem == null)
            {
                return;
            }
            FilmFile SelectedFilm = (FilmFile)LinkedFilmDataGrid.SelectedItem;
            if (e.ChangedButton == MouseButton.Left)
            {
                if (Utils.IsFileExists(SelectedFilm.Path))
                {
                    System.Diagnostics.Process.Start(SelectedFilm.Path);
                }
                else
                {
                    Utils.ShowWarningDialog("Link is corrupted");
                }
            }           
        }

        public bool IsLinkTabSelectedFirstTime{get;set;}

        private void TabSel_Change(object sender, SelectionChangedEventArgs e)
        {
            UpdateLinksTab();
        }

        private void UpdateLinksTab()
        {
            if (Links.IsSelected && IsLinkTabSelectedFirstTime)
            {
                IsLinkTabSelectedFirstTime = false;
                LinksList linksList = new LinksList();
                XElement[] ArrayOfFilms = linksList.GetLinkedFilm(FilmID);
                IList<FilmFile> LinkedFilms = new List<FilmFile> { };
                foreach (XElement Film in ArrayOfFilms)
                {
                    Guid LinkedFilmID = new Guid(Film.LastAttribute.Value);
                    FilmFilesList filmFilesList = new FilmFilesList();
                    FilmFile filmFile = filmFilesList.GetFilmByID(LinkedFilmID);
                    LinkedFilms.Add(filmFile);
                }
                LinkedFilmDataGrid.ItemsSource = LinkedFilms;
                //FilmScreenShot ScreenClass = new FilmScreenShot();
                XMLEdit xMLEdit = new XMLEdit();
                IList<ScreenShotItem> ScreenShotItems = xMLEdit.GetScreenShootsByFilmFile(FilmPublic); //ScreenClass.GetScreenShotsByFilmID(FilmID);
                FilmScreens.Children.Clear();
                int ColumnPosition = 0;
                foreach (ScreenShotItem ScreenItem in ScreenShotItems)
                {
                    Image img = Utils.GetImageByBase64Str(ScreenItem.Base64String);
                    img.Tag = ScreenItem.ScreenShotID;
                    img.Height = 150;
                    img.Width = 170;
                    img.MouseLeftButtonUp += ImageSelected;
                    img.MouseEnter += MouseOverImage;
                    img.MouseLeave += MouseLeftImage;
                    img.Margin = new Thickness(3, 5, 1, 1);
                    FilmScreens.Children.Add(img);                    
                    double RowPosition = Math.Ceiling((ScreenShotItems.IndexOf(ScreenItem) + 1) / 5.0) - 1;
                    Canvas.SetTop(img, 150 * RowPosition);
                    if (ColumnPosition == 5) ColumnPosition = 0;                    
                    Canvas.SetLeft(img, 180 * ColumnPosition);
                    ColumnPosition++;

                }
                double RowsCount = Math.Ceiling(ScreenShotItems.Count / 5.0);
                FilmScreensBorder.Margin = new Thickness(0, 315 - 150 * (RowsCount - 1), 5, 0);
                FilmScreensBorder.Height = 150 * RowsCount;
            }            
        }

        private void MouseOverImage(object sender, RoutedEventArgs e)
        {
            Uri CurUri = new Uri("pack://application:,,,/Resources/ImgDel.cur", UriKind.RelativeOrAbsolute);
            StreamResourceInfo sri = Application.GetResourceStream(CurUri);
            Cursor customCursor = new Cursor(sri.Stream);
            this.Cursor = customCursor;            
        }

        private void MouseLeftImage(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void ImageSelected(object sender, RoutedEventArgs e)        
        {
            Image img = (Image)sender;
            string ScreenShotID = (string)img.Tag;
            FilmScreenShot filmScreenShot = new FilmScreenShot();
            XMLEdit xMLEdit = new XMLEdit();
            xMLEdit.DelScreenShotByID(ScreenShotID);
            //filmScreenShot.DelScreenShotByID(ScreenShotID);
            IsLinkTabSelectedFirstTime = true;
            UpdateLinksTab();            
        }

        private void CryptFile_Click(object sender, RoutedEventArgs e)
        {
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted; 
            if (IsCrypted)
            {
                Utils.ShowWarningDialog("File allready crypted!!!");
                return;
            }
            NewFileName = Utils.GenerateCryptFilePath(FilePath);
            Utils.ShowLoadingWindow("Encoding...", FilePath);
            worker.RunWorkerAsync();          
        }        

        private readonly BackgroundWorker worker = new BackgroundWorker();

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run all background tasks here                 
            try
            {
                Utils.EncryptFile(FilePath, NewFileName);
            }
            catch (OutOfMemoryException err)
            {
                Utils.ShowErrorDialog(string.Concat("Not enough memory, close unused programs and try again ", err.Source));
                return;
            }
            
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //update ui once worker complete his work            
            Window LoadingWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Title == "wnd_Loading");
            if (LoadingWindow != null)
            {
                LoadingWindow.Close();
            }

            if (Utils.IsFileExists(NewFileName))
            {
                FileInfo f = new FileInfo(NewFileName);
                if (f.Length == 0)
                {
                    Utils.ShowErrorDialog("File crypt error, try againe");
                    return;
                }
                else
                {
                    if (Utils.ShowYesNoDialog("Delete origin file")) Utils.DeleteFile(FilePath);
                    IsCrypted = true;
                    FileStatus(NewFileName);
                }
            }
        }

        private void FilmItemsWnd_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double WinHeight = e.NewSize.Height;
            double WinWidth = e.NewSize.Width;
            MainTabControl.Height = WinHeight - 5;
            MainTabControl.Width = WinWidth - 5;
            FilmName.Width = WinWidth - 240;
            CategoryGrid.Height = WinHeight - 235;  
            Canvas.SetTop(FilmItemStatusBar, WinHeight - 95);
            FilmItemStatusBar.Width = WinWidth - 27;
            
            Canvas.SetTop(CryptFile, WinHeight - 126);
            Canvas.SetTop(Complete_Decrypt, WinHeight - 126);            

            Canvas.SetTop(FilmRate, WinHeight - 129);            
            
            Canvas.SetTop(DelChb, WinHeight - 126);
            Canvas.SetLeft(DelChb, WinWidth - 322);

            Canvas.SetTop(Btn_Ok, WinHeight - 126);
            Canvas.SetLeft(Btn_Ok, WinWidth - 210);

            Canvas.SetTop(Btn_Cancel, WinHeight - 126);
            Canvas.SetLeft(Btn_Cancel, WinWidth - 112);  
         
            Canvas.SetTop(FilmHint, WinHeight - 171);    
            FilmHint.Width =  WinWidth - 47;
            
            SellCategoris.Height = WinHeight - 235;           
 
            MovieBorder.Height = WinHeight - 216;
            MovieBorder.Width = WinWidth - 527;
            
            var FilmBrowser = this.FindName("FilmBrowser");
            if (FilmBrowser is WebBrowser)
            {
                WebBrowser filmBrowser = (WebBrowser)FilmBrowser;
                filmBrowser.Height = WinHeight - 231;
                filmBrowser.Width = WinWidth - 542;                     
            }
            //Height="560" Width="915"
            FilmPlayer.Height = WinHeight - 271;
            FilmPlayer.Width = WinWidth - 553; 

            Canvas.SetTop(Btn_Play, WinHeight - 209);
            Canvas.SetTop(Btn_Screen, WinHeight - 209);

            Canvas.SetTop(SeekBar, WinHeight - 209);            
            SeekBar.Width = WinWidth -730; //!!!

            Canvas.SetTop(VolumeBar, WinHeight - 209);
            VolumeBar.Width = WinWidth - 840;

            Canvas.SetLeft(MoveToBtn, WinWidth - 210);

            Canvas.SetLeft(ChangePathBtn, WinWidth - 112);            
        }             

        private void btnScreenShot_Click(object sender, RoutedEventArgs e)
        {
            byte[] screenshot = GetScreenShot(FilmPlayer, 0.5, 90);
            XMLEdit xMLEdit = new XMLEdit();
            xMLEdit.AddScreenShotToXML(screenshot, FilmID);
            //FilmScreenShot FilmScreenShotClass = new FilmScreenShot();
            //FilmScreenShotClass.SaveScreenShotToXML(screenshot, FilmID);            
        }
        
        public byte[] GetScreenShot(UIElement source, double scale, int quality)
        {
            double actualHeight = source.RenderSize.Height;
            double actualWidth = source.RenderSize.Width;
            double renderHeight = actualHeight * scale;
            double renderWidth = actualWidth * scale;

            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)renderWidth,
                (int)renderHeight, 96, 96, PixelFormats.Pbgra32);
            VisualBrush sourceBrush = new VisualBrush(source);

            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            using (drawingContext)
            {
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0),
                    new Point(actualWidth, actualHeight)));
            }
            renderTarget.Render(drawingVisual);

            JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
            jpgEncoder.QualityLevel = quality;
            jpgEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

            Byte[] imageArray;

            using (MemoryStream outputStream = new MemoryStream())
            {
                jpgEncoder.Save(outputStream);
                imageArray = outputStream.ToArray();
            }
            return imageArray;
        }

        private void CompleteDecrypt_Click(object sender, RoutedEventArgs e)
        {
            string NewFileName = Utils.GenerateTempFilePath(FilePath);
            PassWnd passWnd = new PassWnd(FilePath, NewFileName);
            passWnd.ShowDialog();
            if (Utils.IsFileExists(NewFileName))
            {
                Utils.DeleteFile(FilePath);                                
                IsCrypted = false;
                FilePath = NewFileName;
                FileStatus(NewFileName);
                List<string> openedCryptedFiles = MainWindow.OpenedCryptedFiles;
                openedCryptedFiles.Remove(NewFileName);                
            }            
        }

        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FilmPlayer.Volume = (double)VolumeBar.Value;
        }      
    }
}
