using System.Windows.Forms;

namespace CbmCode.Gui
{
    public static class MessageDisplayer
    {
        public static bool Ask(Form owner, string text, string title)
        {
            owner.Cursor = Cursors.Default;
            return MessageBox.Show(owner, text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK;
        }

        public static void Information(Form owner, string text)
        {
            owner.Cursor = Cursors.Default;
            MessageBox.Show(owner, text, owner.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Error(Form owner, string text)
        {
            owner.Cursor = Cursors.Default;
            MessageBox.Show(owner, text, owner.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}