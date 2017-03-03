using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DAL
{
    class test
    {
        static void Main(string[] args)
        {
            Console.WriteLine("start");
            DBAccess.ResetTables();
            
            DBAccess.AddFile(new Entities.DBFile("aaa", 1234567), new Entities.DBPeer("Vit", "127.0.0.1", "7777"));
     //       DBAccess.AddFile(new Entities.File("aaa", 1234567), new Entities.Peer("Os", "127.0.0.1", "7778"));

            List<Entities.DBPeer> peers = new List<Entities.DBPeer>();
            peers = DBAccess.GetPeersByFile("aaa");
            foreach (var item in peers)
            {
                Console.WriteLine(item.Ip +" "+ item.Port);
            }
            peers = DBAccess.GetPeersByFile("aaa");

            Console.WriteLine(DBAccess.FileExists("aaa"));
        }
    }
}
