using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace Dobrofilm
{
    class HomeFolders
    {
        public IList<DirectoryInfo> HomeFoldersList
        {
            get
            {
                string AllFoldersString = Dobrofilm.Properties.Settings.Default.HomeFolders;
                string[] HomeFoldersArray = AllFoldersString.Split(';');
                IList<DirectoryInfo> HomeFoldersList = new List<DirectoryInfo>();
                foreach (string HomeFolder in HomeFoldersArray)
                {
                    DirectoryInfo HomeFolderInfo = new DirectoryInfo(HomeFolder);
                    HomeFoldersList.Add(HomeFolderInfo);
                }
                return HomeFoldersList;
            }
        }

        public void CheckHomeFolders()
        {
            IList<DirectoryInfo> homeFolderList = HomeFoldersList;
            FilmFilesList filmFilesList = new FilmFilesList();
            XDocument FilmX = XDocument.Load(filmFilesList.FileListPath);
            foreach (DirectoryInfo HomeFolder in homeFolderList)
            {                
                var allfiles = Directory.GetFiles(HomeFolder.FullName, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".mp4") || s.EndsWith(".wmv") || s.EndsWith(".avi") || s.EndsWith(".flv") || s.EndsWith(".mpg") || s.EndsWith(".mov"));
                string[] AllFilesArray = allfiles.ToArray();
                foreach (string FilePath in AllFilesArray)
                {
                    XElement FilmToChange =
                    (from p in FilmX.Descendants("file")
                     where p.Attribute("path").Value == FilePath
                     select p).Single();
                    if (FilmToChange == null)
                    {
                        AskToAddNewFilm(FilePath, filmFilesList);
                    }                    
                }
            }            
        }

        public void AskToAddNewFilm(string FilePath, FilmFilesList filmFilesList)
        {
            if (!Utils.ShowYesNoDialog(string.Format("New file {0} found is home folders, add to filmList?", FilePath))) return;

            
        }
    }
}
