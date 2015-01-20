using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;

namespace irc_pseudo_overlay
{
    public partial class OverlayForm : Form
    {
        private delegate void AppendLineCallback(string message, string nickname = "");
        private bool _adjustMode;

        protected override CreateParams CreateParams
        {
            get
            {
                // Hiding form from alt-tab
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        public OverlayForm()
        {
            // TODO add: settings profiles for games [maybe ui?], ui to change settings, reply in game?, reconnect on dc?
            InitializeComponent();
            Settings.Load("Resources/Settings.xml");
            Size = rtb.Size = Settings.DefaultSize;
            DesktopLocation = Settings.DefaultLocation;

            // Keyboard hook
            var keyListener = new KeyboardHookListener(new GlobalHooker())
            {
                Enabled = true
            };
            keyListener.KeyUp += KeyListener_KeyUp;

            // Starting irc listener
            var listener = new IrcListener(this, Settings.Credentials, Settings.Server, Settings.Channel);

            var ircListenerThread = new Thread(listener.Run)
            {
                IsBackground = true
            };
            ircListenerThread.Start();
        }

        private void KeyListener_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F8: // Shut down
                    Application.Exit();
                    break;
                case Keys.F9: // Adjust position
                    _adjustMode = !_adjustMode;
                    SyncMode();
                    break;
            }
        }

        public void AppendLine(string message, string nickname = "")
        {
            if (rtb.InvokeRequired)
            {
                Invoke(new AppendLineCallback(AppendLine), new object[] { message, nickname });
            }
            else
            {
                rtb.AppendText(nickname.Length == 0
                    ? String.Format("{0}{1}", message, Environment.NewLine)
                    : String.Format("<{0}> {1}{2}", nickname, message, Environment.NewLine));
            }
        }

        private void rtb_TextChanged(object sender, EventArgs e)
        {
            rtb.SelectionStart = rtb.Text.Length;
            rtb.ScrollToCaret();
        }

        private void SyncMode()
        {
            if (_adjustMode)
            {
                ForefrontMode();
            }
            else
            {
                BackgroundMode();
            }
        }

        private void ForefrontMode()
        {
            BackColor = Color.Azure;
            rtb.BackColor = Color.Azure;
            rtb.ForeColor = Color.DarkSlateGray;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
        }

        private void BackgroundMode()
        {
            BackColor = Color.Black;
            rtb.BackColor = Color.Black;
            rtb.ForeColor = Color.Azure;
            FormBorderStyle = FormBorderStyle.None;
        }
    }
}
