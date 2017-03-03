using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DBFile
    {
        private static long id = 0;

        public long ID { get; private set; }
        public string Name { get; set; }
        public long Size { get; set; }


        public DBFile(string Name, long Size)
        {
            ID = id++;
            this.Name = Name;
            this.Size = Size;
        }
    }
}
