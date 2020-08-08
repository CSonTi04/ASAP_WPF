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
using System.Windows.Shapes;
using Emgu.CV;

namespace ASAP_WPF
{
    /// <summary>
    /// Interaction logic for PopupImage.xaml
    /// </summary>
    public partial class PopupImage : Window
    {
        public PopupImage(IInputArray img, string imgName)
        {
            InitializeComponent();
            PopupImgBox.Image = img;
            this.Title = imgName;
        }
    }
}
