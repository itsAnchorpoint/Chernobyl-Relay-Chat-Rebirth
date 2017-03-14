﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chernobyl_Relay_Chat
{
    class CRCCommands
    {
        public static CRCClient client;

        private static readonly Regex commandRx = new Regex(@"^/(\S+)\s*(.*)$");

        private static readonly List<CRCCommand> commands = new List<CRCCommand>()
        {
            new CRCCommand("commands", "/commands", "Displays all available commands.", 0, false, ShowCommands),
            new CRCCommand("help", "/help [command] Use /commands to see all available commands.", "Displays information about the given command.", 1, false, ShowHelp),
            new CRCCommand("msg", "/msg [nick] [message]", "Sends a private message to another user.", 2, true, SendQuery),
            new CRCCommand("nick", "/nick [nick]", "Changes your nickname.", 1, false, ChangeNick),
            new CRCCommand("reply", "/reply [message]", "Sends a private message to the last user who sent you one.", 1, true, SendReply),
        };

        public static void ProcessCommand(string message, ICRCSendable output)
        {
            Match commandMatch = commandRx.Match(message);
            string commandString = commandMatch.Groups[1].Value;
            string args = commandMatch.Groups[2].Value;
            foreach (CRCCommand command in commands)
            {
                if (command.Name == commandString)
                {
                    command.Process(args, output);
                    return;
                }
            }
            output.AddError("Command \"" + commandString + "\" not recognized. Use /commands to see all available commands.");
        }

        private static void ShowCommands(List<string> args, ICRCSendable output)
        {
            output.AddInformation("Available commands: " + string.Join(", ", commands));
        }

        private static void ShowHelp(List<string> args, ICRCSendable output)
        {
            foreach (CRCCommand command in commands)
            {
                if (command.Name == args[0])
                {
                    output.AddInformation(command.Help);
                    return;
                }
            }
            output.AddError("Command \"" + args[0] + "\" not recognized. Use /commands to see all available commands.");
        }

        private static void SendQuery(List<string> args, ICRCSendable output)
        {
            client.SendQuery(args[0], args[1]);
        }

        private static void ChangeNick(List<string> args, ICRCSendable output)
        {
            string result = CRCStrings.ValidateNick(args[0]);
            if (result != null)
                client.ChangeNick(args[0]);
            else
                output.AddError(result);
        }

        private static void SendReply(List<string> args, ICRCSendable output)
        {
            if (!client.SendReply(args[0]))
                output.AddError("You can't reply if you haven't been sent any messages yet.");
        }
    }
}