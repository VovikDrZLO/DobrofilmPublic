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
using System.Windows.Data;

namespace Dobrofilm
{
    class XMLEdit
    {
        public string SettingsPath = Dobrofilm.Properties.Settings.Default.SettingsPath;
        public XDocument SettingsXMLDoc //valid
        {
            get
            {   
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
                                SettingsPath = dlg.FileName;
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
                    XDocument XMLDoc_1 = XDocument.Load(SettingsPath);
                    return XMLDoc_1;
                }
                XDocument XMLDoc_2 = XDocument.Load(SettingsPath);
                return XMLDoc_2;
            }
        }

        XNamespace ns = "http://tempuri.org/DobrofilmData.xsd";

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
                    //<category id="0">No Category</category>
                    writer.WriteStartElement("category");
                    writer.WriteAttributeString("id", "0");
                    writer.WriteValue("No Category");
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteStartElement("profiles");                    
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                SettingsPath = path;
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
        public IList<FilmFile> GetFilmFileFromXML(bool ShowCrypted, ProfileClass Profile, bool DisableFilters) 
        {
            if (Profile == null) Profile = new ProfileClass { ProfileID = Guid.Empty };
            XDocument myXDocument = SettingsXMLDoc;
            XmlNamespaceManager namespaceManager = GetDefNameSpaceManager();
            IEnumerable<XElement> FilesList = myXDocument.XPathSelectElements("//prefix:Dobrofilm/prefix:files/prefix:file", namespaceManager);
            IList<FilmFile> FilmFileList = new List<FilmFile>();            
            foreach (XElement file in FilesList)
            {
                FilmFile FileClass = ConvertXElementToFilmFile(file);
                if (((ShowCrypted || !FileClass.IsCrypted) && (FileClass.Profile == Profile.ProfileID)) || DisableFilters) FilmFileList.Add(FileClass); 
            }            
            myXDocument = null;
            return FilmFileList;
        }

        public FilmFile ConvertXElementToFilmFile(XElement file) //valid
        {
            FilmFilesList filmFilesList = new FilmFilesList();
            FilmFile FileClass = new FilmFile();
            FileClass.ID = new Guid((string)file.Attribute("GUID"));
            FileClass.Hint = (string)file.Attribute("hint");
            FileClass.Path = (string)file.Attribute("path");
            FileClass.Name = (string)file.Attribute("name");
            FileClass.Rate = (int)file.Attribute("rate");
            FileClass.IsCrypted = ((string)file.Attribute("isCrypted") == "1");
            FileClass.IsOnline = ((string)file.Attribute("isOnline") == "1");
            
            FileClass.Profile = ((string)file.Attribute("profile") == null)? Guid.Empty : new Guid((string)file.Attribute("profile"));
            FileClass.Categoris = filmFilesList.CategorisArray((string)file.Attribute("categoris"));
            FileClass.filmsScr = file.XPathSelectElement(@"./prefix:filmsScr", GetDefNameSpaceManager());
            FileClass.links = file.XPathSelectElement(@"./prefix:links", GetDefNameSpaceManager());
            return FileClass;
        }

        public FilmFile GetFilmFileFromXMLByID(Guid FilmID) //valid
        {
            XDocument ScreenShotX = SettingsXMLDoc;
            XElement Film = (from p in ScreenShotX.XPathSelectElements(@"//prefix:Dobrofilm/prefix:files/prefix:file", GetDefNameSpaceManager())
                             where p.Attribute("GUID").Value == Convert.ToString(FilmID)
                             select p).Single();
            return ConvertXElementToFilmFile(Film);
        }

        public void AddFilmToXML(FilmFile filmFile, bool RenameFiles)  //valid
        {
            if (filmFile == null)
            {
                Utils.ShowWarningDialog("Saving Error");
                return;
            }
            XDocument FilmX = SettingsXMLDoc;
            if (RenameFiles)
            {                
                XElement FilesNode = FilmX.XPathSelectElement("//prefix:Dobrofilm/prefix:files", GetDefNameSpaceManager());
                string NewFileMask = (string)FilesNode.Attribute("filemask");  
                int FileNumber = (int)FilesNode.Attribute("nextnumber");       
                if (NewFileMask != null || FileNumber != 0)
                {
                    string NewFileName = NewFileMask + FileNumber;
                    string NewFilePath = Utils.RenameFile(filmFile.Path, NewFileName);
                    filmFile.Hint = filmFile.Name;
                    filmFile.Name = NewFileName;
                    filmFile.Path = NewFilePath;
                    FilesNode.SetAttributeValue("nextnumber", Convert.ToString(FileNumber + 1));
                }                
            }           
            XElement FilmXElement = CreateFilmXElement(filmFile);
            if (filmFile.ID == System.Guid.Empty)
            {
                XElement FilmsNode = FilmX.XPathSelectElement("//prefix:Dobrofilm/prefix:files", GetDefNameSpaceManager());
                FilmsNode.Add(FilmXElement); //!!!
            }
            else
            {
                var FilmToChange =
                    (from p in FilmX.Descendants(ns + "file")
                     where new Guid(p.Attribute("GUID").Value) == filmFile.ID
                     select p).Single();
                FilmToChange.ReplaceWith(FilmXElement);
            }
            FilmX.Save(SettingsPath);
            FilmX = null;
        }

        public XElement CreateFilmXElement(FilmFile FilmItem) //valid
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
            XElement FilmElement = new XElement(ns + "file", 
                new XAttribute("name",FilmItem.Name),
                new XAttribute("GUID", ID),
                new XAttribute("hint", FilmItem.Hint),
                new XAttribute("path", FilmItem.Path),
                new XAttribute("rate", FilmItem.Rate),
                new XAttribute("categoris", GetStringFromIntArray(FilmItem.Categoris)),
                new XAttribute("isCrypted", Convert.ToInt32(FilmItem.IsCrypted)),
                new XAttribute("isOnline", Convert.ToInt32(FilmItem.IsOnline)),
                new XAttribute("profile", FilmItem.Profile),
                (FilmItem.filmsScr == null) ? new XElement(ns + "filmsScr", new XAttribute("nextid", "1")) : FilmItem.filmsScr,
                (FilmItem.links == null) ? new XElement(ns + "links") : FilmItem.links
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

        public void DeleteFilmItemFromXml(FilmFile FilmItem) //valid
        {
            XDocument FilmX = SettingsXMLDoc;
            var FilmToDelete =
                    (from p in FilmX.Descendants(ns + "file")
                     where new Guid(p.Attribute("GUID").Value) == FilmItem.ID
                     select p).Single();
            FilmToDelete.Remove();
            FilmX.Save(SettingsPath);
            FilmX = null;
        }

        public string GetFilmMask() 
        {
            XDocument FilmX = SettingsXMLDoc;
            XElement FilesNode = FilmX.XPathSelectElement("//prefix:Dobrofilm/prefix:files", GetDefNameSpaceManager());
            return FilesNode.Attribute("filemask").Value;
        }

        public string GetFileMaskNextID() //not valid
        {
            XDocument FilmX = SettingsXMLDoc;
            XElement FilesNode = FilmX.XPathSelectElement("//prefix:Dobrofilm/prefix:files", GetDefNameSpaceManager());
            return FilesNode.Attribute("nextnumber").Value;
        }

        public void SetFilmMaskAndCounter(string Mask, int Counter) //not valid
        {
            if (Counter == 0)
            {
                return;
            }
            XDocument FilmX = SettingsXMLDoc;
            XElement FilesNode = FilmX.XPathSelectElement("//prefix:Dobrofilm/prefix:files", GetDefNameSpaceManager());
            FilesNode.SetAttributeValue("nextnumber", Convert.ToString(Counter));
            FilesNode.SetAttributeValue("filemask", Mask);
            //FilmX.Element("files").Attribute("nextnumber").Value = Convert.ToString(Counter);
            //FilmX.Element("files").Attribute("filemask").Value = Mask;
            FilmX.Save(SettingsPath);
        }

        #endregion 
        
        #region Category
        public IList<CategoryClass> GetCategoryListFromXML(ProfileClass Profile) //valid
        {
            if (Profile == null) Profile = new ProfileClass { ProfileID = Guid.Empty };
            XDocument myXDocument = SettingsXMLDoc;
            IEnumerable<XElement> CategorisList = myXDocument.XPathSelectElements("//prefix:Dobrofilm/prefix:categoris/prefix:category", GetDefNameSpaceManager());
            IList<CategoryClass> CategoryList = new List<CategoryClass>();
            CategoryClass categoryClass = new CategoryClass();
            categoryClass.Name = "All Categoris";
            categoryClass.ID = -1;
            CategoryList.Add(categoryClass);
            categoryClass = new CategoryClass();
            foreach (XElement Category in CategorisList)
            {
                
                    categoryClass = new CategoryClass();
                    categoryClass.ID = (int)Category.Attribute("id");
                    categoryClass.Name = Category.Value;
                    categoryClass.Profile = ((string)Category.Attribute("profile") == null) ? Guid.Empty : new Guid((string)Category.Attribute("profile"));
                    categoryClass.Hint = (string)Category.Attribute("hint");
                    categoryClass.Icon = CategoryImgByteArray((string)Category.Attribute("image"));
                    if (Profile.ProfileID == categoryClass.Profile)
                    {
                       CategoryList.Add(categoryClass);
                    }
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

        public void AddCategoryToXML(CategoryClass Category) //valid
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
                CategorisNode.Add(CategoryXElements);
                CategorisNode.SetAttributeValue("nextid", Convert.ToString(CurrentID + 1));
                
            }
            else
            {
                var CategoryToChange =
                    (from p in CategoryX.Descendants(ns + "category")
                     where Convert.ToInt16(p.Attribute("id").Value) == Category.ID
                     select p).Single();
                CategoryToChange.ReplaceWith(CategoryXElements);

            }
            CategoryX.Save(SettingsPath);            
            CategoryX = null;            
        }

        public XElement GetCategoryXElement(CategoryClass CategoryItem) //valid
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
            XElement CategoryElement = new XElement(ns + "category", CategoryItem.Name,
                new XAttribute("id", ID),
                new XAttribute("hint", CategoryItem.Hint),
                new XAttribute("image", ByteArrayeToBase64(CategoryItem.Icon)),
                   (CategoryItem.Profile != Guid.Empty) ? new XAttribute("profile", CategoryItem.Profile) : null 
            );
            //if (CategoryItem.ID == 0) Convert.ToString(CurrentID); else Convert.ToString(CategoryItem.ID);
            return CategoryElement;
        }

        public string ByteArrayeToBase64(byte[] ImageByteArray) //valid
        {
            string base64String = Convert.ToBase64String(ImageByteArray);
            return base64String;
        }

        public void DelCategory(CategoryClass CategoryItem) //valid
        {
            if (CategoryItem.ID == 0)
            {
                Utils.ShowWarningDialog("Нельзя удалить системную категорию");
                return;
            }
            XDocument CategoryX = SettingsXMLDoc;            
            var CategoryToDelete =
                    (from p in CategoryX.Descendants(ns + "category")
                     where Convert.ToInt16(p.Attribute("id").Value) == CategoryItem.ID
                     select p).Single();
            CategoryToDelete.Remove();
            CategoryX.Save(SettingsPath);
            CategoryX = null;
        }

        public string GetCategoryNextID() //not valid
        {
            XDocument CategoryX = SettingsXMLDoc;
            XElement CategoryNode = CategoryX.XPathSelectElement("//prefix:Dobrofilm/prefix:categoris", GetDefNameSpaceManager());
            return CategoryNode.Attribute("nextid").Value;
        }

        public void SetCategoryID(int NewID) //not valid
        {
            if (NewID == 0)
            {
                return;
            }
            XDocument CategoryX = SettingsXMLDoc;
            XElement CategoryNode = CategoryX.XPathSelectElement("//prefix:Dobrofilm/prefix:categoris", GetDefNameSpaceManager());
            CategoryNode.SetAttributeValue("nextid", Convert.ToString(NewID));            
            CategoryX.Save(SettingsPath);
        }

        public XDocument CreateNoCategoryRecordForNewProfile(ProfileClass Profile, XDocument CategoryX)
        {            
            CategoryClass Category = new CategoryClass{Name = "No Category", Profile = Profile.ProfileID, ID = 0 };                        
            XElement CategorisNode = CategoryX.XPathSelectElement("//prefix:Dobrofilm/prefix:categoris", GetDefNameSpaceManager());
            
            XElement CategoryXElements = new XElement(ns + "category", Category.Name,
                                        new XAttribute("id", Category.ID),                                                                                
                                        new XAttribute("profile", Category.Profile) );
            CategorisNode.Add(CategoryXElements);            
            CategoryX.Save(SettingsPath);
            return CategoryX;                        
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
            ScreenShotX.Save(SettingsPath);            
        }

        private XElement ScreenShotXElement(string base64String, Int16 CurrentID) //valid
        {            
            XElement ScreenShotElement = new XElement(ns + "screen",
               new XAttribute("id", Convert.ToString(CurrentID)),               
               new XAttribute("base64Data", base64String));
            return ScreenShotElement;
        }        

        public void DelScreenShotByID(string ScreenID, Guid FilmID) //valid
        {
            if (ScreenID == string.Empty)
            {
                return;
            }
            XDocument ScreenShotX = SettingsXMLDoc;
            XElement FilmWithScreen =
                    (from p in ScreenShotX.XPathSelectElements(@"//prefix:Dobrofilm/prefix:files/prefix:file", GetDefNameSpaceManager()) //.Descendants("Screen") ///prefix:filmsScr/prefix:screen
                     where p.Attribute("GUID").Value == Convert.ToString(FilmID)
                     select p).Single();
            foreach (XElement ScreenShotToDelete in FilmWithScreen.XPathSelectElements(@"./prefix:filmsScr/prefix:screen", GetDefNameSpaceManager()))
            {
                if (ScreenShotToDelete.Attribute("id").Value == ScreenID)
                {
                    ScreenShotToDelete.Remove();
                }
            }            
            ScreenShotX.Save(SettingsPath);
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

        public void AddLinkToXML(LinksClass linkClass) //valid
        {

            XDocument LinkX = SettingsXMLDoc;
            XElement Film = (from p in LinkX.XPathSelectElements(@"//prefix:Dobrofilm/prefix:files/prefix:file", GetDefNameSpaceManager())
                             where p.Attribute("GUID").Value == Convert.ToString(linkClass.From)
                             select p).Single();

            XElement LinkXElements = LinkXElement(linkClass);            

            Film.Element(ns + "links").Add(LinkXElements);
            LinkX.Save(SettingsPath);           
        }

        public XElement LinkXElement(LinksClass LinkItem)
        {            
            XElement LinkElement = new XElement(ns + "link", new XAttribute("GUIDTO", LinkItem.To)
            );
            return LinkElement;
        }

        #endregion

        #region Profile

        public int GetIndexNumberOfProfile(ProfileClass Profile)
        {
            int cnt = 0;
            foreach (ProfileClass PrifileClassItem in GetProfilesList)
            {
                if (PrifileClassItem.ProfileID == Profile.ProfileID) return cnt;
                cnt++ ;
            }
            return -1;
        }

        public void AddProfileToXML(ProfileClass Profile)
        {
            if (Profile == null)
            {
                Utils.ShowWarningDialog("Saving Error");
                return;
            }
            XDocument ProfileX = SettingsXMLDoc;
            XElement ProfilesNode = ProfileX.XPathSelectElement("//prefix:Dobrofilm/prefix:profiles", GetDefNameSpaceManager());                                    
            if (Profile.ProfileID == Guid.Empty)
            {
                ProfilesNode.Add(CreateProfileXElement(Profile));
                ProfileX = CreateNoCategoryRecordForNewProfile(Profile, ProfileX);
            }
            else
            {
                var ProfileToChange =
                    (from p in ProfileX.Descendants(ns + "profile")
                     where new Guid(p.Attribute("GUID").Value) == Profile.ProfileID
                     select p).Single();
                ProfileToChange.ReplaceWith(CreateProfileXElement(Profile));

            }
            ProfileX.Save(SettingsPath);
            ProfileX = null;            
        }

        public ProfileClass GetProfileByID(Guid ProfID)
        {
            var Profile =
                    (from p in GetProfilesList
                     where p.ProfileID == ProfID
                     select p).Single();
            return (ProfileClass)Profile;
        }

        public XElement CreateProfileXElement(ProfileClass ProfileItem)
        {
            if (ProfileItem.ProfileID == Guid.Empty)
            {
                ProfileItem.ProfileID = Guid.NewGuid();
            }
            XElement ProfileElement = new XElement(ns + "profile",
                new XAttribute("GUID", ProfileItem.ProfileID),
                new XAttribute("name", ProfileItem.Name)
            );            
            return ProfileElement;
        }

        //public IList<ProfileClass> GetProfilesList()
        //{
        //    XDocument ProfileX = SettingsXMLDoc;
        //    IEnumerable<XElement> ProfilesList = ProfileX.XPathSelectElements("//prefix:Dobrofilm/prefix:profiles/prefix:profile", GetDefNameSpaceManager());
        //    IList<ProfileClass> ProfileList = new List<ProfileClass>();
        //    ProfileClass profileClass = new ProfileClass();
        //    profileClass.Name = "Default";
        //    ProfileList.Add(profileClass);
        //    foreach (XElement Profile in ProfilesList)
        //    {
        //        profileClass = new ProfileClass();
        //        profileClass.ProfileID = new Guid(Profile.Attribute("GUID").Value);
        //        profileClass.Name = Profile.Attribute("name").Value;
        //        ProfileList.Add(profileClass);
        //    }
        //    ProfileX = null;
        //    return ProfileList;            
        //}

        public IList<ProfileClass> GetProfilesList
        {
            get
            {
                XDocument ProfileX = SettingsXMLDoc;
                IEnumerable<XElement> ProfilesList = ProfileX.XPathSelectElements("//prefix:Dobrofilm/prefix:profiles/prefix:profile", GetDefNameSpaceManager());
                IList<ProfileClass> ProfileList = new List<ProfileClass>();
                ProfileClass profileClass = new ProfileClass();
                profileClass.Name = "Default";
                ProfileList.Add(profileClass);
                foreach (XElement Profile in ProfilesList)
                {
                    profileClass = new ProfileClass();
                    profileClass.ProfileID = new Guid(Profile.Attribute("GUID").Value);
                    profileClass.Name = Profile.Attribute("name").Value;
                    ProfileList.Add(profileClass);
                }
                ProfileX = null;
                return ProfileList;
            }
        }

        public void DeleteProfile(ProfileClass Profile) // TODO Add check for linked films and categoris
        {
            if (Profile == null) return;
            if (ProfileIsBusy(Profile)) return;
            XDocument ProfileX = SettingsXMLDoc;
            var ProfileToDelete =
                    (from p in ProfileX.Descendants(ns + "profile")
                     where new Guid(p.Attribute("GUID").Value) == Profile.ProfileID
                     select p).Single();
            ProfileToDelete.Remove();
            ProfileX.Save(SettingsPath);
            ProfileX = null;
        }

        public bool ProfileIsBusy(ProfileClass Profile)
        {
            FilmFilesList filmFilesList = new FilmFilesList();
            ListCollectionView FilmList = filmFilesList.GetFilmListByProfile(Profile);
            CategoryList categoryList = new CategoryList();
            ListCollectionView CaterorisList = categoryList.GetCategorisListByProfile(Profile);
            if (FilmList.Count > 0 || CaterorisList.Count > 0)
            {
                Utils.ShowWarningDialog("Linked films or categoris exists!!!");
                return true;
            }
            return false;
        }

        #endregion

    }
}
