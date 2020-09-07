using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static LengthCollector LengthCollector { get; set; }
        private bool Switch { get; set; }
        public static SettingsWindow SettingsWindow { get; set; }

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
            ProcessedWithOverlay,
            PictureModifiedByClick
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

        public void UpdateImgBox(bool originIsMouseClick)
        {
            if (null == ImageHandler.FolderName) return;
            //if(_originIsMousClick) this.ImageHandler.DrawCellContour(this.LastClickedPoint);





            var overlayCbBool = this.OverlayCheckBox.IsChecked;
            var processedImgBtnBool = this.ProcessedImgBtn.IsChecked;
            var ogImgBtnBool = this.OgImgBtn.IsChecked;

            if(!originIsMouseClick)
            {
                if (overlayCbBool != null && (bool)overlayCbBool)
                {
                    if (ogImgBtnBool != null && (bool)ogImgBtnBool)
                    {
                        //ImgToDisplay = ImageHandler.OgImgWithContourOverlay;
                        CurrentImageToDisplay = ImageToDisplay.OriginalWithOverlay;
                    }
                    else if (processedImgBtnBool != null && (bool)processedImgBtnBool)
                    {
                        //ImgToDisplay = ImageHandler.ProcessedImgWithContourOverlay;
                        CurrentImageToDisplay = ImageToDisplay.ProcessedWithOverlay;
                    }
                }
                else
                {

                    if (ogImgBtnBool != null && (bool)ogImgBtnBool)
                    {
                        //ImgToDisplay = ImageHandler.Image;
                        CurrentImageToDisplay = ImageToDisplay.Original;
                    }
                    else if (processedImgBtnBool != null && (bool)processedImgBtnBool)
                    {
                        //ImgToDisplay = ImageHandler.ProcessedImage;
                        CurrentImageToDisplay = ImageToDisplay.Processed;
                    }
                }
            }
            else
            {
                ImageHandler.DrawSelectedCellContourBoxToImageToDisplay(ImageHandler.ContourCenter(LastClickedPoint));
                CurrentImageToDisplay = ImageToDisplay.PictureModifiedByClick;
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
            UpdateImgBox(true);
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
            UpdateImgBox(false);

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
            LengthCollector.Add(currImgNumber, this.LastClickedPoint, ImageHandler.GetCellLengthWithBoundingBox(LastClickedPoint));
            //TODO lehet a lengthcollvetor néha takarítani is kellene :|
            //TempLengthList = LengthCollector.GetLengthList(currImgNumber, SettingsWindow.PPM_Sl.Value);
            TempLengthList = LengthCollector.GetLengthTripletList(SettingsWindow.PPM_Sl.Value);
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
                    UpdateImgBox(false);
                    //EmguImgBox.Image = ImageHandler.Image;
                    //ImageHandler.Process();
                    //EmguImgBox.Image = ImageHandler.ProcessedImage;
                    break;
                case Key.Next:
                    ImageHandler.PreviousImage();
                    ShownImageChanged();
                    UpdateImgBox(false);
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





        private void CurrentIndexTextChanged(object sender, TextChangedEventArgs e)
        {
            var tempText = CurrIdx.Text;
            if (!IsTextAllowed(tempText)) return;
            var idxToJumpTo = int.Parse(tempText);
            ImageHandler.JumpToImage(idxToJumpTo);
            ShownImageChanged();
            UpdateImgBox(false);
        }


        private static bool IsTextAllowed(string text)
        {
            return !Regex.IsMatch(text);
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
            Debug.WriteLine("Original: " + e.X + "," + e.Y + " Converted: " + x0 + "," + y0 + ", ZoomScale: " + EmguImgBox.ZoomScale);
            this.LastClickedPoint = new System.Drawing.Point((int)x0,(int)y0);
            this.SelectedCellLength = this.ImageHandler.GetCellLengthWithBoundingBox(LastClickedPoint);
            if (this.SelectedCellLength < 0) return;
            CurrCellLengthBox.Text = SelectedCellLength.ToString(CultureInfo.InvariantCulture);
            //CurrCellLengthCoordinates.Text = ImageHandler.ContourCenter(LastClickedPoint).ToString();
            CurrCellLengthCoordinates.Text = LastClickedPoint.ToString();

            ImageHandler.PrintAllTypeOfCellLengthToDebug(LastClickedPoint);
            //ImageHandler.DrawSelectedCellContourBoxToImageToDisplay(ImageHandler.ContourCenter(LastClickedPoint));

            var contour = this.ImageHandler.GetContour(this.LastClickedPoint);
            //ImageHandler.ImgProcessor.CalculateCellLength(contour);

            UpdateImgBox(true);
            //this.ImageHandler.Process();
            //this.ImageHandler.DrawCellContour(this.LastClickedPoint);
            //UpdateImgBox();
            //itt még hiányzik valami
        }

        public void ConvertCoordinates(ImageBox picBox, out double x0, out double y0, int x, int y)
        {
            Debug.WriteLine("Coordinates_");
            Debug.WriteLine("X: " + x);
            Debug.WriteLine("Y: " + y);
            Debug.WriteLine("");
            var picHgt = picBox.ClientSize.Height;
            var picWid = picBox.ClientSize.Width;
            var size = picBox.Image.GetInputArray().GetSize();
            //var temp = picBox.HorizontalScrollBar.va
            var imgHgt = size.Height;
            var imgWid = size.Width;

            var floatX = (double) x + picBox.HorizontalScrollBar.Value;
            var floatY = (double) y + picBox.VerticalScrollBar.Value;
            Debug.WriteLine("X offset: " + picBox.HorizontalScrollBar.Value);
            Debug.WriteLine("Y offset: " + picBox.VerticalScrollBar.Value);
            //var scaledX = floatX /= picBox.ZoomScale;
            //var scaledY = floatY /= picBox.ZoomScale;

            //var scaledX = floatX *= picBox.ZoomScale;
            //var scaledY = floatY *= picBox.ZoomScale;



            //x0 = x;
            //y0 = y;

            x0 = floatX;
            y0 = floatX;
            switch (picBox.SizeMode)
            {
                case PictureBoxSizeMode.AutoSize:
                case PictureBoxSizeMode.Normal:
                    // These are okay. Leave them alone.
                    break;
                case PictureBoxSizeMode.CenterImage:
                    Debug.WriteLine("CenterImage");
                    x0 = x - (picWid - imgWid) / 2;
                    y0 = y - (picHgt - imgHgt) / 2;
                    //x0 = x0 *= picBox.ZoomScale;
                    //y0 = y0 *= picBox.ZoomScale;
                    Debug.WriteLine("X: " + x0);
                    Debug.WriteLine("Y: " + y0);
                    Debug.WriteLine("");
                    break;
                case PictureBoxSizeMode.StretchImage:
                    Debug.WriteLine("StretchImage");
                    x0 = (int)(imgWid * x / (float)picWid);
                    y0 = (int)(imgHgt * y / (float)picHgt);
                    //x0 = x0 *= picBox.ZoomScale;
                    //y0 = y0 *= picBox.ZoomScale;
                    Debug.WriteLine("X: " + x0);
                    Debug.WriteLine("Y: " + y0);
                    Debug.WriteLine("");
                    break;
                case PictureBoxSizeMode.Zoom:
                    var picAspect = picWid / (float)picHgt;
                    var imgAspect = imgWid / (float)imgHgt;
                    if (picAspect > imgAspect)
                    {
                        Debug.WriteLine("Zoom, picAspect > imgAspect");
                        // The PictureBox is wider/shorter than the image.
                        y0 = (int)(imgHgt * y / (float)picHgt);

                        // The image fills the height of the PictureBox.
                        // Get its width.
                        var scaledWidth = imgWid * picHgt / (float)imgHgt;
                        var dx = (picWid - scaledWidth) / 2;
                        //
                        Debug.WriteLine("Dx: " + dx);
                        x0 = (int)((x - dx) * imgHgt / (float)picHgt);

                        //x0 = x0 *= picBox.ZoomScale;
                        //y0 = y0 *= picBox.ZoomScale;
                        Debug.WriteLine("X: " + x0);
                        Debug.WriteLine("Y: " + y0);
                        Debug.WriteLine("");
                    }
                    else
                    {
                        Debug.WriteLine("Zoom, picAspect < imgAspect");
                        // The PictureBox is taller/thinner than the image.
                        x0 = (int)(imgWid * x / (float)picWid);

                        // The image fills the height of the PictureBox.
                        // Get its height.
                        var scaledHeight = imgHgt * picWid / (float)imgWid;
                        var dy = (picHgt - scaledHeight) / 2;
                        Debug.WriteLine("Dy: " + dy);
                        y0 = (int)((y - dy) * imgWid / picWid);

                        //x0 = x0 *= picBox.ZoomScale;
                        //y0 = y0 *= picBox.ZoomScale;
                        Debug.WriteLine("X: " + x0);
                        Debug.WriteLine("Y: " + y0);
                        Debug.WriteLine("");
                    }
                    break;
                default:
                    break;
            }
        }

        private void OgImgBtn_OnChecked(object sender, RoutedEventArgs e)
        {
            UpdateImgBox(false);
        }

        private void ProcessedImgBtn_Checked(object sender, RoutedEventArgs e)
        {
            UpdateImgBox(false);
        }

        private void OverlayCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateImgBox(false);
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
            var tempLength = ImageHandler.GetCellLengthWithBoundingBox(this.LastClickedPoint);
            LengthCollector.Add(ImageHandler.OpenedImgNumber,this.LastClickedPoint,tempLength);
        }
    }
}
