using System;
using QuickFix.Fields;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using System.Collections.Generic;

public class FixClient : IApplication
{
    Session _session = null;

    public void OnCreate(SessionID sessionID)
    {
        Console.WriteLine("Looking up session" + sessionID);
        _session = Session.LookupSession(sessionID);
        Console.WriteLine("Session is" + _session.ToString());
    }
    public QuickFix.Transport.SocketInitiator initiator;
    public void FromAdmin(Message message, SessionID sessionID) { }
    public void FromApp(Message message, SessionID sessionID) 
    {
        Console.WriteLine("Received Message: " + message);
    }

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
        Console.WriteLine("Starting client...");
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

    public void Run()
    {            
        while (true)
            {
                try
                {
                    char action = QueryAction();
                    if (action == '1')
                        QueryEnterOrder();
                    else if (action == 'q' || action == 'Q')
                        break;
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Message Not Sent: " + e.Message);
                    Console.WriteLine("StackTrace: " + e.StackTrace);
                }
            }
            Console.WriteLine("Program shutdown.");
        
    }


    private char QueryAction()
    {
        // Commands 'g' and 'x' are intentionally hidden.
        Console.Write("\n"
            + "1) Enter Order\n"
            + "Q) Quit\n"
            + "Action: "
        );

        HashSet<string> validActions = new HashSet<string>("1,2,3,4,q,Q,g,x".Split(','));

        string cmd = Console.ReadLine().Trim();
        if (cmd.Length != 1 || validActions.Contains(cmd) == false)
            throw new System.Exception("Invalid action");

        return cmd.ToCharArray()[0];
    }

    private void QueryEnterOrder()
    {
        Console.WriteLine("\nNewOrderSingle");

        QuickFix.FIX44.NewOrderSingle m = QueryNewOrderSingle44();

        if (m != null && QueryConfirm("Send order"))
        {
            m.Header.GetString(QuickFix.Fields.Tags.BeginString);

            SendMessage(m);
        }
    }

    private void SendMessage(Message m)
    {
        Console.WriteLine(_session.SessionID);
        if (_session != null)
            _session.Send(m);
        else
        {
            // This probably won't ever happen.
            Console.WriteLine("Can't send message: session not created.");
        }
    }

    private bool QueryConfirm(string query)
    {
        Console.WriteLine();
        Console.WriteLine(query + "?: ");
        string line = Console.ReadLine().Trim();
        return (line[0].Equals('y') || line[0].Equals('Y'));
    }

    private QuickFix.FIX44.NewOrderSingle QueryNewOrderSingle44()
    {
        QuickFix.Fields.OrdType ordType = null;

        QuickFix.FIX44.NewOrderSingle newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
            new ClOrdID("order1"),
            new Symbol("AAPL"),
            new Side(Side.BUY),
            new TransactTime(DateTime.Now),
            new OrdType(OrdType.MARKET));

        newOrderSingle.Set(new HandlInst('1'));
        newOrderSingle.Set(new OrderQty(1));
        newOrderSingle.Set(new TimeInForce(TimeInForce.DAY));

        return newOrderSingle;
    }
}



