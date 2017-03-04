using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;


namespace DAL {
    class test {
        static void Main(string[] args) {
            Console.WriteLine("start");
            DBAccess.ResetTables();

            /*        DBAccess.AddFile(new Entities.File("aaa", 1234567), new Peer("Vit","Vit", "127.0.0.1", "7777", true));
                    DBAccess.AddFile(new Entities.File("aaa", 1234567), new Peer("Os", "Os", "127.0.0.1", "7778", true));
                    DBAccess.AddFile(new Entities.File("bbb", 12345678), new Peer("Os", "Os", "127.0.0.1", "7778", true));

                    List<Entities.Peer> peers = new List<Entities.Peer>();
                    peers = DBAccess.GetPeersByFile("aaa");
                    foreach (var item in peers)
                    {
                        Console.WriteLine(item.Ip +" "+ item.Port);
                    }
                    peers = DBAccess.GetPeersByFile("aaa");*/

            //  DBAccess.SetPeerAsOnline("Os");
            DBAccess.ResetTables();
        }
    }
}
