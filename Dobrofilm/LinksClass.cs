using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Data;
using System.Xml.Linq;

namespace Dobrofilm
{
    public class LinksClass
    {
        public Guid From { get; set; }
        public Guid To { get; set; }
    }

    class LinksList
    {
        public ListCollectionView Link { get; private set; }

        //public LinksList()
        //{
        //    IList<LinksClass> _links = new List<LinksClass> { };
        //    XmlDocument LinksXml = new XmlDocument();
        //    LinksXml.Load(LinksListFileName);
        //    XmlNodeReader reader = new XmlNodeReader(LinksXml);
        //    LinksClass linksClass = new LinksClass();            
        //    while (reader.Read())
        //    {
        //        switch (reader.NodeType)
        //        {
        //            case XmlNodeType.Element:
        //                if (reader.Name == "link")                        
        //                {
        //                    linksClass.From = Guid.Parse(reader.GetAttribute("GUIDFROM"));
        //                    linksClass.To = Guid.Parse(reader.GetAttribute("GUIDTO"));                           
        //                }
        //                break;
        //            case XmlNodeType.Text:                        
        //                    break;                  
        //            case XmlNodeType.EndElement:
        //                    if (reader.Name != "links")
        //                    {
        //                        _links.Add(linksClass);
        //                        linksClass = new LinksClass();
        //                    }
        //                break;
        //        }

        //    }
        //    Link = (ListCollectionView)CollectionViewSource.GetDefaultView(_links);
        //}

        public XElement[] GetLinkedFilm(Guid FilmGuid)
        {
            if (FilmGuid == Guid.Empty || FilmGuid == null)
            {
                return null;
            }
            XDocument LinkX = XDocument.Load(LinksListFileName);
            var FindLink =
                (from p in LinkX.Descendants("link")
                 where new Guid(p.Attribute("GUIDFROM").Value) == FilmGuid
                 select p);
            return FindLink.ToArray();
        }

        public string LinksListFileName
        {
            get
            {
                string LinksListPath = Dobrofilm.Properties.Settings.Default.LinksListXMLFile;
                if (Utils.ValidateSettings(LinksListPath, XMLFile.Links))
                {
                    return LinksListPath;
                }
                bool DialogResult = Utils.ShowYesNoDialog("To select existing Links xml library press YES ress NO to create it automatically");
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
                        if (Utils.ValidateSettings(dlg.FileName, XMLFile.Links))
                        {
                            IsInvalid = false;
                            Dobrofilm.Properties.Settings.Default.LinksListXMLFile = dlg.FileName;
                            Dobrofilm.Properties.Settings.Default.Save();
                            return dlg.FileName;
                        }
                        else
                        {
                            Utils.ShowWarningDialog("Select Enother LinkList File");
                        }
                    }

                }
                string NewFilePath = CreateNewLinksListXML();
                Dobrofilm.Properties.Settings.Default.LinksListXMLFile = NewFilePath;
                Dobrofilm.Properties.Settings.Default.Save();
                return NewFilePath;
            }
        }

        public string CreateNewLinksListXML()
        {
            string DirPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\Dobrofilm");
            Utils.CreateDirectory(DirPath);
            string path = string.Concat(DirPath, "\\", "Links.xml");
            using (XmlWriter writer = XmlWriter.Create(path))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("links");
                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            return path;
        }

        //public void AddLink(LinksClass LinkItem)
        //{
        //    XDocument LinkX = XDocument.Load(LinksListFileName);
        //    XElement LinkXElements = LinkXElement(LinkItem);            
        //    LinkX.Element("links").Add(LinkXElements);            
        //    LinkX.Save(LinksListFileName);
        //}
        

        //public XElement LinkXElement(LinksClass LinkItem)
        //{
        //    XElement LinkElement = new XElement("link",
        //        new XAttribute("GUIDFROM", LinkItem.From),
        //        new XAttribute("GUIDTO", LinkItem.To)                
        //    );            
        //    return LinkElement;
        //}

        public void DelLink(LinksClass LinkItem)
        {
            XDocument LinkX = XDocument.Load(LinksListFileName);
            var LinkToDelete =
                    (from p in LinkX.Descendants("link")
                     where new Guid(p.Attribute("GUIDFROM").Value) == LinkItem.From
                     select p).Single();
            LinkToDelete.Remove();
            LinkX.Save(LinksListFileName);
            LinkToDelete = null;
            LinkToDelete =
                (from p in LinkX.Descendants("link")
                 where new Guid(p.Attribute("GUIDTO").Value) == LinkItem.From
                 select p).Single();
            LinkToDelete.Remove();
            LinkX.Save(LinksListFileName);
        }
    }
}
