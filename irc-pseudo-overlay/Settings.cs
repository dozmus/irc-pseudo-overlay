using System.Drawing;
using System.Xml;

namespace IrcPseudoOverlay
{
    class Settings
    {
        #region Form Variables
        public static Point DefaultLocation { get; set; }
        public static Size DefaultSize { get; set; }
        public static bool HideOnHover { get; set; }
        #endregion

        #region IRC Variables
        public static string Server { get; set; }
        public static string Channel { get; set; }
        public static string Nickname { get; set; }
        public static string Username { get; set; }
        public static string Realname { get; set; }
        public static Credentials Credentials
        {
            get
            {
                return new Credentials
                {
                    Nickname = Nickname,
                    Username = Username,
                    Realname = Realname
                };
            }
        }
        #endregion

        public static void Load(string fileName)
        {
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                #region Form Variables
                // Reading DefaultLocation
                reader.ReadToFollowing("DefaultLocation");
                string[] defLoc = reader.ReadElementContentAsString().Split(',');
                DefaultLocation = new Point(int.Parse(defLoc[0]), int.Parse(defLoc[1]));

                // Reading DefaultSize
                reader.ReadToFollowing("DefaultSize");
                string[] defSize = reader.ReadElementContentAsString().Split(',');
                DefaultSize = new Size(int.Parse(defSize[0]), int.Parse(defSize[1]));

                // Reading HideOnHover
                reader.ReadToFollowing("HideOnHover");
                HideOnHover = reader.ReadElementContentAsBoolean();
                #endregion

                #region IRC Variables
                // Reading Server
                reader.ReadToFollowing("Server");
                Server = reader.ReadElementContentAsString();

                // Reading Channel
                reader.ReadToFollowing("Channel");
                Channel = reader.ReadElementContentAsString();

                // Reading Nickname
                reader.ReadToFollowing("Nickname");
                Nickname = reader.ReadElementContentAsString();

                // Reading Username
                reader.ReadToFollowing("Username");
                Username = reader.ReadElementContentAsString();

                // Reading Realname
                reader.ReadToFollowing("Realname");
                Realname = reader.ReadElementContentAsString();
                #endregion
            }
        }
    }
}
