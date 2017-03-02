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
            DBAccess.ResetTables();
            
            //DBAccess.AddFile(new Entities.File("aaa", 1234567), new Entities.Peer("Vit", "127.0.0.1", "7777"));
            //DBAccess.AddFile(new Entities.File("aaa", 1234567), new Entities.Peer("Os", "127.0.0.1", "7778"));
            Console.WriteLine("End!");
        }
    }
}
