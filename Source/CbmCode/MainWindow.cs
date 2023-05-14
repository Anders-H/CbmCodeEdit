using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CbmCode
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var x = new OpenFileDialog())
            {
                x.Filter = @"Source code files (*.ccde)|*.ccde|All files (*.*)|*.*";
                x.Title = @"Open CBM code";

                if (x.ShowDialog(this) == DialogResult.OK)
                {
                    LoadFile(x.FileName);
                }
            }
        }

        private void LoadFile(string filename)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                string code;

                using (var sr = new StreamReader(filename, Encoding.UTF8))
                {
                    code = sr.ReadToEnd();
                    sr.Close();
                }

                ClearUndoBuffer();
                PushCurrentFile(filename);
                rtbIn.Text = code;
                Cursor = Cursors.Default;
            }
            catch (Exception e)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($@"Load failed: {e.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearUndoBuffer()
        {
            // TODO
        }

        private void PushCurrentFile(string filename)
        {
            // TODO
        }

        private void outputWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            outputWindowToolStripMenuItem.Checked = !outputWindowToolStripMenuItem.Checked;
            splitContainer1.Panel2Collapsed = !outputWindowToolStripMenuItem.Checked;
        }
    }
}