using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Windows.Data;
using System.ComponentModel;

namespace Dobrofilm
{
    class HomeFolders
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        public IList<DirectoryInfo> HomeFoldersList
        {
            get
            {
                string AllFoldersString = Dobrofilm.Properties.Settings.Default.HomeFolders;
                string[] HomeFoldersArray = AllFoldersString.Split(';');
                IList<DirectoryInfo> HomeFoldersList = new List<DirectoryInfo>();
                if (HomeFoldersArray.Length == 1 && HomeFoldersArray[0] == string.Empty) return HomeFoldersList;
                foreach (string HomeFolder in HomeFoldersArray)
                {
                    DirectoryInfo HomeFolderInfo = new DirectoryInfo(HomeFolder);
                    HomeFoldersList.Add(HomeFolderInfo);
                }
                return HomeFoldersList;
            }
        }

        public string GlobalFilePath { get; set; }

        public void CheckHomeFolders()
        {
            worker.DoWork += worker_DoWork;
            worker.RunWorkerAsync();
        }

        public bool IsFileInLibtary(object de)
        {
            FilmFile film = de as FilmFile;
            return film.Path == GlobalFilePath;
        }

        public void AskToAddNewFilm(string FilePath)
        {
            string FilmName = System.IO.Path.GetFileNameWithoutExtension(FilePath);
            if (!Utils.ShowSimpleYesNoDialog(string.Format("New file {0} found is home folders, add to filmList?", FilmName))) return;
            XMLEdit xMLEdit = new XMLEdit();
            xMLEdit.AddFilmToXML(new FilmFile
            {
                Name = FilmName,
                Path = FilePath,
                Rate = 0,
                Categoris = new int[1]{0},
                IsCrypted = false,
                IsOnline  = false,
                IsFTP = false
            }, false);
            
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // run all background tasks here            
            IList<DirectoryInfo> homeFolderList = HomeFoldersList;
            XMLEdit xMLEdit = new XMLEdit();
            IList<FilmFile> filmFiles = xMLEdit.GetFilmFileFromXML(true, null, true);
            var FilmFilesList = (ListCollectionView)CollectionViewSource.GetDefaultView(filmFiles);
            //FilmFilesList filmFilesList = new FilmFilesList();
            //ListCollectionView filmFiles = filmFilesList.FilmFiles;
            foreach (DirectoryInfo HomeFolder in homeFolderList)
            {
                if (HomeFolder.Exists)
                {
                    var allfiles = Directory.GetFiles(HomeFolder.FullName, "*.*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".mp4") || s.EndsWith(".wmv") || s.EndsWith(".avi") || s.EndsWith(".flv") || s.EndsWith(".mpg") || s.EndsWith(".mov"));
                    string[] AllFilesArray = allfiles.ToArray();
                    foreach (string FilePath in AllFilesArray)
                    {
                        GlobalFilePath = FilePath;
                        FilmFilesList.Filter = new Predicate<object>(IsFileInLibtary);
                        if (FilmFilesList.Count == 0)
                        {
                            AskToAddNewFilm(FilePath);
                        }
                    }
                }
            }                        
        }

        public void AddHomeFolder(DirectoryInfo HomeFolder)
        {
            if (HomeFolder == null) return;
            string AllFoldersString = Dobrofilm.Properties.Settings.Default.HomeFolders;
            if (AllFoldersString == string.Empty)
            {
                Dobrofilm.Properties.Settings.Default.HomeFolders = HomeFolder.FullName;
            }
            else
            {
                Dobrofilm.Properties.Settings.Default.HomeFolders = string.Concat(AllFoldersString, ";", HomeFolder.FullName);
            }
            Dobrofilm.Properties.Settings.Default.Save();            
        }

        public void RemHomeFolder(DirectoryInfo HomeFolder)
        {
            if (HomeFolder == null) return;
            IList<DirectoryInfo> TempFolderList = HomeFoldersList;
            DirectoryInfo s = TempFolderList.Where(p => p.FullName == HomeFolder.FullName).Single();
            TempFolderList.Remove(s);
            Dobrofilm.Properties.Settings.Default.HomeFolders = string.Empty;
            Dobrofilm.Properties.Settings.Default.Save();
            foreach (DirectoryInfo homefolder in TempFolderList
                )
            {
                AddHomeFolder(homefolder);
            }
            //string AllFoldersString = Dobrofilm.Properties.Settings.Default.HomeFolders;
            //int index = AllFoldersString.IndexOf(HomeFolder.FullName);
            //if (index < 0) return;
            //string NewFoldersString;
            //if ((index + HomeFolder.FullName.Length) == AllFoldersString.Length)
            //{
            //    NewFoldersString =  AllFoldersString.Remove(index, HomeFolder.FullName.Length);
            //}
            //else
            //{
            //    NewFoldersString = AllFoldersString.Remove(index, HomeFolder.FullName.Length + 1);
            //}            
            //Dobrofilm.Properties.Settings.Default.HomeFolders = NewFoldersString;
            
        }
    }
}
