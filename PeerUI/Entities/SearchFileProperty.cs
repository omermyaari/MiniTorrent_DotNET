
namespace PeerUI.Entities {

    /// <summary>
    /// Search file property.
    /// used by the UI in the search tab, holds basic information of files searched by the user.
    /// </summary>
    public class SearchFileProperty {
        public string Name {
            get; set;
        }
        public long Size {
            get; set;
        }
        public int Peers {
            get; set;
        }

        public SearchFileProperty(string Name, long Size, int Peers) {
            this.Name = Name;
            this.Size = Size;
            this.Peers = Peers;
        }
    }
}
