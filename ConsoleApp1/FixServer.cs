using System;
using QuickFix;

using QuickFix.Logger;
using QuickFix.Store;

public class FixServer : IApplication
{
    public ThreadedSocketAcceptor acceptor;
    public void FromAdmin(Message message, SessionID sessionID) { }
    public void FromApp(Message message, SessionID sessionID) 
    {
        Console.WriteLine("Received Message: " + message);
    }
    public void OnCreate(SessionID sessionID) { }
    public void OnLogon(SessionID sessionID) 
    {
        Console.WriteLine("Client Logged In: " + sessionID);
    }
    public void OnLogout(SessionID sessionID) 
    {
        Console.WriteLine("Client Logged Out: " + sessionID);
    }
    public void ToAdmin(Message message, SessionID sessionID) { }
    public void ToApp(Message message, SessionID sessionID) { }

    public void Start()
    {
        Console.WriteLine("Starting server...");
        SessionSettings settings = new SessionSettings("config/server.cfg");
        IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
        ILogFactory logFactory = new FileLogFactory(settings);
        IApplication application = this;

        acceptor = new ThreadedSocketAcceptor(application, storeFactory, settings, logFactory);
        acceptor.Start();
        Console.WriteLine("Server started");
    }

    public void Stop(){
        Console.WriteLine("Stopping server");
        acceptor.Stop();
    }
}
