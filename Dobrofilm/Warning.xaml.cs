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

namespace Dobrofilm
{
    /// <summary>
    /// Interaction logic for Warning.xaml
    /// </summary>
    public partial class Warning : Window
    {
        public Warning(string Message, string AdditionalInfo, MessageType Type)
        {
            InitializeComponent();
            this.Height = 177;
            Canvas.SetBottom(BtnBorder, 1);
            System.Drawing.Bitmap TypeIconBitmap;
            MainText.Text = Message;
            AddText.Text = AdditionalInfo;
            AddText.Visibility = System.Windows.Visibility.Collapsed;
            AddInfo_btn.Visibility  = (AdditionalInfo != string.Empty) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            switch (Type)
            {
                case MessageType.Warning:
                    TypeIconBitmap = Dobrofilm.Properties.Resources.Warning;
                    TypeIcon.Source = Utils.ConvertBitmapToBitmapImage(TypeIconBitmap);
                    break;
                case MessageType.Error:
                    TypeIconBitmap = Dobrofilm.Properties.Resources.Error;
                    TypeIcon.Source = Utils.ConvertBitmapToBitmapImage(TypeIconBitmap);
                    break;
            }
        }

        private void AddInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AddText.Visibility == System.Windows.Visibility.Visible)
            {
                this.Height = 177;
                AddText.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this.Height = 342;
                AddText.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = true;	
            Close();

        }
        
    }
}
