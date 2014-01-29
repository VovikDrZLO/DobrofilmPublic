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
        private MessageType GlobalType { set; get; }

        public Warning(string Message, string AdditionalInfo, MessageType Type)
        {
            InitializeComponent();
            GlobalType = Type;
            this.Height = 177;
            Canvas.SetBottom(BtnBorder, 1);
            System.Drawing.Bitmap TypeIconBitmap;
            MainText.Text = Message;
            AdditionalText.Text = AdditionalInfo;
            AdditionalText.Visibility = System.Windows.Visibility.Collapsed;
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
                case MessageType.Question:
                    Button YesBtn = new Button();
                    YesBtn.Content = "Yes";
                    YesBtn.Margin = new Thickness(370, 2, 100, 2);
                    YesBtn.Click += YesBtn_Click;
                    OkBtn.Content = "No";
                    BtnGrid.Children.Add(YesBtn);
                    TypeIconBitmap = Dobrofilm.Properties.Resources.Question;
                    TypeIcon.Source = Utils.ConvertBitmapToBitmapImage(TypeIconBitmap);
                    break;
            }
        }

        private void AddInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AdditionalText.Visibility == System.Windows.Visibility.Visible)
            {
                this.Height = 177;
                AdditionalText.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this.Height = 342;
                AdditionalText.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalType == MessageType.Question)
            {
                this.DialogResult = false;
            }            
            Close();

        }

        private void YesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalType == MessageType.Question)
            {
                this.DialogResult = true;
            }
            Close();
        }

        private void MainWindow1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape) return;
            if (GlobalType == MessageType.Question)
            {
                this.DialogResult = false;
            }
            Close();
        }

    }
}
