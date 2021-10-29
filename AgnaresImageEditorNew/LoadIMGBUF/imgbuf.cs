using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace AgnaresImageEditorNew.LoadIMGBUF
{
    public unsafe class imgbuf
    {
        public class IMGBUFSTRUCT
        {
            public IMGBUFSTRUCT
            (
                uint dwID, 
                byte bFormat, 
                uint dwSIZE, 
                uint dwDATA, 
                byte[] pDATA
            )
            {
                this.dwID = dwID;
                this.bFormat = bFormat;
                this.dwSIZE = dwSIZE;
                this.dwDATA = dwDATA;
                this.pDATA = pDATA;
            }
            public uint dwID { get; set; }
            public byte bFormat { get; set; }
            public uint dwSIZE { get; set; }
            public uint dwDATA { get; set; }
            public byte[] pDATA { get; set; }
        }

        public List<IMGBUFSTRUCT> m_mapIMGBUF = new List<IMGBUFSTRUCT>();

        public void LoadIMGBUF()
        {
            FileStream fs = File.OpenRead(Default.baseclass.basepath + ".\\Index\\TClientList.LST");
            BinaryReader reader = new BinaryReader(fs);

            m_mapIMGBUF.Clear();

            int nCount = reader.ReadInt32();
            int nTotal = reader.ReadInt32();
            int nIndex = 0;

            for (int i = 0; i < nCount; i++)
                LoadIMGBUF(Default.baseclass.ReadNumBytesString(reader), ref nIndex, nTotal);

            reader.Close();
            fs.Close();
        }

        public void LoadIMGBUF(string strFILE, ref int nIndex, int nTotal)
        {
            FileStream fs = File.OpenRead(Default.baseclass.basepath + ".\\Data\\" + strFILE);
            BinaryReader reader = new BinaryReader(fs);

            uint dwLENGTH = (uint)reader.BaseStream.Length;
            uint dwPOS = (uint)reader.BaseStream.Position;
           
            while (dwPOS < dwLENGTH)
            {
                BinaryReader decompressedReader = Default.baseclass.ucprStream(reader);

                uint dwID = decompressedReader.ReadUInt32();
                byte bFormat = decompressedReader.ReadByte();
                uint dwSIZE = decompressedReader.ReadUInt32();
                uint dwDATA = decompressedReader.ReadUInt32();

                byte[] data = new byte[dwDATA];
                for (uint i = 0; i < dwDATA; i++)
                    data[i] = decompressedReader.ReadByte();

                decompressedReader.BaseStream.Seek(dwDATA, SeekOrigin.Current);

                m_mapIMGBUF.Add(new IMGBUFSTRUCT(dwID, bFormat, dwSIZE, dwDATA, data));

                dwPOS = (uint)reader.BaseStream.Position;
                nIndex++;

                Debug.WriteLine("Loading IMGBUF: " + nIndex * 100 / nTotal + "%");
                decompressedReader.Close();
            }

            reader.Close();
            fs.Close();
        }
    }
}
