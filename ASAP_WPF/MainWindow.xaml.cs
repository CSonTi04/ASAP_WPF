using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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
using Emgu.CV;
using Emgu.CV.Structure;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using ListBox = System.Windows.Forms.ListBox;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Point = System.Drawing.Point;

namespace ASAP_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //ImageHandler ImageHandler = new ImageHandler();
        private ImageHandler ImageHandler { get; set; }

        //https://stackoverflow.com/questions/14262143/wpf-displaying-emgu-image-using-binding
        private Mat ImgToDisplay { get; set; }
        private System.Drawing.Point LastClickedPoint { get; set; }
        private LengthCollector LengthCollector { get; set; }
        private bool Switch { get; set; }
        private SettingsWindow SettingsWindow { get; set; }

        private LengthWindow LengthWindow { get; set; }
        public static ImageProcessorExaminer ImageProcessorExaminer { get; set; }

        //private List<(int, double, double)> TempLengthList { get; set; }
        private List<LengthTriplet> TempLengthList { get; set; }

        private static readonly Regex Regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text

        private double SelectedCellLength { get; set; }
        private Point SelectedCellCenterPoint { get; set; }

        private ImageToDisplay CurrentImageToDisplay { get; set; }

        public enum ImageToDisplay
        {
            None,
            Original,
            Processed,
            OriginalWithOverlay,
            ProcessedWithOverlay
        }

        public MainWindow()
        {
            InitializeComponent();
            CurrentImageToDisplay = ImageToDisplay.None;
            ImageHandler = new ImageHandler();
            LengthCollector = new LengthCollector();
            EmguImgBox.SizeMode = PictureBoxSizeMode.Zoom;
            //hogy ne lehessen pan-elni és zoomolni
            //EmguImgBox.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
            //EmguImgBox.VerticalScrollBar.KeyPress
            ImgToDisplay = new Mat();
            Switch = false;
            OgImgBtn.IsChecked = true;
            ProcessedImgBtn.IsChecked = false;
            SettingsWindow = new SettingsWindow {Visibility = Visibility.Collapsed};
            LengthWindow = new LengthWindow {Visibility = Visibility.Collapsed};
            ImageProcessorExaminer = new ImageProcessorExaminer()
            {
                Visibility = Visibility.Collapsed
            };
        }

        public void UpdateImgBox()
        {
            if (null == ImageHandler.FolderName) return;
            //if(_originIsMousClick) this.ImageHandler.DrawCellContour(this.LastClickedPoint);





            var overlayCbBool = this.OverlayCheckBox.IsChecked;
            var processedImgBtnBool = this.ProcessedImgBtn.IsChecked;
            var ogImgBtnBool = this.OgImgBtn.IsChecked;

            if (overlayCbBool != null && (bool) overlayCbBool)
            {
                if (ogImgBtnBool != null && (bool) ogImgBtnBool)
                {
                    //ImgToDisplay = ImageHandler.OgImgWithContourOverlay;
                    CurrentImageToDisplay = ImageToDisplay.OriginalWithOverlay;
                }
                else if (processedImgBtnBool != null && (bool) processedImgBtnBool)
                {
                    //ImgToDisplay = ImageHandler.ProcessedImgWithContourOverlay;
                    CurrentImageToDisplay = ImageToDisplay.ProcessedWithOverlay;
                }
            }
            else
            {

                if (ogImgBtnBool != null && (bool) ogImgBtnBool)
                {
                    //ImgToDisplay = ImageHandler.Image;
                    CurrentImageToDisplay = ImageToDisplay.Original;
                }
                else if (processedImgBtnBool != null && (bool) processedImgBtnBool)
                {
                    //ImgToDisplay = ImageHandler.ProcessedImage;
                    CurrentImageToDisplay = ImageToDisplay.Processed;
                }
            }

            EmguImgBox.Image = ImageHandler.GetImageToDisplay(this.CurrentImageToDisplay);

            /*
            if (null == this.ImageHandler.ImgName || overlayCbBool == null || !(bool) overlayCbBool || !originIsMousClick) return;
            //Mat tempImg = new Mat();
            //Emgu.CV.CvInvoke.Merge();
            //https://stackoverflow.com/questions/40895785/using-opencv-to-overlay-transparent-image-onto-another-image
            //https://stackoverflow.com/questions/36921496/how-to-join-png-with-alpha-transparency-in-a-frame-in-realtime/37198079#37198079
            //Na mi legyen? kombináljam a képeket, vagy csak vetítsem rá?
            //Na így utólag a zoomolás miatt ez nem tűnik annyira jó ötletnek :|
            //EmguImgBoxOverlay.Image = ImageHandler.CountourImage;
            ImageHandler.ProcessOverlays(this.LastClickedPoint);

            if (ogImgBtnBool != null && (bool)ogImgBtnBool)
            {
                ImgToDisplay = ImageHandler.OgImgWithContourOverlay;
            }
            else if (processedImgBtnBool != null && (bool)processedImgBtnBool)
            {
                ImgToDisplay = ImageHandler.ProcessedImgWithContourOverlay;
            }
            */


        }

        private void OpenFolderClick(object sender, RoutedEventArgs e)
        {
            OpenFolder();
        }

        private void ImgBoxClick(object sender, EventArgs e)
        {

            var me = (MouseEventArgs) e;
            var coordinates = me.Location;
            this.LastClickedPoint = coordinates;
            //ImageHandler.Process();
            //ImageHandler.DrawCellContour(ImgToDisplay, coordinates);
            //EmguImgBox.Image = ImgToDisplay;
            //ImageHandler.ProcessOverlays(coordinates);
            UpdateImgBox();
        }

        private void SwitchImg()
        {
            SwitchRadio();
            EmguImgBox.Image = Switch ? ImageHandler.ProcessedImage : ImageHandler.Image;
        }

        private void SwitchRadio()
        {
            Switch = !Switch;
            ProcessedImgBtn.IsChecked = Switch;
            OgImgBtn.IsChecked = !Switch;
        }

        private void OpenFolder()
        {
            var folderDialog = new CommonOpenFileDialog {IsFolderPicker = true};
            var folderResult = folderDialog.ShowDialog();
            if (folderResult != CommonFileDialogResult.Ok) return;
            textBoxFolderPath.Text = folderDialog.FileName;
            ImageHandler.UpdateFolder(textBoxFolderPath.Text);
            ShownImageChanged();
            UpdateImgBox();

        }

        private void ShowSettingsWindow(object sender, RoutedEventArgs e)
        {
            SettingsWindow.Show();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //ProcessCurrImg();
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
            var currImgNumber = ImageHandler.OpenedImgNumber;
            ImageHandler.Process();
            LengthCollector.Add(currImgNumber, this.LastClickedPoint, ImageHandler.GetCellLength(LastClickedPoint));
            //TODO lehet a lengthcollvetor néha takarítani is kellene :|
            //TempLengthList = LengthCollector.GetLengthList(currImgNumber, SettingsWindow.PPM_Sl.Value);
            TempLengthList = LengthCollector.GetLengthTripletList(currImgNumber, SettingsWindow.PPM_Sl.Value);
            LengthWindow.LengthGrid.ItemsSource = TempLengthList;
            LengthWindow.LengthGrid.Items.Refresh();
        }

        public void ShownImageChanged()
        {
            NumOfPics.Text = ImageHandler.Files.Count.ToString();
            CurrIdx.Text = ImageHandler.OpenedImgNumber.ToString();
            CurrImageDetectedCellCountBox.Text = ImageHandler.DetectedCellCount.ToString();
        }

        private void Window_OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {

                case Key.Return:
                    //DrawContours();
                    break;
                case Key.Space:
                    //SwitchImg();
                    //this.LengthCollector.Add();
                    break;
                case Key.Prior:
                    ImageHandler.NextImage();
                    ShownImageChanged();
                    UpdateImgBox();
                    //EmguImgBox.Image = ImageHandler.Image;
                    //ImageHandler.Process();
                    //EmguImgBox.Image = ImageHandler.ProcessedImage;
                    break;
                case Key.Next:
                    ImageHandler.PreviousImage();
                    ShownImageChanged();
                    UpdateImgBox();
                    //EmguImgBox.Image = ImageHandler.Image;
                    //ImageHandler.Process();
                    //EmguImgBox.Image = ImageHandler.ProcessedImage;
                    break;
                case Key.C:
                    //ImageHandler.UpdateImage(ImageHandler.ModTypeEnum.Clahe.ToString());
                    //EmguImgBox.Image = ImageHandler.Image;
                    break;
                case Key.E:
                    //ImageHandler.UpdateImage(ImageHandler.ModTypeEnum.EqualizeHist.ToString());
                    //EmguImgBox.Image = ImageHandler.Image;
                    break;
                case Key.O:
                    OpenFolder();
                    break;
                case Key.P:
                    //ProcessCurrImg();
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
                    ImageProcessorExaminer.Clear();
                    UpdateImgBox();
                    //EmguImgBox.Image = ImageHandler.Image;
                    //ImageHandler.Process();
                    //EmguImgBox.Image = ImageHandler.ProcessedImage;
                    break;
                case Keys.Next:
                    ImageHandler.NextImage();
                    CurrIdx.Text = ImageHandler.OpenedImgNumber.ToString();
                    ImageProcessorExaminer.Clear();
                    UpdateImgBox();
                    //EmguImgBox.Image = ImageHandler.Image;
                    //ImageHandler.Process();
                    //EmguImgBox.Image = ImageHandler.ProcessedImage;
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
                    return;
            }
        }



        private void CurrentIndexTextChanged(object sender, TextChangedEventArgs e)
        {
            var tempText = CurrIdx.Text;
            if (!IsTextAllowed(tempText)) return;
            var idxToJumpTo = int.Parse(tempText);
            ImageHandler.JumpToImage(idxToJumpTo);
            ShownImageChanged();
            UpdateImgBox();
        }


        private static bool IsTextAllowed(string text)
        {
            return !Regex.IsMatch(text);
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
            ConvertCoordinates(this.EmguImgBox,out var x0,out var y0,e.X,e.Y);
            this.LastClickedPoint = new System.Drawing.Point(x0,y0);
            this.SelectedCellLength = this.ImageHandler.GetCellLength(LastClickedPoint);
            if (this.SelectedCellLength < 0) return;
            CurrCellLengthBox.Text = SelectedCellLength.ToString(CultureInfo.InvariantCulture);
            CurrCellLengthCoordinates.Text = ImageHandler.ContourCenter(LastClickedPoint).ToString();
            //ImageHandler.DrawSelectedCellContourBoxToImageToDisplay(ImageHandler.ContourCenter(LastClickedPoint));
            //UpdateImgBox();
            //this.ImageHandler.Process();
            //this.ImageHandler.DrawCellContour(this.LastClickedPoint);
            //UpdateImgBox();
            //itt még hiányzik valami
        }

        public void ConvertCoordinates(ImageBox picBox, out int x0, out int y0, int x, int y)
        {
            var picHgt = picBox.ClientSize.Height;
            var picWid = picBox.ClientSize.Width;
            var size = picBox.Image.GetInputArray().GetSize();
            var imgHgt = size.Height;
            var imgWid = size.Width;

            x0 = x;
            y0 = y;
            switch (picBox.SizeMode)
            {
                case PictureBoxSizeMode.AutoSize:
                case PictureBoxSizeMode.Normal:
                    // These are okay. Leave them alone.
                    break;
                case PictureBoxSizeMode.CenterImage:
                    x0 = x - (picWid - imgWid) / 2;
                    y0 = y - (picHgt - imgHgt) / 2;
                    break;
                case PictureBoxSizeMode.StretchImage:
                    x0 = (int)(imgWid * x / (float)picWid);
                    y0 = (int)(imgHgt * y / (float)picHgt);
                    break;
                case PictureBoxSizeMode.Zoom:
                    var picAspect = picWid / (float)picHgt;
                    var imgAspect = imgWid / (float)imgHgt;
                    if (picAspect > imgAspect)
                    {
                        // The PictureBox is wider/shorter than the image.
                        y0 = (int)(imgHgt * y / (float)picHgt);

                        // The image fills the height of the PictureBox.
                        // Get its width.
                        float scaledWidth = imgWid * picHgt / imgHgt;
                        var dx = (picWid - scaledWidth) / 2;
                        x0 = (int)((x - dx) * imgHgt / (float)picHgt);
                    }
                    else
                    {
                        // The PictureBox is taller/thinner than the image.
                        x0 = (int)(imgWid * x / (float)picWid);

                        // The image fills the height of the PictureBox.
                        // Get its height.
                        float scaledHeight = imgHgt * picWid / imgWid;
                        var dy = (picHgt - scaledHeight) / 2;
                        y0 = (int)((y - dy) * imgWid / picWid);
                    }
                    break;
                default:
                    break;
            }
        }

        private void OgImgBtn_OnChecked(object sender, RoutedEventArgs e)
        {
            UpdateImgBox();
        }

        private void ProcessedImgBtn_Checked(object sender, RoutedEventArgs e)
        {
            UpdateImgBox();
        }

        private void OverlayCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateImgBox();
        }

        private void ExaminerClick(object sender, RoutedEventArgs e)
        {
            ImageProcessorExaminer.Show();
        }


        private void TextBoxFolderPath_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenFolder();
        }

        private void AddLengthBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
