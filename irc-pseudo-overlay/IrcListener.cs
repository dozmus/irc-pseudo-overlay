using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace IrcPseudoOverlay
{
    class IrcListener
    {
        private readonly TcpClient _client = new TcpClient();
        private readonly OverlayForm _form;
        private readonly StreamWriter _writer;
        private readonly StreamReader _reader;
        private readonly Credentials _creds;
        private readonly string _channel;

        public IrcListener(OverlayForm form, Credentials creds, string server, string channel)
        {
            // Setting fields
            _form = form;
            _creds = creds;
            _channel = channel;

            // Initialising stream
            string[] serverFrag = server.Split(':');
            _client.Connect(serverFrag[0], int.Parse(serverFrag[1]));
            _writer = new StreamWriter(_client.GetStream())
            {
                AutoFlush = true
            };
            _reader = new StreamReader(_client.GetStream());
        }

        private void Identify()
        {
            _writer.WriteLine("USER {0} 8 * :{1}", _creds.Username, _creds.Realname);
            _writer.WriteLine("NICK {0}", _creds.Nickname);
        }

        public void Quit(string reason="Application exit.")
        {
            _writer.WriteLine("QUIT :{0}", reason);
        }

        public void Run()
        {
            // Identifying for the session
            Identify();

            // Read loop
            while (true)
            {
                try
                {
                    // Reading and normalising input
                    string line = _reader.ReadLine();

                    if (line == null)
                        continue;

                    if (line.StartsWith(":"))
                        line = line.Substring(1);

                    Debug.WriteLine("> " + line);

                    // Ping-pong
                    if (line.StartsWith("PING"))
                    {
                        _writer.WriteLine(line.Replace("PING", "PONG"));
                    }

                    // Checking if line is a privmsg
                    string[] frag = line.Split(' ');

                    if (frag.Length < 2)
                        continue;

                    // Joining channels on-connect
                    if (frag[1].Equals("001"))
                    {
                        _writer.WriteLine("JOIN {0}", _channel);
                    }

                    // Displaying that we are now in a channel
                    if (frag[1].Equals("332"))
                    {
                        string channel = frag[3];
                        _form.AppendLine("Now talking on " + channel);
                    }
                    
                    // Parsing
                    string ident = frag[0];
                    string nick = ident.IndexOf('!') != -1 ? ident.Split('!')[0] : null;

                    if (nick == null)
                        continue;

                    // Reading join/part/quit
                    if (!nick.Equals(Settings.Credentials.Nickname))
                    {
                        string msg = line.Substring(line.IndexOf(':') + 1);

                        switch (frag[1])
                        {
                            case "JOIN":
                                _form.AppendLine("* " + nick + " has joined");
                                break;
                            case "PART":
                                _form.AppendLine("* " + nick + " has left (" + msg + ")");
                                break;
                            case "QUIT":
                                _form.AppendLine("* " + nick + " has quit (" + msg + ")");
                                break;
                        }
                    }

                    // Reading privmsg
                    if (frag[1].Equals("PRIVMSG"))
                    {
                        string msg = line.Substring(line.IndexOf(':') + 1);

                        // Displaying received text
                        if (msg.IndexOf(Settings.Credentials.Nickname, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            _form.AppendHighlightLine(msg, nick, Color.LightGoldenrodYellow);
                        }
                        else
                        {
                            _form.AppendLine(msg, nick);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("[Exception:IrcListener#Run]: " + e.Message + "\r\n" + e.StackTrace);
                }
            }
        }
    }

    class Credentials
    {
        public string Nickname { get; set; }
        public string Username { get; set; }
        public string Realname { get; set; }
    }
}
