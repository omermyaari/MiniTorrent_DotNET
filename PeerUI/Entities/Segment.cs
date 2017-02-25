using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerUI
{
    public enum SegmentInfo {FileName, StartPosition, Size};

    public class Segment
    {
        public int Id { get; set; }
        public string FileName { set; get; }
        public long StartPosition { set; get; }
        public long Size { set; get; }

        public Segment() { }

        public Segment(int id, string fileName, long position, long size)
        {
            Id = id;
            FileName = fileName;
            StartPosition = position;
            Size = size;
        }
    }
}
