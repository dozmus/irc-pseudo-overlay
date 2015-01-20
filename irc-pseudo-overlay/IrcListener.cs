using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace irc_pseudo_overlay
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

        public void Run()
        {
            // Identifying for the session
            Identify();

            // Read loop
            while (_client.Connected)
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

                    // Reading privmsg
                    if (frag[1].Equals("PRIVMSG"))
                    {
                        // Parsing input
                        string ident = frag[0];
                        // string chan = frag[2];
                        string msg = line.Substring(line.IndexOf(':') + 1);
                        string nick = ident.Split('!')[0];

                        // Displaying
                        _form.AppendLine(msg, nick);
                    }
                }
                catch
                {
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
