using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Xml.Linq;
using System.Xml;
using System.Collections;
using System.Xml.XPath;



namespace Dobrofilm
{
    class XMLConverter
    {
        public bool IsNeedConvert()
        {
            string SettingFilePath = Dobrofilm.Properties.Settings.Default.SettingsPath;
            if (SettingFilePath == string.Empty || !Utils.IsFileExists(SettingFilePath)) return true;
            XMLEdit xMLEdit = new XMLEdit();
            if (!xMLEdit.ValidateXML(SettingFilePath)) return true;
            return false;
        }

        public void MakeConversion()
        {
            string FilmListXMLFile = Dobrofilm.Properties.Settings.Default.FilmListXMLFile;
            string CategoryListXMLFile = Dobrofilm.Properties.Settings.Default.CategoryListXMLFile;
            string LinksListXMLFile = Dobrofilm.Properties.Settings.Default.LinksListXMLFile;
            string ScreenShotXMLFile = Dobrofilm.Properties.Settings.Default.ScreenShotXMLFile;
            bool IsNew = false;
            XMLEdit xMLEdit = new XMLEdit();
            if (!Utils.IsFileExists(FilmListXMLFile) || !Utils.IsFileExists(CategoryListXMLFile) || !Utils.IsFileExists(LinksListXMLFile) || !Utils.IsFileExists(ScreenShotXMLFile))
            {
                Utils.ShowWarningDialog("No source files found, empty settings file will by created");
                IsNew = true;
            }
            string SettingsPath = Dobrofilm.Properties.Settings.Default.SettingsPath;
            if (!Utils.IsFileExists(SettingsPath))
            {                
                xMLEdit.CreateNewXML();
            }
            if (IsNew) return;
            Dobrofilm.Properties.Settings.Default.SettingsPath = 
                string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\Dobrofilm\\DobrofilmSettings.xml");
            Dobrofilm.Properties.Settings.Default.Save();
            FilmFilesList filmFilesClass = new FilmFilesList();
            ListCollectionView filmFilesList = filmFilesClass.FilmFiles_v1;            
            XDocument MainFilmFileXML = xMLEdit.SettingsXMLDoc;
            foreach (FilmFile File in filmFilesList)
            {
                AddFilmToNewFormat(File, MainFilmFileXML);
            }
            CategoryList categorisClass = new CategoryList();
            categorisClass.CategoryList_v1();
            ListCollectionView categoryList = categorisClass.Category_v1;            
            foreach (CategoryClass Category in categoryList)
            {
                AddCategoryToNewFormat(Category, MainFilmFileXML);
            }
            MainFilmFileXML.Save(Dobrofilm.Properties.Settings.Default.SettingsPath);
        }

        private void AddFilmToNewFormat(FilmFile File, XDocument MainFile)
        {
            XMLEdit xMLEdit = new XMLEdit();
            XNamespace ns = "http://tempuri.org/DobrofilmData.xsd";
            XElement FileXmlElement = new XElement(ns + "file",
                new XAttribute("GUID", File.ID),
                new XAttribute("hint", File.Hint),
                new XAttribute("path", File.Path),
                new XAttribute("rate", File.Rate),
                new XAttribute("categoris", xMLEdit.GetStringFromIntArray(File.Categoris)),
                new XAttribute("isCrypted", File.IsCrypted ? "1" : "0"),
                new XAttribute("isOnline", File.IsOnline ? "1" : "0"),
                new XAttribute("name", File.Name)
                );                  
            FilmScreenShot filmScreenShot = new FilmScreenShot();
            XElement[] ScreenShotArray = filmScreenShot.GetScreenShotsXMLElementsByFilmID(File.ID);
            XElement filmsScr = new XElement(ns + "filmsScr", 
                new XAttribute("nextid", Convert.ToString(ScreenShotArray.Length + 1)));
            FileXmlElement.Add(filmsScr);            
            int cnt = 1;
            foreach (XElement Screen in ScreenShotArray)
            {
                FileXmlElement.Element(ns + "filmsScr").Add(new XElement(ns + "screen", new XAttribute("id", cnt)
                        , new XAttribute("base64Data", Screen.Attribute("base64Data").Value)));
                cnt++;
            }
            LinksList linksList = new LinksList();
            XElement[] LinksArray = linksList.GetLinkedFilm(File.ID);
            FileXmlElement.Add(new XElement(ns + "links"));
            foreach (XElement Link in LinksArray)
            {
                FileXmlElement.Element(ns + "links").Add(new XElement(ns + "link", new XAttribute("GUIDTO", Link.Attribute("GUIDTO").Value)));
            }            
            XDocument myXDocument = MainFile;
            
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("prefix", "http://tempuri.org/DobrofilmData.xsd");
            XElement FilesRootNode = myXDocument.XPathSelectElement("//prefix:Dobrofilm/prefix:files", namespaceManager);     

            FilesRootNode.Add(FileXmlElement);
            FilesRootNode.ReplaceWith(FilesRootNode);
        }

        private void AddCategoryToNewFormat(CategoryClass Category, XDocument MainFile)
        {
            XMLEdit xMLEdit = new XMLEdit();
            CategoryList сategoryList = new CategoryList();            
            XNamespace ns = "http://tempuri.org/DobrofilmData.xsd";
            if (Category.ID == -1) return;            
            XElement CategoryXmlElement = new XElement(ns + "category", Category.Name);
            CategoryXmlElement.SetAttributeValue("id", Category.ID);
            CategoryXmlElement.SetAttributeValue("hint", Category.Hint);
            if (Category.ID != 0) CategoryXmlElement.SetAttributeValue("image", сategoryList.ByteArrayeToBase64(Category.Icon));                
            XDocument myXDocument = MainFile;
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("prefix", "http://tempuri.org/DobrofilmData.xsd");

            XElement CategorisRootNode = myXDocument.XPathSelectElement("//prefix:Dobrofilm/prefix:categoris", namespaceManager);
            CategorisRootNode.Attribute("nextid").Value = Convert.ToString(Convert.ToInt16(CategorisRootNode.Attribute("nextid").Value)+ 1);
            CategorisRootNode.Add(CategoryXmlElement);
            CategorisRootNode.ReplaceWith(CategorisRootNode);
        }
    }
}
