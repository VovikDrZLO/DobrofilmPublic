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
    /// Interaction logic for FilmCategoryFilterWindow.xaml
    /// </summary>
    
    public class CategoryTag
    {
        public int CategoryID { get; set; }
        public bool IsClicked { get; set; }
        public FormatConvertedBitmap GrayIcon { get; set; }
        //public BitmapImage GrayIcon { get; set; }
        public BitmapImage OriginIcon { get; set; }
        public CategoryClass categoryClass { get; set; }
    } 

    public partial class FilmCategoryFilterWindow : Window
    {
        public FilmCategoryFilterWindow()
        {
            InitializeComponent();
            CreateCategoryButtonsGrid();
        }

        public int[] SelectedCategorisIDArray { get; set; }

        public void CreateCategoryButtonsGrid()
        {
            CategoryList categoryList = new CategoryList();
            int RowCount = 0;
            int ColumnCount = 0;
            foreach (CategoryClass categoryClass in categoryList.Category)
            {
                if (categoryClass.ID != -1)
                {
                    if (categoryClass.Icon.Length != 0)
                    {
                        Button CategoryButton = new Button();
                        Image img = new Image();

                        //FormatConvertedBitmap grayBitmap = GetCategoryGrayBitmap(categoryClass.Icon);                    
                        var grayBitmap = categoryList.GetGrayCategoryBitmapImageByID(categoryClass.ID);

                        CategoryButton.Tag = new CategoryTag { CategoryID = categoryClass.ID, IsClicked = false, GrayIcon = grayBitmap };
                        img.Source = grayBitmap;
                        CategoryButton.Content = img;
                        CategoryButton.Click += new RoutedEventHandler(CategoryButtomClick);
                        CategoryIconsGrid.Children.Add(CategoryButton);
                        Grid.SetRow(CategoryButton, RowCount);
                        Grid.SetColumn(CategoryButton, ColumnCount);
                        ColumnCount++;
                        if (ColumnCount > 4)
                        {
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
                int CategoryItemID = categoryTag.CategoryID;
                bool IsClicked = categoryTag.IsClicked;
                //FormatConvertedBitmap GrayIcon = categoryTag.GrayIcon;
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
                        BitmapImage OriginCategoryIcon = categoryList.GetCategoryBitmapImageByID(CategoryItemID);
                        img.Source = OriginCategoryIcon;
                        categoryTag.OriginIcon = OriginCategoryIcon;
                    }
                    int[] CategoryItemsArray = SelectedCategorisIDArray;
                    List<int> CategoryItemsList = new List<int>();
                    if (CategoryItemsArray != null)
                    {
                        foreach (int CatID in CategoryItemsArray)
                        {
                            CategoryItemsList.Add(CatID);
                        }
                    }
                    CategoryItemsList.Add(CategoryItemID);
                    SelectedCategorisIDArray = CategoryItemsList.ToArray();
                }
                else
                {
                    img.Source = GrayIcon;
                    int[] CategoryItemsArray = SelectedCategorisIDArray;
                    CategoryItemsArray = CategoryItemsArray.Where(val => val != CategoryItemID).ToArray();
                    SelectedCategorisIDArray = CategoryItemsArray;
                }
                categoryTag.IsClicked = IsClicked;
            }
        }

        public FormatConvertedBitmap GetCategoryGrayBitmap(byte [] CategoryIconByteArray)
        {
            FormatConvertedBitmap grayBitmap = new FormatConvertedBitmap();
            grayBitmap.BeginInit();
            grayBitmap.Source = Utils.DecodePhoto(CategoryIconByteArray);
            grayBitmap.DestinationFormat = PixelFormats.Gray8;
            grayBitmap.EndInit();
            return grayBitmap;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCategorisIDArray != null)
            {

                FilmFilesList filmFilesList = new FilmFilesList();
                AndOrEnum LogicOpertion;
                if ((bool)AndRaioButton.IsChecked)
                {
                    LogicOpertion = AndOrEnum.And;
                }
                else
                {
                    LogicOpertion = AndOrEnum.Or;
                }

                var FilteredSource = filmFilesList.GetFilmListByCategory(SelectedCategorisIDArray, LogicOpertion);
                Window MainWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Title == "Dobrofilm");                
                var MainGridData = (DataGrid)MainWindow.FindName("MainGridData");
                MainGridData.ItemsSource = FilteredSource;                
            }
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FilmCategoryFilterWindow1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }
    }
}
