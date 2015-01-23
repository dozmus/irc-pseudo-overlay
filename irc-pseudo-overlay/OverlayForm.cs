using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MouseKeyboardActivityMonitor;
using MouseKeyboardActivityMonitor.WinApi;

namespace IrcPseudoOverlay
{
    public partial class OverlayForm : Form
    {
        private delegate void AppendLineCallback(string message, string nickname = "");
        private readonly IrcListener _listener;
        private bool _interfaceMode;
        private bool _hiddenToHover;

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
            // TODO add: settings profiles for games [ui?], ui to change settings, reply in game?, reconnect on dc?
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

            // Mouse hook
            var mouseListener = new MouseHookListener(new GlobalHooker())
            {
                Enabled = true
            };
            mouseListener.MouseMove += MouseListener_MouseMove;

            // Starting irc listener
            _listener = new IrcListener(this, Settings.Credentials, Settings.Server, Settings.Channel);

            var ircListenerThread = new Thread(_listener.Run)
            {
                IsBackground = true
            };
            ircListenerThread.Start();
        }

        private void MouseListener_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Settings.HideOnHover)
                return;

            // Hiding overlay if hovering it and permitted to
            if (!_hiddenToHover && Visible && !_interfaceMode && Intersecting(e.Location, Location, Size))
            {
                Hide();
                _hiddenToHover = true;
            }

            // Showing overlay again if not hovering it and permitted to
            if (_hiddenToHover && !Intersecting(e.Location, Location, Size))
            {
                Show();
                _hiddenToHover = false;
            }
        }

        private static bool Intersecting(Point point, Point source, Size size)
        {
            return point.X > source.X && point.Y > source.Y
                   && point.X < source.X + size.Width && point.Y < source.Y + size.Height;
        }

        private void KeyListener_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F8: // Shut down
                    _listener.SendQuit();
                    Application.Exit();
                    break;
                case Keys.F9: // Adjust position
                    if (_hiddenToHover)
                        break;
                    _interfaceMode = !_interfaceMode;
                    SyncInterfaceState();
                    break;
                case Keys.F10: // Toggle visibility
                    if (_hiddenToHover)
                        break;
                    ToggleVisibility();
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

        private void SyncInterfaceState()
        {
            if (_interfaceMode) // Forefront
            {
                BackColor = Color.Azure;
                rtb.BackColor = Color.Azure;
                rtb.ForeColor = Color.DarkSlateGray;
                FormBorderStyle = FormBorderStyle.SizableToolWindow;
            }
            else // Background
            {
                BackColor = Color.Black;
                rtb.BackColor = Color.Black;
                rtb.ForeColor = Color.Azure;
                FormBorderStyle = FormBorderStyle.None;
            }
        }

        private void ToggleVisibility()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    }
}
