using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Xml;
using System.Drawing;
using System.IO;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Dobrofilm
{
    public class CategoryClass
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public byte[] Icon { get; set; }
        public string Hint { get; set; }        
    }

    public class CategoryList 
    {
        
        public ListCollectionView Category { get; private set; }
        public ListCollectionView Category_v1 { get; private set; }

        public int CurrentID { get; set; }

        public CategoryList()
        {            
            XMLEdit xMLEdit1 = new XMLEdit();
            IList<CategoryClass> _categoris_2 = xMLEdit1.GetCategoryListFromXML();
            Category = (ListCollectionView)CollectionViewSource.GetDefaultView(_categoris_2);
        }

        public void CategoryList_v1()
        {
            IList<CategoryClass> _categoris = new List<CategoryClass> { };
            XmlDocument CategoryXml = new XmlDocument();
            CategoryXml.Load(CategoryListFileName);
            XmlNodeReader reader = new XmlNodeReader(CategoryXml);
            CategoryClass categoryClass = new CategoryClass();
            categoryClass.Name = "All Categoris";
            categoryClass.ID = -1;
            _categoris.Add(categoryClass);
            categoryClass = new CategoryClass();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "categoris")
                        {
                            CurrentID = int.Parse(reader.GetAttribute("nextid"));
                        }
                        else
                        {
                            categoryClass.ID = int.Parse(reader.GetAttribute("id"));
                            categoryClass.Hint = reader.GetAttribute("hint");
                            XMLEdit xMLEdit = new XMLEdit();
                            categoryClass.Icon = CategoryImgByteArray(reader.GetAttribute("image"));
                        }
                        break;
                    case XmlNodeType.Text:
                        categoryClass.Name = reader.Value;
                        break;
                    /*case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.ProcessingInstruction:
                        
                        break;
                    case XmlNodeType.Comment:
                        writer.WriteComment(reader.Value);
                        break;
                     */
                    case XmlNodeType.EndElement:
                        if (reader.Name != "categoris")
                        {
                            _categoris.Add(categoryClass);
                            categoryClass = new CategoryClass();
                        }
                        break;
                }

            }
            XMLEdit xMLEdit1 = new XMLEdit();            
            Category_v1 = (ListCollectionView)CollectionViewSource.GetDefaultView(_categoris);            
        }



        public string CategoryListFileName
        {
            get
            {
                string CategoryListPath = Dobrofilm.Properties.Settings.Default.CategoryListXMLFile;
                if (Utils.ValidateSettings(CategoryListPath, XMLFile.Categoris))
                {
                    return CategoryListPath;
                }
                bool DialogResult = Utils.ShowYesNoDialog("To select existing Category xml library press YES ress NO to create it automatically");
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
                        if (Utils.ValidateSettings(dlg.FileName, XMLFile.Categoris))
                        {
                            IsInvalid = false;
                            Dobrofilm.Properties.Settings.Default.CategoryListXMLFile = dlg.FileName;
                            Dobrofilm.Properties.Settings.Default.Save();
                            return dlg.FileName;
                        }
                        else
                        {
                            Utils.ShowWarningDialog("Select Enother CategoryList File");
                        }
                    }
                }
                //string NewFilePath = CreateNewCategoryListXML();
                //Dobrofilm.Properties.Settings.Default.CategoryListXMLFile = NewFilePath;
                //Dobrofilm.Properties.Settings.Default.Save();
                //return NewFilePath;
                return string.Empty;                
            }
        }


        //public string CreateNewCategoryListXML()
        //{  
        //    string DirPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\Dobrofilm");
        //    Utils.CreateDirectory(DirPath);
        //    string path = string.Concat(DirPath, "\\", "CategoryList.xml"); //Directory.GetCurrentDirectory()
        //    using (XmlWriter writer = XmlWriter.Create(path))
        //    {
        //        writer.WriteStartDocument();
        //            writer.WriteStartElement("categoris");
        //                writer.WriteStartAttribute("nextid");
        //                writer.WriteString("1");
        //                writer.WriteEndAttribute();
        //                writer.WriteStartElement("category");
        //                writer.WriteStartAttribute("id");
        //                writer.WriteString("0");
        //                writer.WriteEndAttribute();
        //                writer.WriteValue("No Category");
        //                writer.WriteEndElement();                
        //            writer.WriteEndElement();
        //        writer.WriteEndDocument();
        //    }
        //    return path;
        //}

        //public void AddCategory(CategoryClass CategoryItem) 
        //{   
        //    XDocument CategoryX = XDocument.Load(CategoryListFileName);
        //    XElement CategoryXElements = CategoryXElement(CategoryItem);                  
        //    if (CategoryItem.ID == 0)
        //    {                            
        //        CategoryX.Element("categoris").Add(CategoryXElements);
        //        CurrentID++;
        //        CategoryX.Element("categoris").Attribute("nextid").Value = Convert.ToString(CurrentID);                
        //    }
        //    else
        //    {               
        //        var CategoryToChange =
        //            (from p in CategoryX.Descendants("category")
        //            where Convert.ToInt16(p.Attribute("id").Value) == CategoryItem.ID
        //            select p).Single();
        //        CategoryToChange.ReplaceWith(CategoryXElements);              

        //    }
        //    CategoryX.Save(CategoryListFileName);            
        //}

      

        //public void DelCategory(CategoryClass CategoryItem)
        //{
        //    if (CategoryItem.ID == 0)
        //    {
        //        Utils.ShowWarningDialog("Нельзя удалить системную категорию");
        //        return;
        //    }            
        //    XDocument CategoryX = XDocument.Load(CategoryListFileName);            
        //    var CategoryToDelete =
        //            (from p in CategoryX.Descendants("category")
        //             where Convert.ToInt16(p.Attribute("id").Value) == CategoryItem.ID
        //             select p).Single();
        //    CategoryToDelete.Remove();
        //    CategoryX.Save(CategoryListFileName);
        //}

        //public XElement CategoryXElement(CategoryClass CategoryItem)
        //{
        //    string ID;
        //    if (CategoryItem.ID == 0)
        //    {
        //         ID = Convert.ToString(CurrentID);
        //    }
        //    else
        //    {
        //         ID = Convert.ToString(CategoryItem.ID);
        //    }                        

        //    XElement CategoryElement = new XElement("category", CategoryItem.Name,
        //        new XAttribute("id", ID),
        //        new XAttribute("hint", CategoryItem.Hint),
        //        new XAttribute("image", ByteArrayeToBase64(CategoryItem.Icon))
        //    );
        //    //if (CategoryItem.ID == 0) Convert.ToString(CurrentID); else Convert.ToString(CategoryItem.ID);
        //    return CategoryElement;            
        //}

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

        public string ByteArrayeToBase64(byte[] ImageByteArray)
        {
            string base64String = Convert.ToBase64String(ImageByteArray);
            return base64String;          
        }

        public BitmapImage GetCategoryBitmapImageByID (int CategoryID)
        {
            BitmapImage bitMap = new BitmapImage();           
            foreach (CategoryClass categoryClass in Category)
            {
                if (categoryClass.ID == CategoryID)
                {
                    bitMap = Utils.DecodePhoto(categoryClass.Icon);
                    return bitMap;
                }
            }           
            return bitMap;
        }


        public FormatConvertedBitmap GetGrayCategoryBitmapImageByID(int CategoryID)
        {
            BitmapImage bitMap = new BitmapImage();
            FormatConvertedBitmap grayBitmap = new FormatConvertedBitmap();
            foreach (CategoryClass categoryClass in Category)
            {
                if (categoryClass.ID == CategoryID)
                {
                    bitMap = Utils.DecodePhoto(categoryClass.Icon);
                   
                    grayBitmap.BeginInit();
                    grayBitmap.Source = bitMap;
                    grayBitmap.DestinationFormat = PixelFormats.Gray32Float;
                    grayBitmap.EndInit();                    
                    //bitMap = (BitmapImage)grayBitmap.Source;
                    return grayBitmap;
                }
            }
            return grayBitmap;
        }
    }
}
