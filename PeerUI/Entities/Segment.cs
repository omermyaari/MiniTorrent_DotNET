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
        public string FileName { set; get; }
        public long StartPosition { set; get; }
        public long SegmentSize { set; get; }

        public Segment() { }

        public Segment(string fileName, long position, long size)
        {
            FileName = fileName;
            StartPosition = position;
            SegmentSize = size;
        }
    }
}
