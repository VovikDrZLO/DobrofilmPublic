﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Windows.Data;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;


namespace Dobrofilm
{
    public class FilmFilesList
    {        
        public ListCollectionView FilmFiles_v1
        {
            get
            {
                {
                    IList<FilmFile> _films = new List<FilmFile> { };
                    XmlDocument FilmXml = new XmlDocument();
                    FilmXml.Load(FileListPath);
                    XmlNodeReader reader = new XmlNodeReader(FilmXml);
                    FilmFile FilmFileClass = new FilmFile();
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Name == "file")
                                {
                                    try
                                    {
                                        FilmFileClass.ID = new Guid(reader.GetAttribute("GUID"));
                                    }
                                    catch
                                    {
                                        FilmFileClass.ID = Guid.NewGuid();
                                    }
                                    FilmFileClass.Hint = reader.GetAttribute("hint");
                                    FilmFileClass.Path = reader.GetAttribute("path");
                                    FilmFileClass.Rate = int.Parse(reader.GetAttribute("rate"));
                                    FilmFileClass.IsCrypted = (reader.GetAttribute("isCrypted") == "1");
                                    FilmFileClass.IsOnline = (reader.GetAttribute("isOnline") == "1");
                                    FilmFileClass.IsFTP = (reader.GetAttribute("isFTP") == "1");
                                    FilmFileClass.Categoris = CategorisArray(reader.GetAttribute("categoris"));
                                }
                                break;
                            case XmlNodeType.Text:
                                FilmFileClass.Name = reader.Value;
                                break;
                            /*case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.ProcessingInstruction:
                            case XmlNodeType.Comment:*/
                            case XmlNodeType.EndElement:
                                if (reader.Name == "file")
                                {
                                    if (ShowCryptFilms || !FilmFileClass.IsCrypted) _films.Add(FilmFileClass);
                                    FilmFileClass = new FilmFile();
                                }
                                break;
                        }

                    }                    
                    //XMLEdit xMLEdit = new XMLEdit();
                    //IList<FilmFile> _films = xMLEdit.GetFilmFileFromXML(ShowCryptFilms);
                    var FilmFilesList = (ListCollectionView)CollectionViewSource.GetDefaultView(_films);
                    return FilmFilesList;
                }
            }
        }

        public ListCollectionView FilmFiles
        {
            get
            {
                XMLEdit xMLEdit = new XMLEdit();
                IList<FilmFile> _films = xMLEdit.GetFilmFileFromXML(ShowCryptFilms, MainWindow.CurrentProfile, false);
                var FilmFilesList = (ListCollectionView)CollectionViewSource.GetDefaultView(_films);
                return FilmFilesList;
            }
        }

        public static bool ShowCryptFilms { get; set; }

        public void AddEncryptedFilmsToMainCollection()
        {

        }

        public string FileListPath
        {
            get 
            {
                string FilmListPath = Dobrofilm.Properties.Settings.Default.FilmListXMLFile;
                if (Utils.ValidateSettings(FilmListPath, XMLFile.Files))
                {
                    return FilmListPath;
                }
                //if (Utils.IsFileExists(FilmListPath))
                //{
                //    return FilmListPath;
                //}
                bool DialogResult = Utils.ShowYesNoDialog("To select existing film files xml library press YES press NO to create it automatically");
                if (!!DialogResult)
                {
                    bool IsInvalid = true;
                    while (IsInvalid)
                    {
                        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                        dlg.DefaultExt = ".xml";
                        dlg.Filter = "XML Files|*.xml";
                        dlg.Multiselect = false;
                        dlg.ShowDialog();
                        if (Utils.ValidateSettings(dlg.FileName, XMLFile.Files))
                        {
                            IsInvalid = false;
                            Dobrofilm.Properties.Settings.Default.FilmListXMLFile = dlg.FileName;
                            Dobrofilm.Properties.Settings.Default.Save();
                            return dlg.FileName;
                        }
                        else
                        {
                            Utils.ShowWarningDialog("Select Enother FilmList File");
                        }
                    }                    
                }
                //string NewFilePath = CreateNewFilmListXML();
                //Dobrofilm.Properties.Settings.Default.FilmListXMLFile = NewFilePath;
                //Dobrofilm.Properties.Settings.Default.Save();
                return string.Empty; 
            }
        }

        //public string CreateNewFilmListXML()
        //{
        //    string DirPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\Dobrofilm");
        //    Utils.CreateDirectory(DirPath);
        //    string path = string.Concat(DirPath, "\\", "FilmList.xml"); //Directory.GetCurrentDirectory()            
        //    using (XmlWriter writer = XmlWriter.Create(path))
        //    {
        //        writer.WriteStartDocument();
        //        writer.WriteStartElement("files");
        //        writer.WriteStartAttribute("filemask");
        //        writer.WriteString("Dobrofilm");
        //        writer.WriteEndAttribute();
        //        writer.WriteStartAttribute("nextnumber");
        //        writer.WriteString("1");
        //        writer.WriteEndAttribute();
        //        writer.WriteEndElement();
        //        writer.WriteEndDocument();
        //    }
        //    return path;
        //}

        public void AddFilmsToFilmFiles()
        {            
            int[] CategorisStartArray = new int[1];
            CategorisStartArray[0] = 0;
            bool DialogResult = false;
            bool DialogShow = false;
            foreach (string Filepath in FilePathArray)
            {
                string FilmName = System.IO.Path.GetFileNameWithoutExtension(Filepath);
                string FilmExt = System.IO.Path.GetExtension(Filepath);
                if (!IsFileInLibrary(Filepath))
                {
                    if (!DialogShow)
                    {
                        DialogResult = Utils.ShowYesNoDialog("Rename Files?");
                        DialogShow = true;
                    }
                    XMLEdit xMLEdit = new XMLEdit();
                    xMLEdit.AddFilmToXML(new FilmFile
                    {
                        Name = FilmName,
                        Path = Filepath,
                        Rate = 0,
                        Categoris = CategorisStartArray,
                        IsCrypted = (FilmExt.EndsWith("CrypDobFilm")),
                        IsOnline = false,
                        IsFTP = false,
                        Profile = MainWindow.CurrentProfile.ProfileID
                    }, DialogResult);
                    //AddSaveFilmItemToXML(new FilmFile
                    //{
                    //    Name = FilmName,
                    //    Path = Filepath,
                    //    Rate = 0,
                    //    Categoris = CategorisStartArray,
                    //    IsCrypted = false,
                    //    IsOnline  = false
                    //}, DialogResult);
                }
                else
                {
                    string Message = String.Format("File {0} allready exists", FilmName);
                    Utils.ShowWarningDialog(Message);
                }
            } 
        }

        public bool IsFileInLibrary(string FilePath)
        {
            if (FilePath == null)
            {
                return false;
            }
            foreach (FilmFile filmFile in FilmFiles)
            {
                if (filmFile.Path == FilePath)
                {
                    return true;
                }
            }
            return false;
        }

        public string[] FilePathArray
        {
            get
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();                
                dlg.DefaultExt = ".avi";
                dlg.Filter = "Video Files|*.avi;*.mpg;*.flv;*.wmv;*.mp4;*.mov;*.aviCrypDobFilm;*.mpgCrypDobFilm;*.flvCrypDobFilm;*.wmvCrypDobFilm;*.mp4CrypDobFilm;*.movCrypDobFilm|All Files|*.*";
                dlg.Multiselect = true;                
                dlg.ShowDialog();
                bool IncludeSubFolders = Utils.ShowYesNoDialog("Include subfolders?");
                if (IncludeSubFolders)
                {
                    string Dir = System.IO.Path.GetDirectoryName(dlg.FileName);
                    DirectoryInfo dInfo = new DirectoryInfo(Dir);
                    DirectoryInfo[] subdirs = dInfo.GetDirectories();
                    var allfiles = Directory.GetFiles(Dir, "*.*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".mp4") || s.EndsWith(".wmv") || s.EndsWith(".avi") || s.EndsWith(".flv") || s.EndsWith(".mpg") || s.EndsWith(".mov") || s.EndsWith("CrypDobFilm"));
                    string[] AllFilesArray = allfiles.ToArray();
                    return AllFilesArray;
                }
                return dlg.FileNames;
            }
        }      

        public int[] CategorisArray(string CategorisString) 
        {           
            string[] CategorisStringArray = CategorisString.Split(':');
            int[] ResultArray = new int[CategorisStringArray.Length];
            for (int i = 0; i < CategorisStringArray.Length; i++)
            {
                ResultArray[i] = int.Parse(CategorisStringArray[i]);
            }
            return ResultArray;
        }

       
        //public void AddSaveFilmItemToXML(FilmFile FilmItem, bool RenameFiles)
        //{            
        //    if (FilmItem == null)
        //    {
        //        Utils.ShowWarningDialog("Saving Error");
        //        return;
        //    }
        //    XDocument FilmX = XDocument.Load(FileListPath);
        //    if (RenameFiles)
        //    {
        //        string NewFileMask = FilmX.Element("files").Attribute("filemask").Value;
        //        int FileNumber = int.Parse(FilmX.Element("files").Attribute("nextnumber").Value);
        //        if (NewFileMask != null || FileNumber != 0)
        //        {
        //            string NewFileName = NewFileMask + FileNumber;
        //            string NewFilePath = Utils.RenameFile(FilmItem.Path, NewFileName);
        //            FilmItem.Hint = FilmItem.Name;
        //            FilmItem.Name = NewFileName;
        //            FilmItem.Path = NewFilePath;
        //            FilmX.Element("files").Attribute("nextnumber").Value = Convert.ToString(FileNumber + 1);
        //        }
        //    }
        //    XMLEdit xMLEdit = new XMLEdit();
        //    XElement FilmXElement = xMLEdit.CreateFilmXElement(FilmItem);
        //    if (FilmItem.ID == System.Guid.Empty)
        //    {
        //        FilmX.Element("files").Add(FilmXElement);
        //    }
        //    else
        //    {
        //        var FilmToChange =
        //            (from p in FilmX.Descendants("file")
        //             where new Guid(p.Attribute("GUID").Value) == FilmItem.ID
        //             select p).Single();
        //        FilmToChange.ReplaceWith(FilmXElement);  
        //    }
        //    FilmX.Save(FileListPath);
        //}        

        public ListCollectionView GetFilmListByCategory(int[] CategotisLocalArray, AndOrEnum LogicOperation)
        {
            if (CategotisLocalArray.Length == 0)
            {
                return FilmFiles;
            }
            
            CategorisIDArray = CategotisLocalArray;
            ChosenLogicType = LogicOperation;
            ListCollectionView FilteredFileList = FilmFiles;
            FilteredFileList.Filter = new Predicate<object>(Contains);
            return FilteredFileList;
        }

        public FilmFile GetFilmByID(Guid FilmID)
        {
            if (FilmID == null || FilmID == Guid.Empty)
            {
                return new FilmFile();
            }
            SFilmID = FilmID;
            ListCollectionView FilteredFileList = FilmFiles;
            FilteredFileList.Filter = new Predicate<object>(FilmByID);
            return (FilmFile)FilteredFileList.CurrentItem;            
        }

        public bool FilmByID(object de)
        {
            FilmFile Film = de as FilmFile;
            return Film.ID == SFilmID;
        }

        public AndOrEnum ChosenLogicType { get; set; }
        public int[] CategorisIDArray { get; set; }
        public Guid SFilmID { get; set; }
        

        public bool Contains(object de)
        {
            FilmFile order = de as FilmFile;
            //TODO обработать условие AND
            if (ChosenLogicType == AndOrEnum.And)
            {
                return IsArrayValuesInArray(order.Categoris, CategorisIDArray);
            }
            else
            {
                foreach (int CategoryID in CategorisIDArray)
                {
                    if (Array.IndexOf(order.Categoris, CategoryID) >= 0)
                    {
                        return true;
                    }
                }
            }
            
        return false;
        }

        public ListCollectionView GetFilmListByProfile(ProfileClass Profile)
        {
            if (Profile == null) return FilmFiles;
            ListCollectionView FilteredFileList = FilmFiles;
            MainWindow.CurrentProfile = Profile;
            FilteredFileList.Filter = new Predicate<object>(CurProfile);
            return FilteredFileList;
        }

        public bool CurProfile(object de)
        {
            FilmFile Film = de as FilmFile;
            if (Film.Profile == MainWindow.CurrentProfile.ProfileID)
            {
                return true;
            }            
            return false;
        }

        public bool IsArrayValuesInArray(int[] SoursArray, int[] FindArray)
        {
            if (FindArray.Length == 0)
            {
                return false;
            }
            int cnt = 0;
            foreach (int FindValue in FindArray)
            {
                if (Array.IndexOf(SoursArray, FindValue) >= 0)
                {
                    cnt++;
                }
            }
            return (FindArray.Length == cnt);
        }
        
        //public void DeleteFilmItemFromXml(FilmFile FilmItem)
        //{
        //    XDocument FilmX = XDocument.Load(FileListPath);            
        //    var FilmToDelete =
        //            (from p in FilmX.Descendants("file")
        //             where new Guid(p.Attribute("GUID").Value) == FilmItem.ID
        //             select p).Single();
        //    FilmToDelete.Remove();
        //    FilmX.Save(FileListPath);
        //}

    }

    public class FilmFile
    {        
        public string Name { get; set; }
        public string Path { get; set; }
        public string Hint { get; set; }
        public int Rate { get; set; }
        public Guid ID { get; set; }
        public int[] Categoris { get; set; }
        public bool IsCheked { get; set; }
        public bool IsCrypted { get; set; }
        public bool IsOnline { get; set; }
        public bool IsFTP { get; set; }
        public Guid Profile { get; set; }
        public XElement filmsScr { get; set; }
        public XElement links { get; set; }        
    }


}
