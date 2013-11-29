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
using System.Drawing;
using System.IO;




namespace Dobrofilm
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class CategoryItem : Window
    {
        public CategoryItem(CategoryClass ExistingCategotyItem)
        {
            InitializeComponent();           
            CategoryName.Text = ExistingCategotyItem.Name;
            CategoryHint.Text = ExistingCategotyItem.Hint;
            _id = ExistingCategotyItem.ID;
            if (ExistingCategotyItem.Icon.Length > 0)
            {
                CategoryIcon.Source = Utils.DecodePhoto(ExistingCategotyItem.Icon);
            }            
        }

        public CategoryItem() 
        {
            InitializeComponent();
            CategoryIcon.Source = Utils.ConvertBitmapToBitmapImage(Dobrofilm.Properties.Resources.BaseImage);
            _id = 0;
        }

        private int _id {get; set; }
        
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryName.Text.Length == 0 || string.Equals(CategoryName.Text, "Name")) 
            {
                Utils.ShowWarningDialog("Укажите имя Категории");
                return;
            }
            CategoryList categoryList = new CategoryList();
            BitmapImage imgSource = CategoryIcon.Source as BitmapImage;
            byte[] ImageBytes;
            if (imgSource != null)
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(imgSource));
                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    ImageBytes = ms.ToArray();
                }
            }
            else
            {
                ImageBytes = new byte[0];
            }
            categoryList.AddCategory(new CategoryClass { Name = CategoryName.Text, Hint = CategoryHint.Text, Icon = ImageBytes, ID = _id});
            CloseWindow();
        }

        private void CloseWindow()
        {
            Window win = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Title == "Category");
            win.Close();            
        }

        private void Insert_Image(object sender, MouseButtonEventArgs e)
        {         
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Image |*.jpg";
            dlg.Multiselect = false;
            dlg.ShowDialog();
            if (dlg.FileName.Length > 0)
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(dlg.FileName, UriKind.RelativeOrAbsolute);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                CategoryIcon.Source = src;
                CategoryIcon.Stretch = Stretch.Uniform;          
                //Image resized = ResizeImage(original, new Size(1024, 768));
                //MemoryStream memStream = new MemoryStream();
                //resized.Save(memStream, ImageFormat.Jpeg);                
            }
        }

        public BitmapSource ByteToBitmapSource(byte[] image)
        {
            BitmapImage imageSource = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(image))
            {
                stream.Seek(0, SeekOrigin.Begin);
                imageSource.BeginInit();
                imageSource.StreamSource = stream;
                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                imageSource.EndInit();
            }
            return imageSource;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();            
        }
     
        private void CategoryItemWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }
    }
}


