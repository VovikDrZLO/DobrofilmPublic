using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Dobrofilm
{
    public class ProfileClass
    {
        public string Name { get; set; }
        public Guid ProfileID { get; set; }
    }

    class ProfilesList
    {
        public void ShowNewProfileWindow()
        {
            Window ProfileWindow = DrawProfileWindow("");
            ProfileWindow.ShowDialog(); 
        }

        private Window DrawProfileWindow(string EditProfileName)
        {
            Window ProfileWindow = new Window { Height = 150, Width = 300, ResizeMode = ResizeMode.NoResize, Title = "EditProfile" };
            TextBox ProfileName = new TextBox { Width = 175, Name = "ProfileName" };
            if (EditProfileName != string.Empty) ProfileName.Text = EditProfileName;
            Label ProfileNameLabel = new Label { Width = 100, Content = "Profile Name" };
            Button Ok_Btn = new Button { Content = "OK", Width = 70 };
            Button Cancel_Btn = new Button { Content = "Cancel", Width = 70, };
            Ok_Btn.Click += OkBtn_Click;
            Cancel_Btn.Click += CancelBtn_Click;
            Canvas ProfileCanvas = new Canvas();
            //ProfileWindow.RegisterName(ProfileName.Name, ProfileName);
            ProfileCanvas.Children.Add(ProfileName);
            ProfileCanvas.Children.Add(ProfileNameLabel);
            ProfileCanvas.Children.Add(Ok_Btn);
            ProfileCanvas.Children.Add(Cancel_Btn);

            Canvas.SetTop(ProfileNameLabel, 10);
            Canvas.SetLeft(ProfileNameLabel, 2);
            Canvas.SetTop(ProfileName, 10);
            Canvas.SetLeft(ProfileName, 100);
            Canvas.SetLeft(Ok_Btn, 125);
            Canvas.SetLeft(Cancel_Btn, 210);
            Canvas.SetBottom(Ok_Btn, 5);
            Canvas.SetBottom(Cancel_Btn, 5);

            IconBitmapDecoder ibd = new IconBitmapDecoder(

            new Uri(@"pack://application:,,/Resources/MainIcon.ico", UriKind.RelativeOrAbsolute),
                BitmapCreateOptions.None, BitmapCacheOption.Default);
            ProfileWindow.Icon = ibd.Frames[0];
            ProfileWindow.Content = ProfileCanvas;
            return ProfileWindow;
        }
        public ProfileClass SelectedProfile;


        public void ShowNewProfileWindow(ProfileClass Profile)
        {
            SelectedProfile = Profile;
            Window ProfileWindow = DrawProfileWindow(Profile.Name);
            ProfileWindow.ShowDialog(); 
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            Window ProfileWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Title == "EditProfile");
            Canvas ProfileCanvas = (Canvas)ProfileWindow.Content;
            foreach (UIElement child in ProfileCanvas.Children)
            {
                if (child is TextBox)
                {
                    TextBox txt = (TextBox)child;
                    if (AddProfile(txt.Text)) ProfileWindow.Close(); else Utils.ShowErrorDialog("Enter profile name");
                }
            }                        
        }

        private bool AddProfile(string ProfileName)
        {            
            if (ProfileName == string.Empty) return false;
            XMLEdit xMLEdit = new XMLEdit();
            if (SelectedProfile == null)
            {
                xMLEdit.AddProfileToXML(new ProfileClass { Name = ProfileName, ProfileID = Guid.Empty });
            }
            else
            {
                xMLEdit.AddProfileToXML(new ProfileClass { Name = ProfileName, ProfileID = SelectedProfile.ProfileID });
            }            
            return true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Window ProfileWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Title == "EditProfile");
            ProfileWindow.Close();
        }
    }
}
