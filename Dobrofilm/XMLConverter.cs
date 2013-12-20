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
        public void Main()
        {
            string FilmListXMLFile = Dobrofilm.Properties.Settings.Default.FilmListXMLFile;
            string CategoryListXMLFile = Dobrofilm.Properties.Settings.Default.CategoryListXMLFile;
            string LinksListXMLFile = Dobrofilm.Properties.Settings.Default.LinksListXMLFile;
            string ScreenShotXMLFile = Dobrofilm.Properties.Settings.Default.ScreenShotXMLFile;
            XMLEdit xMLEdit = new XMLEdit();
            if (!Utils.IsFileExists(FilmListXMLFile) || !Utils.IsFileExists(CategoryListXMLFile) || !Utils.IsFileExists(LinksListXMLFile) || !Utils.IsFileExists(ScreenShotXMLFile))
            {
                Utils.ShowErrorDialog("Not all file path set correctly");
                return;
            }
            string SettingsPath = Dobrofilm.Properties.Settings.Default.SettingsPath;
            if (!Utils.IsFileExists(SettingsPath))
            {                
                xMLEdit.CreateNewXML();
            }
            Dobrofilm.Properties.Settings.Default.SettingsPath = 
                string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\Dobrofilm\\DobrofilmSettings.xml");
            Dobrofilm.Properties.Settings.Default.Save();
            FilmFilesList filmFilesClass = new FilmFilesList();
            ListCollectionView filmFilesList = filmFilesClass.FilmFiles;
            XDocument MainFilmFileXML = xMLEdit.SettingsXMLDoc;
            foreach (FilmFile File in filmFilesList)
            {
                AddFilmToNewFormat(File, MainFilmFileXML);
            }
            MainFilmFileXML.Save(Dobrofilm.Properties.Settings.Default.SettingsPath);
        }

        private void AddFilmToNewFormat(FilmFile File, XDocument MainFile)
        {
            XMLEdit xMLEdit = new XMLEdit();
            string isCrypted = File.IsCrypted ? "1" : "0";
            string isOnline = File.IsOnline ? "1" : "0";                       

            XElement FileXmlElement = new XElement("file",
                new XAttribute("GUID", File.ID),
                new XAttribute("hint", File.Hint),
                new XAttribute("path", File.Path),
                new XAttribute("rate", File.Rate),
                new XAttribute("categoris", xMLEdit.GetStringFromIntArray(File.Categoris)),
                new XAttribute("isCrypted", File.IsCrypted ? "1" : "0"),
                new XAttribute("isOnline", File.IsOnline ? "1" : "0"),
                new XAttribute("name", File.Name)
                );      
            LinksList linksList = new LinksList();
            XElement[] LinksArray = linksList.GetLinkedFilm(File.ID);
            FileXmlElement.Add(new XElement("links"));
            foreach (XElement Link in LinksArray)
            {
                FileXmlElement.Element("links").Add(new XElement("link", new XAttribute("GUIDTO", Link.Attribute("GUIDTO").Value)));
            }
            FilmScreenShot filmScreenShot = new FilmScreenShot();
            XElement[] ScreenShotArray = filmScreenShot.GetScreenShotsXMLElementsByFilmID(File.ID);
            FileXmlElement.Add(new XElement("filmsScr", new XAttribute("nextid", Convert.ToString(ScreenShotArray.Length))));
            int cnt = 1;
            foreach (XElement Screen in ScreenShotArray)
            {
                FileXmlElement.Element("filmsScr").Add(new XElement("screen", new XAttribute("id", cnt)
                        , new XAttribute("base64Data", Screen.Attribute("base64Data").Value)));
                cnt++;
            }
            //var xmlReader = XmlReader.Create(Dobrofilm.Properties.Settings.Default.SettingsPath);
            //var myXDocument = XDocument.Load(xmlReader);
            //var namespaceManager = new XmlNamespaceManager(xmlReader.NameTable);
            //namespaceManager.AddNamespace("prefix", "http://tempuri.org/DobrofilmData.xsd");
            XDocument myXDocument = MainFile;
            XNamespace ns = "http://tempuri.org/DobrofilmData.xsd";
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(new NameTable());
            namespaceManager.AddNamespace("prefix", "http://tempuri.org/DobrofilmData.xsd");

            XElement FilesRootNode = myXDocument.XPathSelectElement("//prefix:Dobrofilm/prefix:files", namespaceManager);
            FilesRootNode.Add(FileXmlElement);
            FilesRootNode.ReplaceWith(FilesRootNode);
        }
    }
}
