using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Path = System.IO.Path;
using Emgu.CV.UI;
using System.Windows.Forms;
using CsvHelper.Configuration.Attributes;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using ListBox = System.Windows.Forms.ListBox;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace ASAP_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ImageHandler ImageHandler = new ImageHandler();
        private System.Drawing.Point LastClickedPoint { get; set; }
        private LengthCollector LengthCollector { get; set; }
        private bool Switch { get; set; }
        private SettingsWindow SettingsWindow { get; set; }

        private LengthWindow LengthWindow { get; set; }

        //private List<(int, double, double)> TempLengthList { get; set; }
        private List<LengthTriplet> TempLengthList { get; set; }

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text

        public MainWindow()
        {
            InitializeComponent();
            LengthCollector = new LengthCollector();
            EmguImgBox.SizeMode = PictureBoxSizeMode.Zoom;
            //hogy ne lehessen pan-elni és zoomolni
            //EmguImgBox.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
            //EmguImgBox.VerticalScrollBar.KeyPress
            Switch = false;
            SettingsWindow = new SettingsWindow {Visibility = Visibility.Collapsed};
            LengthWindow = new LengthWindow {Visibility = Visibility.Collapsed};
        }

        private void OpenFolderClick(object sender, RoutedEventArgs e)
        {
            OpenFolder();
        }

        private void ImgBoxClick(object sender, EventArgs e)
        {
            var me = (MouseEventArgs) e;
            var coordinates = me.Location;
            ImageHandler.Process();
            ImageHandler.DrawCellCountours(coordinates);
            EmguImgBox.Image = ImageHandler.CountourImage;
        }

        private void DrawContours()
        {
            Switch = !Switch;
            if (Switch)
            {
                ImageHandler.Process();
                ImageHandler.DrawAllCellCountours();
                EmguImgBox.Image = ImageHandler.CountourImage;
            }
            else
            {
                //ImageHandler.UpdateImage("EqualizeHist");
                EmguImgBox.Image = ImageHandler.Image;
            }

            RegisterCellLengths();
        }

        private void RegisterCellLengths()
        {
            if (!LengthCollector.ContainsKey(ImageHandler.OpenedImgNumber))
            {
                //LengthCollector.Add(ImageHandler.OpenedImgNumber, ImageHandler.GetAllCellLengthWithCenterPoint());
            }
        }

        private void SwitchImg()
        {
            Switch = !Switch;
            if (Switch)
            {
                ImageHandler.Process();
                EmguImgBox.Image = ImageHandler.ProcessedImage;
            }
            else
            {
                ImageHandler.UpdateImage();
                EmguImgBox.Image = ImageHandler.Image;
            }

            RegisterCellLengths();
        }

        private void OpenFolder()
        {
            var folderDialog = new CommonOpenFileDialog {IsFolderPicker = true};
            var folderResult = folderDialog.ShowDialog();
            if (folderResult != CommonFileDialogResult.Ok) return;
            textBoxFolderPath.Text = folderDialog.FileName;
            ImageHandler.UpdateFolder(textBoxFolderPath.Text);
            EmguImgBox.Image = ImageHandler.Image;
            NumOfPics.Text = ImageHandler.Files.Count.ToString();
            CurrIdx.Text = ImageHandler.OpenedImgNumber.ToString();
        }

        private void ShowSettingsWindow(object sender, RoutedEventArgs e)
        {
            SettingsWindow.Show();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProcessCurrImg();
        }

        private void ProcessCurrImg()
        {
            var currImgNumber = ImageHandler.OpenedImgNumber;
            if (textBoxFolderPath.Text.Length <= 0) return;
            ImageHandler.Process();
            LengthCollector.Add(currImgNumber, ImageHandler.GetAllCellLengthWithCenterPoint());
            //TempLengthList = LengthCollector.GetLengthList(currImgNumber, SettingsWindow.PPM_Sl.Value);
            TempLengthList = LengthCollector.GetLengthTripletList(currImgNumber, SettingsWindow.PPM_Sl.Value);
            LengthWindow.LengthGrid.ItemsSource = TempLengthList;
            LengthWindow.LengthGrid.Items.Refresh();

            /*
            foreach (var VARIABLE in ImageHandler.Files)
            {
                ImageHandler.NextImage();
                ImageHandler.ImgProcessor.Process();
                //ImageHandler.SaveImgWithCountours();
                //ImageHandler.ImgProcessor.ExportImg(Path.Combine(textBoxFolderPath.Text, ImageHandler.OpenedImgNumber + "_export.jpg"));
            }
            */
        }

        private void LengthClick(object sender, RoutedEventArgs e)
        {
            LengthWindow.Show();
            /*
            this.LengthMenuItem.IsChecked = !this.LengthMenuItem.IsChecked;
            if (this.LengthMenuItem.IsChecked)
            {
                LengthGrid.DataContext =
                    LengthCollector.GetLengthList(ImageHandler.OpenedImgNumber, Convert.ToDouble(SettingsWindow.PPM_Sl));
            }
            */
        }

        private void Window_OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {

                case Key.Return:
                    DrawContours();
                    break;
                case Key.Space:
                    SwitchImg();
                    break;
                case Key.Prior:
                    ImageHandler.NextImage();
                    CurrIdx.Text = ImageHandler.OpenedImgNumber.ToString();
                    EmguImgBox.Image = ImageHandler.Image;
                    //ImageHandler.Process();
                    //EmguImgBox.Image = ImageHandler.ProcessedImage;
                    Print("WPF");
                    break;
                case Key.Next:
                    ImageHandler.PreviousImage();
                    CurrIdx.Text = ImageHandler.OpenedImgNumber.ToString();
                    EmguImgBox.Image = ImageHandler.Image;
                    //ImageHandler.Process();
                    //EmguImgBox.Image = ImageHandler.ProcessedImage;Print("Winform");
                    Print("WPF");
                    break;
                case Key.C:
                    ImageHandler.UpdateImage(ImageHandler.ModTypeEnum.Clahe.ToString());
                    EmguImgBox.Image = ImageHandler.Image;
                    break;
                case Key.E:
                    ImageHandler.UpdateImage(ImageHandler.ModTypeEnum.EqualizeHist.ToString());
                    EmguImgBox.Image = ImageHandler.Image;
                    break;
                case Key.O:
                    OpenFolder();
                    break;
                case Key.P:
                    ProcessCurrImg();
                    break;
                default:
                    return;
            }
        }

        private void Window_OnKey(object sender, KeyEventArgs e)
        {
            //A winform dolog miatt kell ez :|
            KeyDataSwitch(e.KeyData);
        }

        private void KeyDataSwitch(Keys e)
        {
            switch (e)
            {
                case Keys.Right:
                    break;
                case Keys.Left:
                    break;
                case Keys.KeyCode:
                    break;
                case Keys.Modifiers:
                    break;
                case Keys.None:
                    break;
                case Keys.LButton:
                    break;
                case Keys.RButton:
                    break;
                case Keys.Cancel:
                    break;
                case Keys.MButton:
                    break;
                case Keys.XButton1:
                    break;
                case Keys.XButton2:
                    break;
                case Keys.Back:
                    break;
                case Keys.Tab:
                    break;
                case Keys.LineFeed:
                    break;
                case Keys.Clear:
                    break;
                case Keys.Return:
                    break;
                case Keys.ShiftKey:
                    break;
                case Keys.ControlKey:
                    break;
                case Keys.Menu:
                    break;
                case Keys.Pause:
                    break;
                case Keys.Capital:
                    break;
                case Keys.KanaMode:
                    break;
                case Keys.JunjaMode:
                    break;
                case Keys.FinalMode:
                    break;
                case Keys.HanjaMode:
                    break;
                case Keys.Escape:
                    break;
                case Keys.IMEConvert:
                    break;
                case Keys.IMENonconvert:
                    break;
                case Keys.IMEAccept:
                    break;
                case Keys.IMEModeChange:
                    break;
                case Keys.Space:
                    break;
                case Keys.Prior:
                    ImageHandler.PreviousImage();
                    CurrIdx.Text = ImageHandler.OpenedImgNumber.ToString();
                    EmguImgBox.Image = ImageHandler.Image;
                    //ImageHandler.Process();
                    //EmguImgBox.Image = ImageHandler.ProcessedImage;
                    Print("Winform");
                    break;
                case Keys.Next:
                    ImageHandler.NextImage();
                    CurrIdx.Text = ImageHandler.OpenedImgNumber.ToString();
                    EmguImgBox.Image = ImageHandler.Image;
                    //ImageHandler.Process();
                    //EmguImgBox.Image = ImageHandler.ProcessedImage;
                    Print("Winform");
                    break;
                case Keys.End:
                    break;
                case Keys.Home:
                    break;
                case Keys.Up:
                    break;
                case Keys.Down:
                    break;
                case Keys.Select:
                    break;
                case Keys.Print:
                    break;
                case Keys.Execute:
                    break;
                case Keys.Snapshot:
                    break;
                case Keys.Insert:
                    break;
                case Keys.Delete:
                    break;
                case Keys.Help:
                    break;
                case Keys.D0:
                    break;
                case Keys.D1:
                    break;
                case Keys.D2:
                    break;
                case Keys.D3:
                    break;
                case Keys.D4:
                    break;
                case Keys.D5:
                    break;
                case Keys.D6:
                    break;
                case Keys.D7:
                    break;
                case Keys.D8:
                    break;
                case Keys.D9:
                    break;
                case Keys.A:
                    break;
                case Keys.B:
                    break;
                case Keys.C:
                    break;
                case Keys.D:
                    break;
                case Keys.E:
                    break;
                case Keys.F:
                    break;
                case Keys.G:
                    break;
                case Keys.H:
                    break;
                case Keys.I:
                    break;
                case Keys.J:
                    break;
                case Keys.K:
                    break;
                case Keys.L:
                    break;
                case Keys.M:
                    break;
                case Keys.N:
                    break;
                case Keys.O:
                    break;
                case Keys.P:
                    break;
                case Keys.Q:
                    break;
                case Keys.R:
                    break;
                case Keys.S:
                    break;
                case Keys.T:
                    break;
                case Keys.U:
                    break;
                case Keys.V:
                    break;
                case Keys.W:
                    break;
                case Keys.X:
                    break;
                case Keys.Y:
                    break;
                case Keys.Z:
                    break;
                case Keys.LWin:
                    break;
                case Keys.RWin:
                    break;
                case Keys.Apps:
                    break;
                case Keys.Sleep:
                    break;
                case Keys.NumPad0:
                    break;
                case Keys.NumPad1:
                    break;
                case Keys.NumPad2:
                    break;
                case Keys.NumPad3:
                    break;
                case Keys.NumPad4:
                    break;
                case Keys.NumPad5:
                    break;
                case Keys.NumPad6:
                    break;
                case Keys.NumPad7:
                    break;
                case Keys.NumPad8:
                    break;
                case Keys.NumPad9:
                    break;
                case Keys.Multiply:
                    break;
                case Keys.Add:
                    break;
                case Keys.Separator:
                    break;
                case Keys.Subtract:
                    break;
                case Keys.Decimal:
                    break;
                case Keys.Divide:
                    break;
                case Keys.F1:
                    break;
                case Keys.F2:
                    break;
                case Keys.F3:
                    break;
                case Keys.F4:
                    break;
                case Keys.F5:
                    break;
                case Keys.F6:
                    break;
                case Keys.F7:
                    break;
                case Keys.F8:
                    break;
                case Keys.F9:
                    break;
                case Keys.F10:
                    break;
                case Keys.F11:
                    break;
                case Keys.F12:
                    break;
                case Keys.F13:
                    break;
                case Keys.F14:
                    break;
                case Keys.F15:
                    break;
                case Keys.F16:
                    break;
                case Keys.F17:
                    break;
                case Keys.F18:
                    break;
                case Keys.F19:
                    break;
                case Keys.F20:
                    break;
                case Keys.F21:
                    break;
                case Keys.F22:
                    break;
                case Keys.F23:
                    break;
                case Keys.F24:
                    break;
                case Keys.NumLock:
                    break;
                case Keys.Scroll:
                    break;
                case Keys.LShiftKey:
                    break;
                case Keys.RShiftKey:
                    break;
                case Keys.LControlKey:
                    break;
                case Keys.RControlKey:
                    break;
                case Keys.LMenu:
                    break;
                case Keys.RMenu:
                    break;
                case Keys.BrowserBack:
                    break;
                case Keys.BrowserForward:
                    break;
                case Keys.BrowserRefresh:
                    break;
                case Keys.BrowserStop:
                    break;
                case Keys.BrowserSearch:
                    break;
                case Keys.BrowserFavorites:
                    break;
                case Keys.BrowserHome:
                    break;
                case Keys.VolumeMute:
                    break;
                case Keys.VolumeDown:
                    break;
                case Keys.VolumeUp:
                    break;
                case Keys.MediaNextTrack:
                    break;
                case Keys.MediaPreviousTrack:
                    break;
                case Keys.MediaStop:
                    break;
                case Keys.MediaPlayPause:
                    break;
                case Keys.LaunchMail:
                    break;
                case Keys.SelectMedia:
                    break;
                case Keys.LaunchApplication1:
                    break;
                case Keys.LaunchApplication2:
                    break;
                case Keys.OemSemicolon:
                    break;
                case Keys.Oemplus:
                    break;
                case Keys.Oemcomma:
                    break;
                case Keys.OemMinus:
                    break;
                case Keys.OemPeriod:
                    break;
                case Keys.OemQuestion:
                    break;
                case Keys.Oemtilde:
                    break;
                case Keys.OemOpenBrackets:
                    break;
                case Keys.OemPipe:
                    break;
                case Keys.OemCloseBrackets:
                    break;
                case Keys.OemQuotes:
                    break;
                case Keys.Oem8:
                    break;
                case Keys.OemBackslash:
                    break;
                case Keys.ProcessKey:
                    break;
                case Keys.Packet:
                    break;
                case Keys.Attn:
                    break;
                case Keys.Crsel:
                    break;
                case Keys.Exsel:
                    break;
                case Keys.EraseEof:
                    break;
                case Keys.Play:
                    break;
                case Keys.Zoom:
                    break;
                case Keys.NoName:
                    break;
                case Keys.Pa1:
                    break;
                case Keys.OemClear:
                    break;
                case Keys.Shift:
                    break;
                case Keys.Control:
                    break;
                case Keys.Alt:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Print(string text)
        {
            Console.Out.WriteLineAsync(text);
        }

        private void CurrIdx_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tempText = CurrIdx.Text;
            if (!IsTextAllowed(tempText)) return;
            var idxToJumpTo = int.Parse(tempText);
            ImageHandler.JumpToImage(idxToJumpTo);
            CurrIdx.Text = ImageHandler.OpenedImgNumber.ToString();
            EmguImgBox.Image = ImageHandler.Image;
        }


        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }


        private void Window_OnKey_Prev(object sender, PreviewKeyDownEventArgs e)
        {
            KeyDataSwitch(e.KeyData);
        }

        /*
        private void MainWindow_OnPreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            e.Handled = true;
        }
        */
        private void EmguImgBox_OnClick(object sender, MouseEventArgs e)
        {
            this.LastClickedPoint = e.Location;
            this.ImageHandler.Process();
            this.ImageHandler.DrawCellCountours(this.LastClickedPoint);
            /*
             pos = evt[0]._scenePos
        mousePoint = self.view_box.mapSceneToView(pos)
        self.last_clicked_point = mousePoint
        self.image_handler.draw_cells_contour((mousePoint.x(), mousePoint.y()))

        self.update()
             */
        }
    }
}
