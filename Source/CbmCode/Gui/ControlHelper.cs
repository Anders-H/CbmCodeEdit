using System.Windows.Forms;

namespace CbmCode.Gui
{
    public static class ControlHelper
    {
        public static void RadioCheckItems(int checkedIndex, params ToolStripMenuItem[] items)
        {
            for (var i = 0; i < items.Length; i++)
                items[i].Checked = i == checkedIndex;
        }
    }
}