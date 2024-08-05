using System;
using QuickFix;

using QuickFix.Logger;
using QuickFix.Store;

public class FixServer : IApplication
{
    public ThreadedSocketAcceptor acceptor;
    public void FromAdmin(Message message, SessionID sessionID) {
        Console.WriteLine("Admin message received by server: " + message.ToString().Replace("\x01", " "));
        FixMessageInterpreter.ParseAndExplainFixMessage(message); 
        FixClient.writeOptions();
     }
    public void FromApp(Message message, SessionID sessionID) 
    {
        Console.WriteLine("Message received by server: " + message.ToString().Replace("\x01", " "));
        FixMessageInterpreter.ParseAndExplainFixMessage(message);
        FixClient.writeOptions();
    }
    public void OnCreate(SessionID sessionID) { }
    public void OnLogon(SessionID sessionID){ }
    public void OnLogout(SessionID sessionID) { }
    public void ToAdmin(Message message, SessionID sessionID) { }
    public void ToApp(Message message, SessionID sessionID) 
    {
        Console.WriteLine("Message received by server: " + message.ToString().Replace("\x01", " "));
        FixMessageInterpreter.ParseAndExplainFixMessage(message);
        FixClient.writeOptions();
    }

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
