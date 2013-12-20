using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace Dobrofilm
{
    class ScreenShotItem
    {
        public string Base64String { get; set; }
        public string ScreenShotID { get; set; }
    }

    class FilmScreenShot
    {
        //public void SaveScreenShotToXML(Byte[] ImageByteArray, Guid FilmID)
        //{
        //    //Byte[] ImageByteArray = GetByteArrayFromBitmapEncoder(Screen);
        //    string base64String = Convert.ToBase64String(ImageByteArray);

        //    XDocument ScreenShotX = XDocument.Load(ScreenShotFileName);
        //    Int16 CurrentID = Convert.ToInt16(ScreenShotX.Element("FilmsScr").Attribute("nextid").Value);            
        //    XElement ScreenShotXElements = ScreenShotXElement(base64String, FilmID, CurrentID);
        //    CurrentID++;
        //    ScreenShotX.Element("FilmsScr").Add(ScreenShotXElements);
        //    ScreenShotX.Element("FilmsScr").Attribute("nextid").Value = Convert.ToString(CurrentID);
        //    ScreenShotX.Save(ScreenShotFileName);
        //}


        //private XElement ScreenShotXElement(string base64String, Guid FilmID, Int16 CurrentID)
        //{
        //     XElement ScreenShotElement = new XElement("Screen",
        //        new XAttribute("id", Convert.ToString(CurrentID)),
        //        new XAttribute("filmGuid", FilmID),
        //        new XAttribute("base64Data", base64String));
        //    return ScreenShotElement;
        //}

        private string ScreenShotFileName
        {
            get
            {
                string ScreenShottPath = Dobrofilm.Properties.Settings.Default.ScreenShotXMLFile;
                if (Utils.ValidateSettings(ScreenShottPath, XMLFile.Screens))
                {
                    return ScreenShottPath;
                }
                bool DialogResult = Utils.ShowYesNoDialog("To select existing ScreenShot xml library press YES ress NO to create it automatically");
                if (!!DialogResult)
                {
                    bool isInvalid = true;
                    while (isInvalid)
                    {
                        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                        dlg.DefaultExt = ".xml";
                        dlg.Filter = "XML Files|*.xml";
                        dlg.Multiselect = false;
                        dlg.ShowDialog();
                        if (Utils.ValidateSettings(dlg.FileName, XMLFile.Screens))
                        {
                            isInvalid = false;
                            Dobrofilm.Properties.Settings.Default.ScreenShotXMLFile = dlg.FileName;
                            Dobrofilm.Properties.Settings.Default.Save();
                            return dlg.FileName;
                        }
                    }
                }
                string NewFilePath = CreateNewScreenShotXML();
                Dobrofilm.Properties.Settings.Default.ScreenShotXMLFile = NewFilePath;
                Dobrofilm.Properties.Settings.Default.Save();
                return NewFilePath;
            }
        }

        private string CreateNewScreenShotXML()
        {
            string DirPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\Dobrofilm");
            Utils.CreateDirectory(DirPath);
            string path = string.Concat(DirPath, "\\", "FilmScreens.xml");
            using (XmlWriter writer = XmlWriter.Create(path))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("FilmsScr");
                writer.WriteStartAttribute("nextid");
                writer.WriteString("1");
                writer.WriteEndAttribute();
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            return path;
        }

        //private Byte[] GetByteArrayFromBitmapEncoder(JpegBitmapEncoder BitmapEnc)
        //{
        //    Byte[] _imageArray;
        //    MemoryStream memStream = new MemoryStream();
        //    BitmapEnc.Save(memStream);
        //    _imageArray = memStream.ToArray();
        //    return _imageArray;
        //}

        //public void DelScreenShotByID(string ID)
        //{
        //    if (ID == string.Empty)
        //    {                
        //        return;
        //    }
        //    XDocument ScreenShotX = XDocument.Load(ScreenShotFileName);
        //    var ScreenShotToDelete =
        //            (from p in ScreenShotX.Descendants("Screen")
        //             where p.Attribute("id").Value == ID
        //             select p).Single();
        //    ScreenShotToDelete.Remove();
        //    ScreenShotX.Save(ScreenShotFileName);
        //}

        //public IList<ScreenShotItem> GetScreenShotsByFilmID(Guid FilmID)
        //{
        //    if (FilmID == Guid.Empty)
        //    {
        //        return null;
        //    }
        //    XDocument ScreenShotX = XDocument.Load(ScreenShotFileName);
        //    var FindScreens =
        //        (from p in ScreenShotX.Descendants("Screen")
        //         where new Guid(p.Attribute("filmGuid").Value) == FilmID
        //         select p);
        //    XElement[] ScreensArray = FindScreens.ToArray();
        //    IList<ScreenShotItem> FilmScreens = new List<ScreenShotItem> { };
        //    foreach (XElement ScreenElement in ScreensArray)
        //    {
        //        ScreenShotItem FilmScreen = new ScreenShotItem { Base64String = ScreenElement.Attribute("base64Data").Value, ScreenShotID = ScreenElement.Attribute("id").Value };
        //        FilmScreens.Add(FilmScreen);
        //    }
        //    return FilmScreens;
        //}


        public XElement[] GetScreenShotsXMLElementsByFilmID(Guid FilmID)
        {
            if (FilmID == Guid.Empty)
            {
                return null;
            }
            XDocument ScreenShotX = XDocument.Load(ScreenShotFileName);
            var FindScreens =
                (from p in ScreenShotX.Descendants("Screen")
                 where new Guid(p.Attribute("filmGuid").Value) == FilmID
                 select p);
            XElement[] ScreensArray = FindScreens.ToArray();
            return ScreensArray;
        }

    }
}
