
namespace PeerUI
{

    /// <summary>
    /// Segment, this class is used to hold file segment properties.
    /// </summary>
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
