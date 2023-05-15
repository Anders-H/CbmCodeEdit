using System;
using System.Text;
using System.Windows.Forms;
using CbmCode.CodeGeneration;
using CbmCode.Gui;
using CbmCode.Io;

namespace CbmCode
{
    public partial class MainWindow : Form
    {
        private string _currentFilename;
        private bool _dirtyFlag;

        public MainWindow()
        {
            InitializeComponent();
            _currentFilename = "";
            _dirtyFlag = false;
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e) =>
            Close();

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_dirtyFlag)
            {
                if (MessageBox.Show(@"You have unsaved changes. Continue?", @"Open", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return;
            }

            using (var x = OpenSaveDialogs.GetOpenDialog())
            {
                if (x.ShowDialog(this) == DialogResult.OK)
                    LoadFile(x.FileName);
            }
        }

        private void LoadFile(string filename)
        {
            Cursor = Cursors.WaitCursor;

            try
            {
                var code = Storage.LoadFile(filename);
                ClearUndoBuffer();
                PushCurrentFile(filename);
                rtbIn.Text = code;
                _dirtyFlag = false;
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
            _currentFilename = filename;

            if (string.IsNullOrEmpty(_currentFilename))
            {
                Text = @"CBM-Code";
                return;
            }

            Text = $@"CBM-Code [{filename}]";
        }

        private void outputWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            outputWindowToolStripMenuItem.Checked = !outputWindowToolStripMenuItem.Checked;
            splitContainer1.Panel2Collapsed = !outputWindowToolStripMenuItem.Checked;
        }

        private void buildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rightPaneToolStripMenuItem.Checked && splitContainer1.Panel2Collapsed)
                outputWindowToolStripMenuItem_Click(sender, e);

            Cursor = Cursors.WaitCursor;

            var code = rtbIn.Text.Split('\n', '\r');

            var generatedCode = new Generate(code).Do();

            if (!generatedCode.success)
            {
                rtbOut.Text = "";
                Cursor = Cursors.Default;
                MessageBox.Show(@"Code generation failed.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (generatedCode.generatedLines.Count <= 0)
            {
                rtbOut.Text = "";
                Cursor = Cursors.Default;
                MessageBox.Show(@"Code generation did nog give any result.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (rightPaneToolStripMenuItem.Checked)
            {
                var s = new StringBuilder();

                foreach (var generatedCodeGeneratedLine in generatedCode.generatedLines)
                    s.AppendLine(generatedCodeGeneratedLine);

                rtbOut.Text = s.ToString();
                Cursor = Cursors.Default;
            }
            else if (clipboardToolStripMenuItem.Checked)
            {
                var s = new StringBuilder();

                foreach (var generatedCodeGeneratedLine in generatedCode.generatedLines)
                    s.AppendLine(generatedCodeGeneratedLine);

                Clipboard.SetText(s.ToString());
                Cursor = Cursors.Default;
                MessageBox.Show($@"{generatedCode.generatedLines.Count} lines of code copied.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (fileToolStripMenuItem1.Checked)
            {
                Cursor = Cursors.Default;
                using (var x = OpenSaveDialogs.GetSaveBuildResultDialog())
                {
                    if (x.ShowDialog(this) != DialogResult.OK)
                        return;

                    try
                    {
                        Storage.SaveFile(x.FileName, generatedCode.generatedLines);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show($@"Save failed: {exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            Cursor = Cursors.Default;
        }

        private void clipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clipboardToolStripMenuItem.Checked = true;
            rightPaneToolStripMenuItem.Checked = false;
            fileToolStripMenuItem1.Checked = false;
        }

        private void rightPaneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clipboardToolStripMenuItem.Checked = false;
            rightPaneToolStripMenuItem.Checked = true;
            fileToolStripMenuItem1.Checked = false;
        }

        private void fileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            clipboardToolStripMenuItem.Checked = false;
            rightPaneToolStripMenuItem.Checked = false;
            fileToolStripMenuItem1.Checked = true;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_dirtyFlag)
            {
                if (MessageBox.Show(@"You have unsaved changes. Continue?", @"New", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return;
            }

            rtbIn.Text = "";
            rtbOut.Text = "";
            _dirtyFlag = false;
            PushCurrentFile("");
        }

        private void rtbIn_TextChanged(object sender, EventArgs e)
        {
            _dirtyFlag = true;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_dirtyFlag && e.CloseReason == CloseReason.UserClosing)
            {
                if (MessageBox.Show(@"You have unsaved changes. Quit?", @"Quit", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    e.Cancel = true;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentFilename))
            {
                saveAsToolStripMenuItem_Click(sender, e);
                return;
            }

            Save(_currentFilename);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var x = OpenSaveDialogs.GetSaveAsDialog())
            {
                if (x.ShowDialog(this) == DialogResult.OK)
                    Save(x.FileName);
            }
        }

        private void Save(string filename)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                Storage.SaveFile(filename, rtbIn.Text);
                _dirtyFlag = false;
                PushCurrentFile("");
                Cursor = Cursors.Default;
            }
            catch (Exception e)
            {
                Cursor = Cursors.Default;
                MessageBox.Show($@"Save failed: {e.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}