using CbmCode.CodeGeneration;
using CbmCode.Gui;
using CbmCode.Io;
using CbmCode.Text;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CbmCode
{
    public partial class MainWindow : Form
    {
        private string _currentFilename;
        private bool _dirtyFlag;
        private readonly UndoBuffer _undoBuffer;
        private Thread _undoThread;
        private bool _undoOperationInProgress;

        public MainWindow()
        {
            InitializeComponent();
            _currentFilename = "";
            _dirtyFlag = false;
            _undoBuffer = new UndoBuffer();
            _undoOperationInProgress = false;
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e) =>
            Close();

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_dirtyFlag && !MessageDisplayer.Ask(this, @"You have unsaved changes. Continue?", @"Open"))
                return;

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
                MessageDisplayer.Error(this, $@"Load failed: {e.Message}");
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

            var generatedCode = new Generate(rtbIn.Text.SplitLines()).Do();

            if (!generatedCode.success)
            {
                rtbOut.Text = "";
                MessageDisplayer.Error(this, @"Code generation failed.");
                return;
            }

            var generatedLines = generatedCode.codeGenerations.Last();

            if (generatedLines.Count <= 0)
            {
                rtbOut.Text = "";
                MessageDisplayer.Information(this, @"Code generation did not give any result.");
            }

            if (rightPaneToolStripMenuItem.Checked)
            {
                rtbOut.Text = generatedLines.Join();
                Cursor = Cursors.Default;
            }
            else if (clipboardToolStripMenuItem.Checked)
            {
                Clipboard.SetText(generatedLines.Join());
                MessageDisplayer.Information(this, $@"{generatedLines.Count} lines of code copied.");
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
                        Storage.SaveFile(x.FileName, generatedLines);
                    }
                    catch (Exception exception)
                    {
                        MessageDisplayer.Error(this, $@"Save failed: {exception.Message}");
                    }
                }
            }
            Cursor = Cursors.Default;
        }

        private void clipboardToolStripMenuItem_Click(object sender, EventArgs e) =>
            ControlHelper.RadioCheckItems(0, clipboardToolStripMenuItem, rightPaneToolStripMenuItem, fileToolStripMenuItem1);

        private void rightPaneToolStripMenuItem_Click(object sender, EventArgs e) =>
            ControlHelper.RadioCheckItems(1, clipboardToolStripMenuItem, rightPaneToolStripMenuItem, fileToolStripMenuItem1);
        
        private void fileToolStripMenuItem1_Click(object sender, EventArgs e) =>
            ControlHelper.RadioCheckItems(2, clipboardToolStripMenuItem, rightPaneToolStripMenuItem, fileToolStripMenuItem1);

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_dirtyFlag && !MessageDisplayer.Ask(this, @"You have unsaved changes. Continue?", @"New"))
                return;

            rtbIn.Text = "";
            rtbOut.Text = "";
            _dirtyFlag = false;
            PushCurrentFile("");
        }

        private void rtbIn_TextChanged(object sender, EventArgs e) =>
            _dirtyFlag = true;

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_dirtyFlag && e.CloseReason == CloseReason.UserClosing && !MessageDisplayer.Ask(this, @"You have unsaved changes. Quit?", @"Quit"))
                e.Cancel = true;
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
                MessageDisplayer.Error(this, $@"Save failed: {e.Message}");
            }
        }

        private void btnNew_Click(object sender, EventArgs e) =>
            newToolStripMenuItem_Click(sender, e);

        private void btnOpen_Click(object sender, EventArgs e) =>
            openToolStripMenuItem_Click(sender, e);

        private void btnSave_Click(object sender, EventArgs e) =>
            saveToolStripMenuItem_Click(sender, e);

        private void openGithubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/Anders-H/CbmCodeEdit");
            }
            catch (Exception exception)
            {
                MessageDisplayer.Error(this, $"Failed to open Github. {exception.Message}");
            }
        }

        private void btnBuild_Click(object sender, EventArgs e) =>
            buildToolStripMenuItem_Click(sender, e);

        private void btnUndo_Click(object sender, EventArgs e) =>
            undoToolStripMenuItem_Click(sender, e);

        private void btnRedo_Click(object sender, EventArgs e) =>
            redoToolStripMenuItem_Click(sender, e);

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateUndoRedoStatus();

            if (!_undoBuffer.CanUndo)
                return;

            Cursor = Cursors.WaitCursor;

            while (_undoOperationInProgress)
                Thread.Sleep(10);

            if (_undoBuffer.CanUndo)
                rtbIn.Text = _undoBuffer.Undo();

            UpdateUndoRedoStatus();
            Cursor = Cursors.Default;
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateUndoRedoStatus();

            if (!_undoBuffer.CanRedo)
                return;

            Cursor = Cursors.WaitCursor;

            while (_undoOperationInProgress)
                Thread.Sleep(10);

            if (_undoBuffer.CanRedo)
                rtbIn.Text = _undoBuffer.Redo();

            UpdateUndoRedoStatus();
            Cursor = Cursors.Default;
        }

        private void UpdateUndoRedoStatus()
        {
            undoToolStripMenuItem.Enabled = _undoBuffer.CanUndo;
            btnUndo.Enabled = undoToolStripMenuItem.Enabled;
            redoToolStripMenuItem.Enabled = _undoBuffer.CanRedo;
            btnRedo.Enabled = redoToolStripMenuItem.Enabled;
        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            var pushUndoState = new Action(PushUndoState);

            _undoThread = new Thread(() =>
            {
                do
                {
                    Thread.Sleep(2000);
                    Invoke(pushUndoState);
                    Thread.Sleep(2000);
                } while (true);
            });

            _undoThread.Start();
        }

        private void PushUndoState()
        {
            _undoBuffer.PushState(rtbIn.Text);
            UpdateUndoRedoStatus();
            _undoBuffer.GetBufferState(out var bufferSize, out var indexPointer);
            Debug.WriteLine($"Buffer size: {bufferSize}, index pointer: {indexPointer}");
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                _undoThread.Abort();
            }
            catch
            {
                // ignored
            }
        }
    }
}