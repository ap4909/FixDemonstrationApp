using System;
using QuickFix;

using QuickFix.Logger;
using QuickFix.Store;

public class FixClient : IApplication
{
    public QuickFix.Transport.SocketInitiator initiator;
    public void FromAdmin(Message message, SessionID sessionID) { }
    public void FromApp(Message message, SessionID sessionID) 
    {
        Console.WriteLine("Received Message: " + message);
    }
    public void OnCreate(SessionID sessionID) { }
    public void OnLogon(SessionID sessionID) 
    {
        Console.WriteLine("Connected to Server: " + sessionID);
    }
    public void OnLogout(SessionID sessionID) 
    {
        Console.WriteLine("Disconnected from Server: " + sessionID);
    }
    public void ToAdmin(Message message, SessionID sessionID) { }
    public void ToApp(Message message, SessionID sessionID) { }

    public void Start()
    {
        SessionSettings settings = new SessionSettings("config/client.cfg");
        IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
        ILogFactory logFactory = new FileLogFactory(settings);
        IApplication application = new FixClient();
        
        initiator = new QuickFix.Transport.SocketInitiator(application, storeFactory, settings, logFactory);
        //ThreadedSocketInitiator initiator = new ThreadedSocketInitiator(application, storeFactory, settings, logFactory);
        initiator.Start();
    }

    public void Stop()
    {
        Console.WriteLine("Stopping client");
        initiator.Stop();
    }
}
