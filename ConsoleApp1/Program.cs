using System;

namespace FixDemonstration{
    public class Program
    {
        static void Main()
        {
            Console.WriteLine("Starting application...");
            string configFile = "config/server.cfg";
            
            FixServer server = new FixServer();
            server.Start(configFile);
        }
    }
}

