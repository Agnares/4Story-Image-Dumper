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

        public Bitmap MergeImages(List<Bitmap> bmpImgs, LoadIMG.img.IMAGESET imgset)
        {
            var bmp = new Bitmap(imgset.m_vImage[0].m_nWidth, imgset.m_vImage[0].m_nHeight);
            for (int i = 0; i < bmpImgs.Count; i++)
                Graphics.FromImage(bmp).DrawImage(bmpImgs[i], imgset.m_vImage[0].m_pVERTEXDATA[i][0].m_fPosX + (bmpImgs.Count > 1 ? 0.5f : 0.0f), imgset.m_vImage[0].m_pVERTEXDATA[i][0].m_fPosY + (bmpImgs.Count > 1 ? 0.5f : 0.0f));
            return bmp;
        }

        public Bitmap GetFinishedImage(LoadIMG.img.IMAGESET imgset)
        {
            List<DDSImage> ddsImgs = new List<DDSImage>();
            List<Bitmap> bmpImgs = new List<Bitmap>();

            for (int i = 0; i < imgset.m_vImage[0].m_nPartCount; i++)
            {
                ddsImgs.Add(new DDSImage(Default.baseclass.Decompress(imgset.m_vImage[0].m_pT3DTEX[i].pDATA)));
            }

            for (int i = 0; i < ddsImgs.Count; i++)
            {
                int width = (int)(imgset.m_vImage[0].m_pVERTEXDATA[i][3].m_fPosX - imgset.m_vImage[0].m_pVERTEXDATA[i][0].m_fPosX);
                int height = (int)(imgset.m_vImage[0].m_pVERTEXDATA[i][3].m_fPosY - imgset.m_vImage[0].m_pVERTEXDATA[i][0].m_fPosY);
                int fullwidth = ddsImgs[i].BitmapImage.Width;
                int fullheight = ddsImgs[i].BitmapImage.Height;
                int x = (int)(imgset.m_vImage[0].m_pVERTEXDATA[i][0].m_fU * fullwidth);
                int y = (int)(imgset.m_vImage[0].m_pVERTEXDATA[i][0].m_fV * fullheight);

                bmpImgs.Add(CropImage(ddsImgs[i].BitmapImage, x, y, width, height));
            }

            Bitmap ImageMerged = MergeImages(bmpImgs, imgset);

            ddsImgs.Clear();
            bmpImgs.Clear();

            return ImageMerged;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadIMG.img.IMAGESET imgset = img.m_mapIMG.ElementAt(listBox1.SelectedIndex).Value;
            pictureBox1.Image = GetFinishedImage(imgset);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int counter = 0;
            foreach (LoadIMG.img.IMAGESET imgset in img.m_mapIMG.Values)
            {
                string subPath = ".\\dumped";
                bool exists = System.IO.Directory.Exists(subPath);
                if (!exists)
                    System.IO.Directory.CreateDirectory(subPath);

                GetFinishedImage(imgset).Save(subPath + "\\img_" + counter + ".png", ImageFormat.Png);

                counter++;
            }
        }
    }
}
