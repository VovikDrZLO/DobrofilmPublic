using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
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
    public enum MessageType {Warning, Error, Question};
    static public class Utils
    {        

        static public void ShowWarningDialog(string Message) 
        {
            Warning WarningWnd = new Warning(Message, "", MessageType.Warning);
            WarningWnd.ShowDialog();
            //string messageBoxText = Message;
            //string caption = "Warning";
            //MessageBoxButton button = MessageBoxButton.OK;
            //MessageBoxImage icon = MessageBoxImage.Warning;
            //MessageBox.Show(messageBoxText, caption, button, icon);
        }

        static public void ShowErrorDialog(string Message)
        {
            Warning WarningWnd = new Warning(Message, "", MessageType.Error);
            WarningWnd.ShowDialog();
            //string messageBoxText = Message;
            //string caption = "Error";
            //MessageBoxButton button = MessageBoxButton.OK;
            //MessageBoxImage icon = MessageBoxImage.Error;
            //MessageBox.Show(messageBoxText, caption, button, icon);
        }

        static public void ShowErrorDialog(string Message, string AdditionalInf)
        {
            Warning WarningWnd = new Warning(Message, AdditionalInf, MessageType.Error);
            WarningWnd.ShowDialog();
            
        }

         static public string RenameFile(string FilePath, string NewName)
         {
             if (!Utils.IsFileExists(FilePath))
             {
                 return FilePath;
             }
             string NewFilePath = GhangeFileNameInPath(FilePath, NewName);
             MoveFile(FilePath, NewFilePath);
             return NewFilePath;
         }

         static public bool ShowYesNoDialog(string Message)
         {
             Warning warning = new Warning(Message, "", MessageType.Question);
             warning.ShowDialog();
             bool Result = (bool)warning.DialogResult;
             return Result;                       
         }

         static public bool ShowSimpleYesNoDialog(string Message)
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
             processInfo.Arguments = string.Format("/c del \"{0}\"", FilePath);
             try
             {
                 Process.Start(processInfo);
             }
             catch (Exception ex)
             {
                 Utils.ShowErrorDialog(string.Format("File {0} was not delleted because {1}", FilePath, ex.Message));
             }             
         }

         static public void MoveFile(string FromFilePath, string ToFilePath)
         {
             if (!IsFileExists(FromFilePath) || ToFilePath == string.Empty) return;
             try
             {
                 File.Move(FromFilePath, ToFilePath);
             }
             catch (Exception ex)
             {
                 Utils.ShowErrorDialog(string.Format("File {0} was not moved because {1}", FromFilePath, ex.Message));
             }
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
         private const string Pass = "rbfgIerNu6O0Xuyv24x3+YES2BeN4GRxCrsBa7PERJY= RBVM0g==";

         internal static bool IsPassValid(string Password)
         {
             SaltedHash saltedHash = new SaltedHash();
             return saltedHash.VerifyHashString(Password, Pass);
         }

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

         public static string SelectFolderDlg
         {
             get
             {
                 System.Windows.Forms.FolderBrowserDialog DirDialog = new System.Windows.Forms.FolderBrowserDialog();
                 DirDialog.ShowDialog();
                 return DirDialog.SelectedPath;
             }
         }


         public static long TotalFilmsLength
         {
             get
             {
                 FilmFilesList filmFilesList = new FilmFilesList();
                 ListCollectionView filmFiles = filmFilesList.FilmFiles;
                 long TotalLength = 0;
                 foreach (FilmFile Film in filmFiles)
                 {
                     if (!Film.IsOnline && Utils.IsFileExists(Film.Path))
                     {
                         FileInfo FileInfo = new FileInfo(Film.Path);
                         TotalLength = TotalLength + FileInfo.Length;
                     }
                 }
                 return TotalLength;
             }
         }

    }

    public class SaltedHash
    {
        #region Fields

        /// <summary>
        /// Delimiter character
        /// </summary>
        private string delimiter = " ";

        /// <summary>
        /// Hash Provider
        /// </summary>
        private HashAlgorithm hashProvider;

        /// <summary>
        /// Salth length
        /// </summary>
        private int salthLength;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the SaltedHash class.
        /// </summary>
        /// <param name="hashAlgorithm">A <see cref="HashAlgorithm"/> HashAlgorihm which is derived from HashAlgorithm. C# provides
        /// the following classes: SHA1Managed,SHA256Managed, SHA384Managed, SHA512Managed and MD5CryptoServiceProvider</param>
        /// <param name="theSaltLength">Length in bytes</param>
        public SaltedHash(HashAlgorithm hashAlgorithm, int theSaltLength)
        {
            this.hashProvider = hashAlgorithm;
            this.salthLength = theSaltLength;
        }

        /// <summary>
        /// Initializes a new instance of the SaltedHash class.
        /// </summary>
        public SaltedHash()
            : this(new SHA256Managed(), 4)
        {
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Gets the hashed string with salt
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <returns> The hashed string </returns>
        public string GetHashedString(string data)
        {
            string hash, salt;
            this.GetHashAndSaltString(data, out hash, out salt);
            return hash.Replace(this.delimiter, String.Empty) + this.delimiter + salt.Replace(this.delimiter, string.Empty);
        }

        /// <summary>
        /// Verifies the data and hash
        /// </summary>
        /// <param name="data">The data to compare</param>
        /// <param name="hashedData">The hash to compare with</param>
        /// <returns>Returns bool flag</returns>
        public bool VerifyHashString(string data, string hashedData)
        {
            if (String.IsNullOrEmpty(hashedData))
            {
                return false;
            }

            string[] split = hashedData.Split(this.delimiter.ToCharArray());

            if (split == null || split.Length != 2)
            {
                return false;
            }

            return this.VerifyHashString(data, split[0], split[1]);
        }

        /// <summary>
        /// The actual hash calculation is shared by both GetHashAndSalt and the VerifyHash functions
        /// </summary>
        /// <param name="data">The data byte array</param>
        /// <param name="salt">The salt byte array</param>
        /// <returns>
        /// A byte array with the calculated hash
        /// </returns>
        private byte[] ComputeHash(byte[] data, byte[] salt)
        {
            // Allocate memory to store both the Data and Salt together
            byte[] dataAndSalt = new byte[data.Length + this.salthLength];

            // Copy both the data and salt into the new array
            Array.Copy(data, dataAndSalt, data.Length);
            Array.Copy(salt, 0, dataAndSalt, data.Length, this.salthLength);

            // Calculate the hash
            // Compute hash value of our plain text with appended salt.
            return this.hashProvider.ComputeHash(dataAndSalt);
        }

        /// <summary>
        /// Given a data block this routine returns both a Hash and a Salt
        /// </summary>
        /// <param name="data">The data byte array</param>
        /// <param name="hash">The hash byte array</param>
        /// <param name="salt">The salt byte array</param>
        private void GetHashAndSalt(byte[] data, out byte[] hash, out byte[] salt)
        {
            // Allocate memory for the salt
            salt = new byte[this.salthLength];

            // Strong runtime pseudo-random number generator, on Windows uses CryptAPI
            // on Unix /dev/urandom
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

            // Create a random salt
            random.GetNonZeroBytes(salt);

            // Compute hash value of our data with the salt.
            hash = this.ComputeHash(data, salt);

            random.Dispose();
        }

        /// <summary>
        /// The routine provides a wrapper around the GetHashAndSalt function providing conversion
        /// from the required byte arrays to strings. Both the Hash and Salt are returned as Base-64 encoded strings.
        /// </summary>
        /// <param name="data">The data byte array</param>
        /// <param name="hash">The hash byte array</param>
        /// <param name="salt">The salt byte array</param>
        private void GetHashAndSaltString(string data, out string hash, out string salt)
        {
            byte[] hashOut;
            byte[] saltOut;

            // Obtain the Hash and Salt for the given string
            this.GetHashAndSalt(Encoding.UTF8.GetBytes(data), out hashOut, out saltOut);

            // Transform the byte[] to Base-64 encoded strings
            hash = Convert.ToBase64String(hashOut);
            salt = Convert.ToBase64String(saltOut);
        }

        /// <summary>
        /// This routine verifies whether the data generates the same hash as we had stored previously
        /// </summary>
        /// <param name="data">The data byte array</param>
        /// <param name="hash">The hash byte array</param>
        /// <param name="salt">The salt byte array</param>
        /// <returns>
        /// True on a succesful match
        /// </returns>
        private bool VerifyHash(byte[] data, byte[] hash, byte[] salt)
        {
            byte[] newHash = this.ComputeHash(data, salt);

            //// Compare hash
            if (newHash.Length != hash.Length)
            {
                return false;
            }

            for (int lp = 0; lp < hash.Length; lp++)
            {
                if (!hash[lp].Equals(newHash[lp]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// This routine provides a wrapper around VerifyHash converting the strings containing the
        /// data, hash and salt into byte arrays before calling VerifyHash.
        /// </summary>
        /// <param name="data">The data byte array</param>
        /// <param name="hash">The hash byte array</param>
        /// <param name="salt">The salt byte array</param>
        /// <returns>
        /// Returns bool flag
        /// </returns>
        private bool VerifyHashString(string data, string hash, string salt) 
        {
            byte[] hashToVerify = Convert.FromBase64String(hash);
            byte[] saltToVerify = Convert.FromBase64String(salt);
            byte[] dataToVerify = Encoding.UTF8.GetBytes(data);
            return this.VerifyHash(dataToVerify, hashToVerify, saltToVerify);
        }

        #endregion Methods


    }

    class Simple3Des // fore safe. don't used in project
     {
        TripleDESCryptoServiceProvider tripleDes = new TripleDESCryptoServiceProvider();
 
        public Simple3Des(string key)
         {
         // Initialize the crypto provider.
         tripleDes.Key = TruncateHash(key, tripleDes.KeySize / 8);
         tripleDes.IV = TruncateHash("", tripleDes.BlockSize / 8);
         }
 
        private byte[] TruncateHash(string key, int length)
         {
         SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
         // Hash the key.
         byte[] keyBytes = System.Text.Encoding.Unicode.GetBytes(key);
         byte[] hash = sha1.ComputeHash(keyBytes);
 
        // Truncate or pad the hash.
         Array.Resize(ref hash, length);
         return hash;
         }
 
        public string EncryptData(string plaintext)
         {
 
        // Convert the plaintext string to a byte array.
         byte[] plaintextBytes = System.Text.Encoding.Unicode.GetBytes(plaintext);
 
        // Create the stream.
         System.IO.MemoryStream ms = new System.IO.MemoryStream();
         // Create the encoder to write to the stream.
         CryptoStream encStream = new CryptoStream(ms, tripleDes.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write);
 
        // Use the crypto stream to write the byte array to the stream.
         encStream.Write(plaintextBytes, 0, plaintextBytes.Length);
         encStream.FlushFinalBlock();
 
        // Convert the encrypted stream to a printable string.
         return Convert.ToBase64String(ms.ToArray());
         }
 
        public string DecryptData(string encryptedtext)
         {
 
        // Convert the encrypted text string to a byte array.
         byte[] encryptedBytes = Convert.FromBase64String(encryptedtext);
 
        // Create the stream.
         System.IO.MemoryStream ms = new System.IO.MemoryStream();
         // Create the decoder to write to the stream.
         CryptoStream decStream = new CryptoStream(ms, tripleDes.CreateDecryptor(), System.Security.Cryptography.CryptoStreamMode.Write);
 
        // Use the crypto stream to write the byte array to the stream.
         decStream.Write(encryptedBytes, 0, encryptedBytes.Length);
         decStream.FlushFinalBlock();
 
        // Convert the plaintext stream to a string.
         return System.Text.Encoding.Unicode.GetString(ms.ToArray());
         }
    }
}
