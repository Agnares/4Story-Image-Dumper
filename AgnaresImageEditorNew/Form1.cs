using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;
using ddsparser;

namespace AgnaresImageEditorNew
{
    public partial class Form1 : Form
    {
        public LoadIMGBUF.imgbuf imgbuf = new LoadIMGBUF.imgbuf();
        public LoadIMG.img img = new LoadIMG.img();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            imgbuf.LoadIMGBUF();
            img.LoadIMG(imgbuf);
            img.ComplateIMG();

            listBox1.Items.Clear();
            foreach (KeyValuePair<uint, LoadIMG.img.IMAGESET> item in img.m_mapIMG)
            {
                int nSubImages = item.Value.m_vImage.Count;
                if (nSubImages > 1)
                    listBox1.Items.Add(item.Key + " subimages: " + item.Value.m_vImage.Count);
                else
                    listBox1.Items.Add(item.Key);
            }
        }

        public Bitmap CropImage(Bitmap source, int x, int y, int width, int height)
        {
            Rectangle crop = new Rectangle(x, y, width, height);
            var bmp = new Bitmap(crop.Width, crop.Height);
            Graphics.FromImage(bmp).DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            return bmp;
        }

        public List<Bitmap> MergeBitmapPartsOfAllImages(List<List<Bitmap>> imgs, LoadIMG.img.IMAGESET imgset)
        {
            List<Bitmap> bmps = new List<Bitmap>();
            for (int i = 0; i < imgs.Count; i++)
            {
                Bitmap bmp = new Bitmap(imgset.m_vImage[i].m_nWidth, imgset.m_vImage[i].m_nHeight);
                int nBitmapCount = imgs[i].Count;
                for (int j = 0; j < nBitmapCount; j++)
                {
                    Graphics.FromImage(bmp).DrawImage(imgs[i][j],
                        imgset.m_vImage[i].m_pVERTEXDATA[j][0].m_fPosX + (nBitmapCount > 1 ? 0.5f : 0.0f),
                        imgset.m_vImage[i].m_pVERTEXDATA[j][0].m_fPosY + (nBitmapCount > 1 ? 0.5f : 0.0f));
                }
                bmps.Add(bmp);
            }
            return bmps;
        }

        public List<Bitmap> GetFinishedImages(LoadIMG.img.IMAGESET imgset)
        {
            List<List<Bitmap>> imgs = new List<List<Bitmap>>();
            for (int i = 0; i < imgset.m_vImage.Count; i++)
            {
                imgs.Add(new List<Bitmap>());
                for (int j = 0; j < imgset.m_vImage[i].m_nPartCount; j++)
                {
                    DDSImage dds = new DDSImage(Default.baseclass.Decompress(imgset.m_vImage[i].m_pT3DTEX[j].pDATA));

                    int width = (int)(imgset.m_vImage[i].m_pVERTEXDATA[j][3].m_fPosX - imgset.m_vImage[i].m_pVERTEXDATA[j][0].m_fPosX);
                    int height = (int)(imgset.m_vImage[i].m_pVERTEXDATA[j][3].m_fPosY - imgset.m_vImage[i].m_pVERTEXDATA[j][0].m_fPosY);
                    int x = (int)(imgset.m_vImage[i].m_pVERTEXDATA[j][0].m_fU * dds.BitmapImage.Width);
                    int y = (int)(imgset.m_vImage[i].m_pVERTEXDATA[j][0].m_fV * dds.BitmapImage.Height);

                    imgs[i].Add(CropImage(dds.BitmapImage, x, y, width, height));
                }
            }

            return MergeBitmapPartsOfAllImages(imgs, imgset);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            LoadIMG.img.IMAGESET imgset = img.m_mapIMG.ElementAt(index).Value;
            List<Bitmap> bmpFinished = GetFinishedImages(imgset);

            for (int i = 0; i < bmpFinished.Count; i++)
            {
                pictureBox1.Image = bmpFinished[i];
                Application.DoEvents();
                if (bmpFinished.Count > 1)
                {
                    if (index != listBox1.SelectedIndex)
                        break;
                    Thread.Sleep(100);
                }
                if (i == bmpFinished.Count - 1)
                    pictureBox1.Image = bmpFinished[0];
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < img.m_mapIMG.Count; i++)
            {
                string subPath = ".\\dumped";
                if (!Directory.Exists(subPath))
                    Directory.CreateDirectory(subPath);

                List<Bitmap> bmpFinished = GetFinishedImages(img.m_mapIMG.ElementAt(i).Value);
                for (int j = 0; j < bmpFinished.Count; j++)
                {
                    bmpFinished[j].Save(subPath + "\\img_" + img.m_mapIMG.ElementAt(i).Key + "_" + j + ".png", ImageFormat.Png);
                }
            }

            MessageBox.Show("Img dumping finished!");
        }
    }
}
