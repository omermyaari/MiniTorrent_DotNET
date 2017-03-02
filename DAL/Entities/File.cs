using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    class File
    {
        public string Name { get; set; }
        public long Size { get; set; }

        public File(string name, long size)
        {
            Name = name;
            Size = size;

        }
    }
}
