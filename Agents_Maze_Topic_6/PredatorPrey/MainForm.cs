using System.Windows.Forms;
using System.Drawing;

namespace AgentsMaze
{
    public partial class MainForm : Form
    {
        private MazeMapUI mapUI;
        private TextBox textBox;
        public Cell[][] map = null;

        public MainForm(Cell[][] _map)
        {
            map = _map;
            mapUI = new MazeMapUI(15, 15, 600, 600, map);
            textBox = new TextBox();
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.ReadOnly = true;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.White;
            textBox.Location = new Point(640, 15);
            textBox.Size = new Size(300, 600);
            this.Controls.Add(mapUI);
            this.Controls.Add(textBox);
            Form.CheckForIllegalCrossThreadCalls = false;

            InitializeComponent();
        }

        public void AddText(string text)
        {
            textBox.Text += text;
            textBox.SelectionStart = textBox.Text.Length;
            textBox.ScrollToCaret();
        }

        private void MainForm_Load(object sender, System.EventArgs e)
        {

        }
    }
}
