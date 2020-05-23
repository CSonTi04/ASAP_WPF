using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ASAP_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ImageHandler imageHandler = new ImageHandler();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFolderClick(object sender, RoutedEventArgs e)
        {
            var folderDialog = new CommonOpenFileDialog {IsFolderPicker = true};
            var folderResult = folderDialog.ShowDialog();
            if (folderResult == CommonFileDialogResult.Ok)
            {
                textBoxFolderPath.Text = folderDialog.FileName;
                imageHandler.UpdateFolder(textBoxFolderPath.Text);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

            if (textBoxFolderPath.Text.Length > 0)
            {
                imageHandler.ImgProcessor.Process();
            }
        }
    }
}
