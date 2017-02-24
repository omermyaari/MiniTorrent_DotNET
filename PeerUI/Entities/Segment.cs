using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerUI
{
    class Segment
    {
        public string FileName { set; get; }
        public long StartPosition { set; get; }
        public long Size { set; get; }

        public Segment(string fileName, long position, long size)
        {
            FileName = fileName;
            StartPosition = position;
            Size = size;
        }
    }
}
