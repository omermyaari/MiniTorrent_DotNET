using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    class Segment
    {
        public string FileName { set; get; }
        public long startPosition { set; get; }
        public long size { set; get; }
    }
}
