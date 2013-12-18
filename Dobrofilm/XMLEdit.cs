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
        public XDocument SettingsXMLDoc
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
        
        public bool ValidateXML(string XMLDoc)
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

        public void CreateNewXML()
        {
            string DirPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\Dobrofilm");
            try
            {
                Utils.CreateDirectory(DirPath);
                string path = string.Concat(DirPath, "\\", "DobrofilmSettings.xml");            
                using (XmlWriter writer = XmlWriter.Create(path))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Dobrofilm");
                    writer.WriteStartAttribute("xmlns");
                    writer.WriteString("http://tempuri.org/DobrofilmData.xsd");
                    writer.WriteEndAttribute(); //<files filemask="filemask1" nextnumber="1">
                    writer.WriteStartElement("files");
                    writer.WriteStartAttribute("filemask");
                    writer.WriteString("Dobrotfilm");
                    writer.WriteEndAttribute();
                    writer.WriteStartAttribute("nextnumber");
                    writer.WriteString("1");
                    writer.WriteEndAttribute();
                    writer.WriteEndElement();
                    writer.WriteStartElement("categoris"); // <categoris nextid="1">
                    writer.WriteStartAttribute("nextid");
                    writer.WriteString("Dobrotfilm");
                    writer.WriteEndAttribute();
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

        #region Film
        public IList<FilmFile> GetFilmFileFromXML(bool ShowCrypted)
        {
            var xmlReader = XmlReader.Create(Dobrofilm.Properties.Settings.Default.SettingsPath);
            var myXDocument = XDocument.Load( xmlReader );
            var namespaceManager = new XmlNamespaceManager( xmlReader.NameTable );
            namespaceManager.AddNamespace("prefix", "http://tempuri.org/DobrofilmData.xsd");
            IEnumerable<XElement> FilesList = myXDocument.XPathSelectElements("//prefix:Dobrofilm/prefix:files/prefix:file", namespaceManager);
            IList<FilmFile> FilmFileList = new List<FilmFile>();
            FilmFilesList filmFilesList = new FilmFilesList();
            foreach (XElement file in FilesList)
            {
                FilmFile FileClass = new FilmFile();
                FileClass.ID = new Guid((string)file.Attribute("GUID"));
                FileClass.Hint = (string)file.Attribute("hint");
                FileClass.Path = (string)file.Attribute("path");
                FileClass.Rate = (int)file.Attribute("rate");
                FileClass.IsCrypted = ((string)file.Attribute("isCrypted") == "1");
                FileClass.IsOnline = ((string)file.Attribute("isOnline") == "1");
                FileClass.Categoris = filmFilesList.CategorisArray((string)file.Attribute("categoris"));
                XElement FilmsScr = file.XPathSelectElement("//prefix:file//prefix:filmsScr", namespaceManager);
                XElement links = file.XPathSelectElement("//prefix:file//prefix:links", namespaceManager);
                if (ShowCrypted || !FileClass.IsCrypted) FilmFileList.Add(FileClass);
            }
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
                var xmlReader = XmlReader.Create(Dobrofilm.Properties.Settings.Default.SettingsPath);
                var myXDocument = XDocument.Load( xmlReader );
                var namespaceManager = new XmlNamespaceManager( xmlReader.NameTable );
                namespaceManager.AddNamespace("prefix", "http://tempuri.org/DobrofilmData.xsd");
                XElement FilesNode = myXDocument.XPathSelectElement("//prefix:Dobrofilm/prefix:files", namespaceManager);
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
        }        
        #endregion 

        
        #region Category
        public IList<CategoryClass> GetCategoryListFromXML()
        {
            var xmlReader = XmlReader.Create(Dobrofilm.Properties.Settings.Default.SettingsPath);
            var myXDocument = XDocument.Load(xmlReader);
            var namespaceManager = new XmlNamespaceManager(xmlReader.NameTable);
            namespaceManager.AddNamespace("prefix", "http://tempuri.org/DobrofilmData.xsd");
            IEnumerable<XElement> CategorisList = myXDocument.XPathSelectElements("//prefix:Dobrofilm/prefix:categoris/prefix:category", namespaceManager);
            IList<CategoryClass> CategoryList = new List<CategoryClass>();
            foreach (XElement Category in CategorisList)
            {
                CategoryClass categoryClass = new CategoryClass();
                categoryClass.ID = (int)Category.Attribute("id");
                categoryClass.Hint = (string)Category.Attribute("hint");
                categoryClass.Icon = CategoryImgByteArray((string)Category.Attribute("image"));
                CategoryList.Add(categoryClass);
            }

            return CategoryList;
        }

        public byte[] CategoryImgByteArray(string Base64String)
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
            var xmlReader = XmlReader.Create(Dobrofilm.Properties.Settings.Default.SettingsPath);
            var myXDocument = XDocument.Load(xmlReader);
            var namespaceManager = new XmlNamespaceManager(xmlReader.NameTable);
            namespaceManager.AddNamespace("prefix", "http://tempuri.org/DobrofilmData.xsd");

            XElement FilesNode = myXDocument.XPathSelectElement("//prefix:Dobrofilm/prefix:categoris", namespaceManager);
            CurrentID = (int)FilesNode.Attribute("nextid");
            XElement CategoryXElements = CategoryXElement(Category);
            if (Category.ID == 0)
            {
                CategoryX.Element("categoris").Add(CategoryXElements);                
                CategoryX.Element("categoris").Attribute("nextid").Value = Convert.ToString(CurrentID + 1);
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
        }

        public XElement CategoryXElement(CategoryClass CategoryItem)
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

        public string ByteArrayeToBase64(byte[] ImageByteArray)
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
        }

        #endregion

        #region Screen
        public void AddScreenToXML()
        {

        }

        public void DelScreenFromXML()
        {

        }
        #endregion

        public void AddLinkToXML()
        {

        }


    }
}
