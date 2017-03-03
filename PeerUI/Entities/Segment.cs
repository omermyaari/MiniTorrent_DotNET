﻿using System;
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

        public Segment(int Id, string FileName, long StartPosition, long Size)
        {
            this.Id = Id;
            this.FileName = FileName;
            this.StartPosition = StartPosition;
            this.Size = Size;
        }
    }
}
