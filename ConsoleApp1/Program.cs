using System;

namespace FixDemonstration{
    public class Program
    {
        static void Main()
        {
            Console.WriteLine("Starting application...");
            
            FixServer server = new FixServer();
            FixClient client = new FixClient();    

            server.Start();
            server.Stop();
        }
    }
}

