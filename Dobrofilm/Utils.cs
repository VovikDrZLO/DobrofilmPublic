using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;

namespace Dobrofilm
{
    public enum AndOrEnum {And, Or};
    public enum XMLFile {Files, Categoris, Links, Screens };
    static public class Utils
    {        

        static public void ShowWarningDialog(string Message) 
        {
            string messageBoxText = Message;
            string caption = "Warning";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBox.Show(messageBoxText, caption, button, icon);
        }

        static public void ShowErrorDialog(string Message)
        {
            string messageBoxText = Message;
            string caption = "Error";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBox.Show(messageBoxText, caption, button, icon);
        }

         static public string RenameFile(string FilePath, string NewName)
         {
             if (!File.Exists(FilePath))
             {
                 return FilePath;
             }
             string NewFilePath = GhangeFileNameInPath(FilePath, NewName);
             File.Move(FilePath, NewFilePath);
             return NewFilePath;
         }

         static public bool ShowYesNoDialog(string Message)
         {
             string messageBoxText = Message;
             string caption = "Question";
             MessageBoxButton button = MessageBoxButton.YesNo;
             MessageBoxImage icon = MessageBoxImage.Warning;
             return (MessageBox.Show(messageBoxText, caption, button, icon) == MessageBoxResult.Yes);             
         }

         static public string GhangeFileNameInPath(string FilePath, string NewName)
         {
             return Path.GetDirectoryName(FilePath) + @"\" + NewName + Path.GetExtension(FilePath);
         }

         static public BitmapImage DecodePhoto(byte[] value)
         {
             if (value == null) return null;
             byte[] byteme = value as byte[];
             if (byteme == null) return null;
             MemoryStream strmImg = new MemoryStream(byteme);
             BitmapImage myBitmapImage = new BitmapImage();
             myBitmapImage.BeginInit();
             myBitmapImage.StreamSource = strmImg;
             myBitmapImage.DecodePixelWidth = 200;
             myBitmapImage.EndInit();
             return myBitmapImage;
         }

         static public bool IsFileExists(string FilePath)
         {
             if (FilePath == null)
             {
                 return false;
             }
             return File.Exists(FilePath);
         }

         static public void CreateDirectory(string DirName)
         {
             if (!Directory.Exists(DirName))
             {
                 Directory.CreateDirectory(DirName);
             }
         }

         static public BitmapImage ConvertBitmapToBitmapImage(System.Drawing.Bitmap BitmapSource)
         {
             var memoryStream = new MemoryStream();
             BitmapSource.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
             memoryStream.Position = 0;

             var bitmapImage = new BitmapImage();
             bitmapImage.BeginInit();
             bitmapImage.StreamSource = memoryStream;
             bitmapImage.EndInit();

             return bitmapImage;
         }

         static public void DeleteFile(string FilePath)
         {
             if (!IsFileExists(FilePath)) return;             
             ProcessStartInfo processInfo = new ProcessStartInfo();
             processInfo.WindowStyle = ProcessWindowStyle.Hidden;
             processInfo.FileName = "cmd.exe";
             processInfo.Arguments = string.Format("/c del \"{0}\"", FilePath); ;
             Process.Start(processInfo);
         }

         static public bool IsStringUriValid(string Adsress)
         {
             return Uri.IsWellFormedUriString(Adsress, UriKind.Absolute);
         }

         static public string GenerateTempFilePath(string FilePath)
         {
             return FilePath.Substring(0, FilePath.Length - 11);
         }

         static public string GenerateCryptFilePath(string FilePath)
         {
             return string.Concat(System.IO.Path.GetDirectoryName(FilePath), @"\", System.IO.Path.GetFileName(FilePath), "CrypDobFilm");
         }

         static public void ShowLoadingWindow(string MainText, string AddText)
         {
             Window wnd_Loading = new Window { Title = "wnd_Loading", 
                                                WindowStyle = WindowStyle.None, 
                                                Height = 70, Width = 400, 
                                                ResizeMode = ResizeMode.NoResize, 
                                                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                                                Cursor = Cursors.Wait};
             Uri iconUri = new Uri("Resources/MainIcon.ico", UriKind.RelativeOrAbsolute);
             wnd_Loading.Icon = BitmapFrame.Create(iconUri);
             Controls.LoadingPanel loadingPanel = new Controls.LoadingPanel { Message = MainText, SubMessage = AddText, IsLoading = true};             
             StackPanel stackPanel = new StackPanel { Orientation = Orientation.Vertical};
             stackPanel.Children.Add(loadingPanel);
             wnd_Loading.Content = stackPanel;
             wnd_Loading.Show();             
         }

         private const string Salt = "d5fg4df5sg4ds5fg45sdfg4";
         private const int SizeOfBuffer = 1024 * 8;
         private const string Pass = "P@ssw0rd";

         internal static void EncryptFile(string inputPath, string outputPath)
         {
             var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
             var output = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write);

             // Essentially, if you want to use RijndaelManaged as AES you need to make sure that:
             // 1.The block size is set to 128 bits
             // 2.You are not using CFB mode, or if you are the feedback size is also 128 bits

             var algorithm = new RijndaelManaged { KeySize = 256, BlockSize = 128 };
             var key = new Rfc2898DeriveBytes(Pass, Encoding.ASCII.GetBytes(Salt));

             algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
             algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

             using (var encryptedStream = new CryptoStream(output, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
             {
                 CopyStream(input, encryptedStream);
             }
         }

         internal static void DecryptFile(string inputPath, string outputPath, string password)
         {
             var input = new FileStream(inputPath, FileMode.Open, FileAccess.Read);
             var output = new FileStream(outputPath, FileMode.OpenOrCreate, FileAccess.Write);

             // Essentially, if you want to use RijndaelManaged as AES you need to make sure that:
             // 1.The block size is set to 128 bits
             // 2.You are not using CFB mode, or if you are the feedback size is also 128 bits
             var algorithm = new RijndaelManaged { KeySize = 256, BlockSize = 128 };
             var key = new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes(Salt));

             algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
             algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

             try
             {
                 using (var decryptedStream = new CryptoStream(output, algorithm.CreateDecryptor(), CryptoStreamMode.Write))
                 {
                     CopyStream(input, decryptedStream);
                 }
                 if (MainWindow.OpenedCryptedFiles == null)
                 {
                     MainWindow.OpenedCryptedFiles = new List<string>();
                 }
                 MainWindow.OpenedCryptedFiles.Add(outputPath);                    
                 
             }
             catch (CryptographicException err)
             {
                 throw new InvalidDataException("Please supply a correct password " + err.Message);
             }
             catch (Exception ex)
             {
                 throw new Exception(ex.Message);
             }
         }    

         private static void CopyStream(Stream input, Stream output)
         {
             using (output)
             using (input)
             {
                 byte[] buffer = new byte[SizeOfBuffer];
                 int read;
                 while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                 {
                     output.Write(buffer, 0, read);
                 }
             }
         }

         public static Image GetImageByBase64Str(string Base64String)
         {
            if (Base64String == string.Empty)
            {
                return null;
            }             
            Image img = new Image();
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(Base64String);
            }
            catch
            {
                imageBytes = new byte[0];
            }
            MemoryStream strmImg = new MemoryStream(imageBytes);
            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.StreamSource = strmImg;
            myBitmapImage.DecodePixelWidth = 120; //Величина картинки.
            myBitmapImage.EndInit();
            img.Source = myBitmapImage;
            return img;
             
         }

         public static bool ValidateSettings()
         {             
             string MPC = Dobrofilm.Properties.Settings.Default.MPCPath;          
             string DefBrowser = Dobrofilm.Properties.Settings.Default.DefaultBrowser;
             
             //Dobrofilm.Properties.Settings.Default.Save();             
             return true;
         }

         public static bool ValidateSettings(string FileName, XMLFile FileType)
         {
             if (!IsFileExists(FileName) || Path.GetExtension(FileName) != ".xml")
             {
                 return false;
             }
                 XmlDocument ValidDoc = new XmlDocument();
                 ValidDoc.Load(FileName);
                 XmlReaderSettings settings = new XmlReaderSettings();
                 switch (FileType)
                 {
                     case XMLFile.Categoris:
                         settings.Schemas.Add(null, "Resources/CategoryList.xsd");                         
                         break;
                     case XMLFile.Files:
                         
                         settings.Schemas.Add(null, "Resources/FilmList.xsd");
                         break;
                     case XMLFile.Links:
                         settings.Schemas.Add(null, "Resources/LinksList.xsd");
                         break;
                     case XMLFile.Screens:
                         settings.Schemas.Add(null, "Resources/ScreenList.xsd");
                         break;
                 }
                 settings.ValidationType = ValidationType.Schema;
                 XmlReader TryReader = XmlReader.Create(FileName, settings);
                 try
                 {
                     ValidDoc.Load(TryReader);
                 }
                 catch (XmlSchemaValidationException e)
                 {
                     string Message = String.Format("The {0} Validating error; {1}, try enother file", FileType, e.Message);
                     ShowErrorDialog(Message);
                     return false;
                 }
                 finally
                 {
                     TryReader.Close();
                     ValidDoc = null;
                 }
                 return true;                 
         }
    }
}
