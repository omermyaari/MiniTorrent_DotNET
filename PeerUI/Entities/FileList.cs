using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerUI.Entities
{
    class FileList
    {
        public ObservableCollection<MyFileInfo> Files { get; private set; }

        public FileList()
        {
            Files = new ObservableCollection<MyFileInfo>();            
        }

        public void RefreshFiles(List<DataFile> listOfFiles)
        {
            Files.Clear();
            for (int i = 0; i < listOfFiles.Count; i++)
                Files.Add(new MyFileInfo(listOfFiles[i].FileName, listOfFiles[i].FileSize, listOfFiles[i].UsersList.Count));        
        }
    }

    public class MyFileInfo
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public int NumOfPeers { get; set; }

        public MyFileInfo(string fileName, long fileSize, int count)
        {
            Name = fileName;
            Size = fileSize;
            NumOfPeers = count;
        }
    }
}
