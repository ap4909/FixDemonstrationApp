using System;
using System.Timers;
using QuickFix.Fields;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;
using System.Collections.Generic;

public class FixClient : IApplication
{
    public Session _session = null;


    public void OnCreate(SessionID sessionID)
    {
        _session = Session.LookupSession(sessionID);
    }
    public QuickFix.Transport.SocketInitiator initiator;
    public void FromAdmin(Message message, SessionID sessionID) { }
    public void FromApp(Message message, SessionID sessionID) 
    {
        Console.WriteLine("Received Message: " + message);
    }

    public void OnLogon(SessionID sessionID) { }
    public void OnLogout(SessionID sessionID) { }
    public void ToAdmin(Message message, SessionID sessionID) 
    {
      
    }
    public void ToApp(Message message, SessionID sessionID) {}

    public void Start()
    {
        Console.WriteLine("Starting client...");
        SessionSettings settings = new SessionSettings("config/client.cfg");
        IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
        ILogFactory logFactory = new FileLogFactory(settings);
        IApplication application = this;
        
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
        QuickFix.FIX44.NewOrderSingle orderMessage = null;
        while (true)
            {
                try
                {
                    char action = QueryAction();
                    if (action == '1')
                        orderMessage = CreateOrderMessage(Side.BUY);
                    else if (action == '2')
                        orderMessage = CreateOrderMessage(Side.SELL);
                    else if (action == 'q' || action == 'Q')
                        break;
                    SendMessage(orderMessage);
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
        WriteOptions();

        HashSet<string> validActions = new HashSet<string>("1,2,3,4,q,Q,g,x".Split(','));

        string cmd = Console.ReadLine().Trim();
        if (cmd.Length != 1 || validActions.Contains(cmd) == false)
            throw new System.Exception("Invalid action");

        return cmd.ToCharArray()[0];
    }

    public static void WriteOptions()
    {
        Console.Write("Please select an option:\n"
            + "1) Make buy order\n"
            + "2) Make sell order\n"
            + "Q) Quit\n"
            + "Enter option: "
        );
    }

    private void SendMessage(Message m)
    {
        if (_session != null)
            _session.Send(m);
        else
        {
            // This probably won't ever happen.
            Console.WriteLine("Can't send message: session not created.");
        }
    }

    private QuickFix.FIX44.NewOrderSingle CreateOrderMessage(char side)
    {
        QuickFix.FIX44.NewOrderSingle newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
            new ClOrdID("order1"),
            new Symbol("AAPL"),
            new Side(side),
            new TransactTime(DateTime.Now),
            new OrdType(OrdType.MARKET));

        newOrderSingle.Set(new HandlInst('1'));
        newOrderSingle.Set(new OrderQty(1));
        newOrderSingle.Set(new TimeInForce(TimeInForce.DAY));

        return newOrderSingle;
    }
}



