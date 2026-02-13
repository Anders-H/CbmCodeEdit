using System.Windows.Forms;

namespace CbmCode.Gui;

public static class OpenSaveDialogs
{
    public static OpenFileDialog GetOpenDialog() =>
        new()
        {
            Title = @"Open CBM code",
            Filter = @"Source code files (*.ccde)|*.ccde|All files (*.*)|*.*"
        };

    public static SaveFileDialog GetSaveAsDialog() =>
        new()
        {

            Title = @"Save CBM code",
            Filter = @"Source code files (*.ccde)|*.ccde|All files (*.*)|*.*"
        };

    public static SaveFileDialog GetSaveBuildResultDialog() =>
        new()
        {
            Title = @"Save generated code",
            Filter = @"Text files (*.txt)|*.txt|All files (*.*)|*.*"
        };
}