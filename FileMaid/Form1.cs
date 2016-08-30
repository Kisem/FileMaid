using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileMaid
{
    public partial class MainForm : Form
    {
        ContextMenu contextMenu = new ContextMenu();
        List<MenuItem> menuItems = new List<MenuItem>();

        public MainForm()
        {
            InitializeComponent();

            menuItems.Add(new MenuItem("Show", new EventHandler(this.menuitem_Show)));
            menuItems.Add(new MenuItem("Exit", new EventHandler(this.menuitem_Exit)));
            contextMenu.MenuItems.AddRange(menuItems.ToArray());
            notifyIcon.ContextMenu = contextMenu;

            FormClosing += MainForm_FormClosing;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            FileManager.Initialize();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                notifyIcon.Visible = true;
                Hide();
                e.Cancel = true;
            }
            else
            {
                notifyIcon.Dispose();
            }
        }

        private void menuitem_Show(object sender, EventArgs e)
        {
            Show();
        }
        private void menuitem_Exit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Show();
            }
        }
    }
}
