using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using MessageBox = System.Windows.MessageBox;

namespace ASAP_WPF
{
    /// <summary>
    /// Interaction logic for ImageProcessorExaminer.xaml
    /// </summary>
    public partial class ImageProcessorExaminer
    {
        //public static Dictionary<string, Image<Bgra, byte>> ImgList;
        public static Dictionary<string, Mat> ImgList;

        public ImageProcessorExaminer()
        {
            InitializeComponent();
            ImageProcessorImgBox.SizeMode = PictureBoxSizeMode.Zoom;
            ImgList = new Dictionary<string, Mat>();
        }

        public void Clear()
        {
            foreach (var node in from entry in ImgList where !ImgTreeView.Nodes.ContainsKey(entry.Key) select new TreeNode(entry.Key) {Name = entry.Key})
            {
                ImgTreeView.Nodes.Remove(node);
            }
            ImgList.Clear();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public void AddImage(Mat img, string keyname)
        {
            if (!ImgTreeView.Nodes.ContainsKey(keyname))
            {
                var node = new TreeNode(keyname) {Name = keyname};
                ImgTreeView.Nodes.Add(node);
                ImgTreeView.SelectedNode = node;
            }

            if (!ImgList.ContainsKey(keyname))
            {
                ImgList.Add(keyname, img);
            }
            else
            {
                ImgList[keyname] = img;
            }
        }

        public void RefreshImgTreeView()
        {
            foreach (var node in from entry in ImgList where !ImgTreeView.Nodes.ContainsKey(entry.Key) select new TreeNode(entry.Key) {Name = entry.Key})
            {
                ImgTreeView.Nodes.Add(node);
                ImgTreeView.SelectedNode = node;
            }
        }

        private void ImgTreeView_OnNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                ImageProcessorImgBox.Image = ImgList[e.Node.Text];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
