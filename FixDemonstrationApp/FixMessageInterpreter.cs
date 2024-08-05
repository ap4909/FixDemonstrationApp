using System;
using QuickFix;

public class FixMessageInterpreter
{
    private static readonly Dictionary<int, string> FixTags = new Dictionary<int, string>
    {
        { 8, "BeginString" },
        { 9, "BodyLength" },
        { 35, "MsgType" },
        { 34, "MsgSeqNum" },
        { 49, "SenderCompID" },
        { 52, "SendingTime" },
        { 56, "TargetCompID" },
        { 10, "CheckSum" },
        { 98, "EncryptMethod" },
        { 108, "HeartBtInt" },
        { 43, "PossDupFlag" },
        { 122, "OrigSendingTime" },
        { 36, "NewSeqNo" },
        { 123, "GapFillFlag" },
        { 11, "ClOrdID" },
        { 21, "HandlInst" },
        { 38, "OrderQty" },
        { 40, "OrdType" },
        { 54, "Side" },
        { 55, "Symbol" },
        { 59, "TimeInForce" },
        { 60, "TransactTime" }
    };

    private static readonly Dictionary<string, string> MsgTypeDescriptions = new Dictionary<string, string>
    {
        { "0", "Heartbeat" }
    };

    private static readonly Dictionary<int, Func<string, string>> FixValueDescriptions = new Dictionary<int, Func<string, string>>
    {
        { 8, value => $"Protocol version: {value}" },
        { 9, value => $"Message body length: {value} bytes" },
        { 35, value => $"Message type: {value} ({(MsgTypeDescriptions.ContainsKey(value) ? MsgTypeDescriptions[value] : "Unknown")})" },
        { 34, value => $"Message sequence number: {value}" },
        { 49, value => $"Sender's identifier: {value}" },
        { 52, value => $"Timestamp of sending: {value}" },
        { 56, value => $"Recipient's identifier: {value}" },
        { 10, value => $"Checksum: {value}" },
        { 98, value => value switch
            {
                "0" => "No encryption",
                "1" => "PKCS",
                "2" => "DES",
                "3" => "PKCS/DES",
                "4" => "PGP/DES",
                "5" => "PGP/DES-MD5",
                "6" => "PEM/DES-MD5",
                _ => "Unknown encryption method"
            }
        },
        { 108, value => $"Heartbeat interval: {value} seconds" },
        { 43, value => value switch
            {
                "Y" => "Possible duplicate",
                "N" => "Original transmission",
                _ => "Unknown value"
            }
        },
        { 122, value => $"Original sending time: {value}" },
        { 36, value => $"New sequence number: {value}" },
        { 123, value => value switch
            {
                "Y" => "Gap fill message",
                "N" => "Sequence reset, ignore message sequence number",
                _ => "Unknown value"
            }
        },
        { 11, value => $"Client order ID: {value}" },
        { 21, value => value switch
            {
                "1" => "Automated execution order, private, no broker intervention",
                "2" => "Automated execution order, public, broker intervention OK",
                "3" => "Manual order, best execution",
                _ => "Unknown handling instruction"
            }
        },
        { 38, value => $"Order quantity: {value}" },
        { 40, value => value switch
            {
                "1" => "Market order",
                "2" => "Limit order",
                "3" => "Stop order",
                "4" => "Stop limit order",
                _ => "Unknown order type"
            }
        },
        { 54, value => value switch
            {
                "1" => "Buy",
                "2" => "Sell",
                "3" => "Buy minus",
                "4" => "Sell plus",
                "5" => "Sell short",
                "6" => "Sell short exempt",
                _ => "Unknown side"
            }
        },
        { 55, value => $"Symbol: {value}" },
        { 59, value => value switch
            {
                "0" => "Day",
                "1" => "Good Till Cancel",
                "2" => "At the Opening",
                "3" => "Immediate or Cancel",
                "4" => "Fill or Kill",
                "5" => "Good Till Crossing",
                "6" => "Good Till Date",
                _ => "Unknown time in force"
            }
        },
        { 60, value => $"Transaction time: {value}" }
        // Add more value descriptions as needed
    };

    private static readonly Dictionary<int, string> FixTagDescriptions = new Dictionary<int, string>
    {
        { 8, "Identifies the beginning of a new message and protocol version." },
        { 9, "Length of the message body, in bytes, from tag 35 to tag 10 inclusive." },
        { 35, "Defines the message type. In this case, 0 represents a Heartbeat message." },
        { 34, "Sequence number of the message within a session. Helps ensure message ordering." },
        { 49, "Identifier of the sender's organization or system." },
        { 52, "Timestamp when the message was sent. Typically in UTC." },
        { 56, "Identifier of the intended recipient's organization or system." },
        { 10, "Checksum value calculated across the message. Used to verify message integrity." },
        { 98, "Method of encryption used for message. 0 = None, 1 = PKCS, 2 = DES, 3 = PKCS/DES, 4 = PGP/DES, 5 = PGP/DES-MD5, 6 = PEM/DES-MD5." },
        { 108, "Heartbeat interval (seconds)." },
        { 43, "Indicates possible retransmission of message with this sequence number. Y = Possible duplicate, N = Original transmission." },
        { 122, "Original time of message transmission when retransmitting." },
        { 36, "New sequence number." },
        { 123, "Indicates that the sequence reset message is replacing administrative or application messages that will not be resent. Y = Gap fill message, N = Sequence reset, ignore message sequence number." },
        { 11, "Unique identifier for Order as assigned by the buy-side." },
        { 21, "Instructions for order handling. 1 = Automated execution order, private, no broker intervention, 2 = Automated execution order, public, broker intervention OK, 3 = Manual order, best execution." },
        { 38, "Quantity ordered." },
        { 40, "Type of order. 1 = Market, 2 = Limit, 3 = Stop, 4 = Stop Limit." },
        { 54, "Side of order. 1 = Buy, 2 = Sell, 3 = Buy minus, 4 = Sell plus, 5 = Sell short, 6 = Sell short exempt." },
        { 55, "Ticker symbol (security identifier)." },
        { 59, "Time in force. 0 = Day, 1 = Good Till Cancel, 2 = At the Opening, 3 = Immediate or Cancel, 4 = Fill or Kill, 5 = Good Till Crossing, 6 = Good Till Date." },
        { 60, "Time of transaction." }
    };

    public static void ParseAndExplainFixMessage(Message fixMessage)
    {
        Console.WriteLine("======== Explaining the FIX message above ========");
        string fixMessageString = fixMessage.ToString().Replace("\x01", " ").Trim();
        string[] fields = fixMessageString.Split(' ');
        foreach (string field in fields)
        {
            string[] keyValue = field.Split('=');
            if (keyValue.Length == 2)
            {
                int tag = int.Parse(keyValue[0]);
                string value = keyValue[1];

                string tagName = FixTags.ContainsKey(tag) ? FixTags[tag] : "Unknown";
                string description = FixTagDescriptions.ContainsKey(tag) ? FixTagDescriptions[tag] : "No description available.";

                if (tag == 35) // MsgType
                {
                    description += " Message Type: " + (MsgTypeDescriptions.ContainsKey(value) ? MsgTypeDescriptions[value] : "Unknown Message Type");
                }

                Console.WriteLine($"Tag: {tag} ({tagName}) = {value} ({description})");
            }
            else
            {
                Console.WriteLine($"Invalid field: {field}");
            }
        }
        Console.WriteLine("======== FIX message explanation end ========");
    }
}
