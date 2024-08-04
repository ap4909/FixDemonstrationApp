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
        { 10, "CheckSum" }
        // Add more FIX tags as needed
    };

    private static readonly Dictionary<string, string> MsgTypeDescriptions = new Dictionary<string, string>
    {
        { "0", "Heartbeat" }
        // Add more message types as needed
    };

    private static readonly Dictionary<int, string> FixTagDescriptions = new Dictionary<int, string>
    {
        { 8, "Identifies the beginning of a new message and protocol version." },
        { 9, "Length of the message body, in bytes, from tag 35 to tag 10 inclusive." },
        { 35, "Defines the message type." },
        { 34, "Sequence number of the message within a session. Helps ensure message ordering." },
        { 49, "Identifier of the sender's organization or system." },
        { 52, "Timestamp when the message was sent. Typically in UTC." },
        { 56, "Identifier of the intended recipient's organization or system." },
        { 10, "Checksum value calculated across the message. Used to verify message integrity." }
        // Add more FIX tag descriptions as needed
    };

    public static void ParseAndExplainFixMessage(Message fixMessage)
    {
        Console.WriteLine("Explaining the message above:");
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
    }
}
