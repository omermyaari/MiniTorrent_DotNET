using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TorrentWcfServiceLibrary {
    public class TorrentWcfService : ITorrentWcfService {

        //  Searches in the DB and returns a list of files and peers sharing them.
        public List<SearchResult> SearchFile(string FileName) {
            if (FileName == null) {
                return null;
            }
            List<SearchResult> searchResults = new List<SearchResult>();
            //  TODO generate list from db;
            var sr = new SearchResult {
                FileName = "Shitfuckdickface",
                FileSize = 1955,
                PeerList = new Dictionary<string, string> {
                    {"10.0.0.1", "4001"}
                }
            };
            searchResults.Add(sr);
            return searchResults;
        }
    }
}
