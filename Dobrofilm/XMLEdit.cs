using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Collections;

namespace Dobrofilm
{
    class XMLEdit
    {
        public XDocument SettingsXMLDoc //valid
        {
            get
            {                
                string SettingsPath = Dobrofilm.Properties.Settings.Default.SettingsPath;
                if (!Utils.IsFileExists(SettingsPath))
                {
                    bool DialogResult = Utils.ShowYesNoDialog("To select existing FilmFiles xml library press YES press NO to create it automatically");
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
                            if (ValidateXML(dlg.FileName))
                            {
                                IsInvalid = false;
                                Dobrofilm.Properties.Settings.Default.SettingsPath = dlg.FileName;
                                Dobrofilm.Properties.Settings.Default.Save();
                                XDocument XMLDoc = XDocument.Load(dlg.FileName);                                
                                return XMLDoc;
                            }
                            else
                            {
                                Utils.ShowWarningDialog("Select Enother FilmList File");
                            }
                        }
                    }
                    CreateNewXML();
                    XDocument XMLDoc_1 = XDocument.Load(Dobrofilm.Properties.Settings.Default.SettingsPath);
                    return XMLDoc_1;
                }
                XDocument XMLDoc_2 = XDocument.Load(SettingsPath);
                return XMLDoc_2;
            }
        } 

        public bool ValidateXML(string XMLDoc) //Valid
        {
            XmlDocument ValidDoc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add(null, "DobrofilmData.xsd");
            settings.ValidationType = ValidationType.Schema;
            XmlReader TryReader = XmlReader.Create(XMLDoc, settings);
            try
            {
                ValidDoc.Load(TryReader);
            }
            catch (XmlSchemaValidationException e)
            {
                string Message = String.Format("Validating error; {1}, try enother file", e.Message);
                Utils.ShowErrorDialog(Message);
                return false;
            }
            finally
            {
                TryReader.Close();
                ValidDoc = null;
            }
            return true;                 
        }

        public void CreateNewXML() //Valid
        {
            string DirPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\Dobrofilm");
            try
            {
                Utils.CreateDirectory(DirPath);
                string path = string.Concat(DirPath, "\\", "DobrofilmSettings.xml");            
                using (XmlWriter writer = XmlWriter.Create(path))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Dobrofilm", "http://tempuri.org/DobrofilmData.xsd");                                                     
                    //<files filemask="filemask1" nextnumber="1">
                    writer.WriteStartElement("files");
                    writer.WriteStartAttribute("filemask");
                    writer.WriteString("Dobrotfilm");
                    writer.WriteEndAttribute();                    
                    writer.WriteAttributeString("nextnumber", "1");                    
                    writer.WriteEndElement();
                    writer.WriteStartElement("categoris"); // <categoris nextid="1">                    
                    writer.WriteAttributeString("nextid", "1");                    
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                Dobrofilm.Properties.Settings.Default.SettingsPath = path;
                Dobrofilm.Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                Utils.ShowErrorDialog(ex.Message);
            }
        } 

        public XmlNamespaceManager GetDefNameSpaceManager() //valid
        {
            var namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("prefix", "http://tempuri.org/DobrofilmData.xsd");
            return namespaceManager;
        } 

        #region Film
        public IList<FilmFile> GetFilmFileFromXML(bool ShowCrypted) //valid
        {
            XDocument myXDocument = SettingsXMLDoc;
            XmlNamespaceManager namespaceManager = GetDefNameSpaceManager();
            IEnumerable<XElement> FilesList = myXDocument.XPathSelectElements("//prefix:Dobrofilm/prefix:files/prefix:file", namespaceManager);
            IList<FilmFile> FilmFileList = new List<FilmFile>();
            FilmFilesList filmFilesList = new FilmFilesList();
            foreach (XElement file in FilesList)
            {
                FilmFile FileClass = new FilmFile();
                FileClass.ID = new Guid((string)file.Attribute("GUID"));
                FileClass.Hint = (string)file.Attribute("hint");
                FileClass.Path = (string)file.Attribute("path");
                FileClass.Name = (string)file.Attribute("name");
                FileClass.Rate = (int)file.Attribute("rate");
                FileClass.IsCrypted = ((string)file.Attribute("isCrypted") == "1");
                FileClass.IsOnline = ((string)file.Attribute("isOnline") == "1");
                FileClass.Categoris = filmFilesList.CategorisArray((string)file.Attribute("categoris"));
                FileClass.filmsScr = file.XPathSelectElement(@"./prefix:filmsScr", namespaceManager);                
                FileClass.links = file.XPathSelectElement(@"./prefix:links", namespaceManager);                
                if (ShowCrypted || !FileClass.IsCrypted) FilmFileList.Add(FileClass);
            }            
            myXDocument = null;
            return FilmFileList;
        }

        public void AddFilmToXML(FilmFile filmFile, bool RenameFiles)
        {
            if (filmFile == null)
            {
                Utils.ShowWarningDialog("Saving Error");
                return;
            }
            XDocument FilmX = SettingsXMLDoc;
            if (RenameFiles)
            {
                XDocument myXDocument = SettingsXMLDoc;
                XElement FilesNode = myXDocument.XPathSelectElement("//prefix:Dobrofilm/prefix:files", GetDefNameSpaceManager());
                string NewFileMask = (string)FilesNode.Attribute("filemask");  //FilmX.Element("files").Attribute("filemask").Value;
                int FileNumber = (int)FilesNode.Attribute("nextnumber");       //int.Parse(FilmX.Element("files").Attribute("nextnumber").Value);
                if (NewFileMask != null || FileNumber != 0)
                {
                    string NewFileName = NewFileMask + FileNumber;
                    string NewFilePath = Utils.RenameFile(filmFile.Path, NewFileName);
                    filmFile.Hint = filmFile.Name;
                    filmFile.Name = NewFileName;
                    filmFile.Path = NewFilePath;
                    FilesNode.SetAttributeValue("nextnumber", Convert.ToString(FileNumber + 1));
                }                
                myXDocument = null;
            }
            XElement FilmXElement = CreateFilmXElement(filmFile);
            if (filmFile.ID == System.Guid.Empty)
            {
                FilmX.Element("files").Add(FilmXElement);
            }
            else
            {
                var FilmToChange =
                    (from p in FilmX.Descendants("file")
                     where new Guid(p.Attribute("GUID").Value) == filmFile.ID
                     select p).Single();
                FilmToChange.ReplaceWith(FilmXElement);
            }
            FilmX.Save(Dobrofilm.Properties.Settings.Default.SettingsPath);
            FilmX = null;
        }

        public XElement CreateFilmXElement(FilmFile FilmItem)
        {
            Guid ID;
            if (FilmItem.ID == System.Guid.Empty)
            {
                ID = Guid.NewGuid();
            }
            else
            {
                ID = FilmItem.ID;
            }
            if (FilmItem.Hint == null)
            {
                FilmItem.Hint = "Короткая характеристика";
            }
            XElement FilmElement = new XElement("file", 
                new XAttribute("name",FilmItem.Name),
                new XAttribute("GUID", ID),
                new XAttribute("hint", FilmItem.Hint),
                new XAttribute("path", FilmItem.Path),
                new XAttribute("rate", FilmItem.Rate),
                new XAttribute("categoris", GetStringFromIntArray(FilmItem.Categoris)),
                new XAttribute("isCrypted", Convert.ToInt32(FilmItem.IsCrypted)),
                new XAttribute("isOnline", Convert.ToInt32(FilmItem.IsOnline))
            );
            return FilmElement;
        }

        public string GetStringFromIntArray(int[] FilmsArray)
        {
            string ResultString = "0";
            if (FilmsArray != null)
            {
                if (FilmsArray.Length > 0)
                {
                    ResultString = string.Join(":", FilmsArray);
                }

            }
            return ResultString;
        }

        public void DeleteFilmItemFromXml(FilmFile FilmItem)
        {
            XDocument FilmX = SettingsXMLDoc;
            var FilmToDelete =
                    (from p in FilmX.Descendants("file")
                     where new Guid(p.Attribute("GUID").Value) == FilmItem.ID
                     select p).Single();
            FilmToDelete.Remove();
            FilmX.Save(Dobrofilm.Properties.Settings.Default.SettingsPath);
            FilmX = null;
        }        
        #endregion 
        
        #region Category
        public IList<CategoryClass> GetCategoryListFromXML() //valid
        {
            XDocument myXDocument = SettingsXMLDoc;
            IEnumerable<XElement> CategorisList = myXDocument.XPathSelectElements("//prefix:Dobrofilm/prefix:categoris/prefix:category", GetDefNameSpaceManager());
            IList<CategoryClass> CategoryList = new List<CategoryClass>();
            foreach (XElement Category in CategorisList)
            {
                CategoryClass categoryClass = new CategoryClass();
                categoryClass.ID = (int)Category.Attribute("id");
                categoryClass.Name = Category.Value;
                categoryClass.Hint = (string)Category.Attribute("hint");
                categoryClass.Icon = CategoryImgByteArray((string)Category.Attribute("image"));
                CategoryList.Add(categoryClass);
            }
            myXDocument = null;            
            return CategoryList;
        }

        public byte[] CategoryImgByteArray(string Base64String) //valid
        {
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(Base64String);
            }
            catch
            {
                imageBytes = new byte[0];
            }
            return imageBytes;
        }

        public int CurrentID { get; set; }

        public void AddCategoryToXML(CategoryClass Category)
        {
            if (Category == null)
            {
                Utils.ShowWarningDialog("Saving Error");
                return;
            }
            XDocument CategoryX = SettingsXMLDoc;
            XElement CategorisNode = CategoryX.XPathSelectElement("//prefix:Dobrofilm/prefix:categoris", GetDefNameSpaceManager());
            CurrentID = (int)CategorisNode.Attribute("nextid");
            XElement CategoryXElements = GetCategoryXElement(Category);
            if (Category.ID == 0)
            {
                CategoryX.Element("categoris").Add(CategoryXElements);
                CategoryX.Element("categoris").SetAttributeValue("nextid", Convert.ToString(CurrentID + 1));
                
            }
            else
            {
                var CategoryToChange =
                    (from p in CategoryX.Descendants("category")
                     where Convert.ToInt16(p.Attribute("id").Value) == Category.ID
                     select p).Single();
                CategoryToChange.ReplaceWith(CategoryXElements);

            }
            CategoryX.Save(Dobrofilm.Properties.Settings.Default.SettingsPath);            
            CategoryX = null;            
        }

        public XElement GetCategoryXElement(CategoryClass CategoryItem)
        {
            string ID;
            if (CategoryItem.ID == 0)
            {
                ID = Convert.ToString(CurrentID);
            }
            else
            {
                ID = Convert.ToString(CategoryItem.ID);
            }

            XElement CategoryElement = new XElement("category", CategoryItem.Name,
                new XAttribute("id", ID),
                new XAttribute("hint", CategoryItem.Hint),
                new XAttribute("image", ByteArrayeToBase64(CategoryItem.Icon))
            );
            //if (CategoryItem.ID == 0) Convert.ToString(CurrentID); else Convert.ToString(CategoryItem.ID);
            return CategoryElement;
        }

        public string ByteArrayeToBase64(byte[] ImageByteArray) //valid
        {
            string base64String = Convert.ToBase64String(ImageByteArray);
            return base64String;
        }

        public void DelCategory(CategoryClass CategoryItem)
        {
            if (CategoryItem.ID == 0)
            {
                Utils.ShowWarningDialog("Нельзя удалить системную категорию");
                return;
            }
            XDocument CategoryX = SettingsXMLDoc;
            var CategoryToDelete =
                    (from p in CategoryX.Descendants("category")
                     where Convert.ToInt16(p.Attribute("id").Value) == CategoryItem.ID
                     select p).Single();
            CategoryToDelete.Remove();
            CategoryX.Save(Dobrofilm.Properties.Settings.Default.SettingsPath);
            CategoryX = null;
        }

        #endregion

        #region Screen

        public IList<ScreenShotItem> GetScreenShootsByFilmFile(FilmFile Film) //valid
        {
            if (Film == null)
            {
                Utils.ShowErrorDialog("Screen not found");
                return null;
            }
            XElement ScreenShoots = Film.filmsScr;
            IList<ScreenShotItem> FilmScreens = new List<ScreenShotItem>();
            if (ScreenShoots == null) return FilmScreens;
            IEnumerable<XElement> ScreenShootsElements = ScreenShoots.XPathSelectElements(@"./prefix:screen", GetDefNameSpaceManager());            
            foreach (XElement ScreenElement in ScreenShootsElements)
            {
                ScreenShotItem FilmScreen = new ScreenShotItem { Base64String = ScreenElement.Attribute("base64Data").Value, ScreenShotID = ScreenElement.Attribute("id").Value };
                FilmScreens.Add(FilmScreen);
            }
            return FilmScreens;
        }

        public void AddScreenShotToXML(Byte[] ImageByteArray, Guid FilmID) //valid
        {            
            string base64String = Convert.ToBase64String(ImageByteArray);
            XDocument ScreenShotX = SettingsXMLDoc;

            XElement Film = (from p in ScreenShotX.XPathSelectElements(@"//prefix:Dobrofilm/prefix:files/prefix:file", GetDefNameSpaceManager())
                     where p.Attribute("GUID").Value == Convert.ToString(FilmID)
                     select p).Single();
            XElement FilmsScr = Film.XPathSelectElement(@"./prefix:filmsScr", GetDefNameSpaceManager());
            Int16 CurrentID = Convert.ToInt16(FilmsScr.Attribute("nextid").Value);
            XElement ScreenShotXElements = ScreenShotXElement(base64String, CurrentID);
            CurrentID++;
            FilmsScr.Add(ScreenShotXElements);
            FilmsScr.Attribute("nextid").Value = Convert.ToString(CurrentID);
            Film.ReplaceWith(Film);
            ScreenShotX.Save(Dobrofilm.Properties.Settings.Default.SettingsPath);            
        }

        private XElement ScreenShotXElement(string base64String, Int16 CurrentID) //valid
        {
            XNamespace ns = "http://tempuri.org/DobrofilmData.xsd";
            XElement ScreenShotElement = new XElement(ns + "screen",
               new XAttribute("id", Convert.ToString(CurrentID)),               
               new XAttribute("base64Data", base64String));
            return ScreenShotElement;
        }        

        public void DelScreenShotByID(string ScreenID, Guid FilmID) //not valid
        {
            if (ScreenID == string.Empty)
            {
                return;
            }
            XDocument ScreenShotX = SettingsXMLDoc;
            var ScreenShotToDelete =
                    (from p in ScreenShotX.Descendants("Screen")
                     where p.Attribute("id").Value == ScreenID
                     select p).Single();
            ScreenShotToDelete.Remove();
            ScreenShotX.Save(Dobrofilm.Properties.Settings.Default.SettingsPath);
        }
        #endregion

        #region Links

        public IList<LinksClass> GetLinksListByFilm(FilmFile Film) //valid
        {
            XElement LinksElement = Film.links;
            IEnumerable<XElement> LinksElements = LinksElement.XPathSelectElements(@"./prefix:link", GetDefNameSpaceManager());  //LinksElement.Descendants("link");
            IList<LinksClass> FilmScreens = new List<LinksClass>();
            foreach (XElement LinkElement in LinksElements)
            {
                LinksClass FilmScreen = new LinksClass { To = new Guid(LinkElement.Attribute("GUIDTO").Value) };
                FilmScreens.Add(FilmScreen);
            }
            return FilmScreens;
        }

        public void AddLinkToXML(LinksClass linkClass)
        {

        }

        #endregion
        


    }
}
