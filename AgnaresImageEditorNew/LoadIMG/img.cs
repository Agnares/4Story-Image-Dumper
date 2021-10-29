using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace AgnaresImageEditorNew.LoadIMG
{
    public unsafe class img
    {
        public struct TVERTEX
        {
            public float m_fPosX;
            public float m_fPosY;
            public float m_fPosZ;
            public float m_fRHW;

            public float m_fU;
            public float m_fV;
        }

        public struct T3DDATA
        {
            public byte[] m_pData;
            public uint m_dwSize;
        }

        public class IMAGE
        {
            public List<uint> dwImageID = new List<uint>();
            public List<LoadIMGBUF.imgbuf.IMGBUFSTRUCT> m_pT3DTEX = new List<LoadIMGBUF.imgbuf.IMGBUFSTRUCT>();
            public List<List<TVERTEX>> m_pVERTEXDATA = new List<List<TVERTEX>>();
            public int m_nPartCount;
            public int m_nWidth;
            public int m_nHeight;
            public int nMask;
        }

        public class KEY
        {
            public uint m_dwTick;
            public uint m_dwColor;
        }

        public class IMAGESET
        {
            public List<IMAGE> m_vImage = new List<IMAGE>();
            public List<KEY> m_vKey = new List<KEY>();

            public uint m_dwTotalTick;
            public uint m_dwCurTick;
            public byte bFormat;
        }

        private Dictionary<uint, IMAGE> m_mapIMGSRC = new Dictionary<uint, IMAGE>();
        public Dictionary<uint, IMAGESET> m_mapIMG = new Dictionary<uint, IMAGESET>();

        public void LoadIMG(LoadIMGBUF.imgbuf imgbuf)
        {
            FileStream fs = File.OpenRead(Default.baseclass.basepath + ".\\Index\\TClientI.IDX");
            BinaryReader reader = new BinaryReader(fs);

            m_mapIMGSRC.Clear();
            m_mapIMG.Clear();

            int nCount = reader.ReadInt32();
            int nTotal = reader.ReadInt32();
            int nIndex = 0;

            List<Dictionary<uint, IMAGE>> pTRESDATA = new List<Dictionary<uint, IMAGE>>();
            List<Dictionary<uint, IMAGESET>> pTRES = new List<Dictionary<uint, IMAGESET>>();

            for (int i = 0; i < nCount; i++)
            {
                pTRESDATA.Add(new Dictionary<uint, IMAGE>());
                pTRES.Add(new Dictionary<uint, IMAGESET>());
            }

            for (int i = 0; i < nCount; i++)
                LoadIMG(Default.baseclass.ReadNumBytesString(reader), ref pTRESDATA, ref pTRES, ref nIndex, nTotal, i, imgbuf);

            for (int i = 0; i < nTotal; i++)
            {
                uint dwID = reader.ReadUInt32();
                uint dwFileID = reader.ReadUInt32();
                uint dwPOS = reader.ReadUInt32();

                IMAGE resultData;
                if (pTRESDATA[Convert.ToInt32(dwFileID)].TryGetValue(dwPOS, out resultData))
                {
                    m_mapIMGSRC.Add(dwID, resultData);
                }

                IMAGESET result;
                if (pTRES[Convert.ToInt32(dwFileID)].TryGetValue(dwPOS, out result))
                {
                    m_mapIMG.Add(dwID, result);
                }
            }

            pTRESDATA.Clear();
            pTRES.Clear();

            reader.Close();
            fs.Close();
        }

        public void LoadIMG(string strFILE, ref List<Dictionary<uint, IMAGE>> pTRESDATA, ref List<Dictionary<uint, IMAGESET>> pTRES, ref int nIndex, int nTotal, int nDataIndex, LoadIMGBUF.imgbuf imgbuf)
        {
            FileStream fs = File.OpenRead(Default.baseclass.basepath + ".\\Data\\" + strFILE);
            BinaryReader reader = new BinaryReader(fs);

            uint dwLENGTH = (uint)reader.BaseStream.Length;
            uint dwPOS = (uint)reader.BaseStream.Position;

            while (dwPOS < dwLENGTH)
            {
                BinaryReader decompressedReader = Default.baseclass.ucprStream(reader);

                IMAGESET pIMG = new IMAGESET();
                pIMG.m_dwTotalTick = 1000;
                pIMG.m_dwCurTick = 0;

                int nCount = 0;

                nCount = decompressedReader.ReadInt32();
                for(int i = 0; i < nCount; i++)
                {
                    IMAGE pDATA = new IMAGE();
                    pDATA.dwImageID.Add(decompressedReader.ReadUInt32());

                    pIMG.m_vImage.Add(pDATA);
                }

                nCount = decompressedReader.ReadInt32();
                for(int i = 0; i < nCount; i++)
                {
                    KEY pKEY = new KEY();
                    pKEY.m_dwTick = decompressedReader.ReadUInt32();
                    pKEY.m_dwColor = decompressedReader.ReadUInt32();

                    pIMG.m_vKey.Add(pKEY);
                }

                pIMG.m_dwTotalTick = decompressedReader.ReadUInt32();
                pIMG.bFormat = decompressedReader.ReadByte();
                uint dwSize = decompressedReader.ReadUInt32();

                if(Convert.ToInt32(dwSize) > 0)
                {
                    IMAGE pDATA = new IMAGE();

                    pDATA.m_nPartCount = decompressedReader.ReadInt32();
                    pDATA.m_nWidth = decompressedReader.ReadInt32();
                    pDATA.m_nHeight = decompressedReader.ReadInt32();

                    for (int i = 0; i < pDATA.m_nPartCount; i++)
                    {
                        uint dwImageID = decompressedReader.ReadUInt32();
                        pDATA.dwImageID.Add(dwImageID);

                        foreach (LoadIMGBUF.imgbuf.IMGBUFSTRUCT item in imgbuf.m_mapIMGBUF)
                        {
                            if (item.dwID == dwImageID)
                            {
                                pDATA.m_pT3DTEX.Add(item);
                            }
                        }

                        pDATA.m_pVERTEXDATA.Add(new List<TVERTEX>());

                        for (int j = 0; j < 4; j++)
                        {
                            TVERTEX readVTX = new TVERTEX();
                            readVTX.m_fPosX = decompressedReader.ReadSingle();
                            readVTX.m_fPosY = decompressedReader.ReadSingle();
                            readVTX.m_fPosZ = decompressedReader.ReadSingle();
                            readVTX.m_fRHW = decompressedReader.ReadSingle();
                            readVTX.m_fU = decompressedReader.ReadSingle();
                            readVTX.m_fV = decompressedReader.ReadSingle();
                            pDATA.m_pVERTEXDATA[i].Add(readVTX);
                        }
                        
                        //decompressedReader.ReadBytes(4 * sizeof(TVERTEX));
                    }

                    pDATA.nMask = decompressedReader.ReadInt32();

                    pTRESDATA[nDataIndex].Add(dwPOS, pDATA);
                }

                pTRES[nDataIndex].Add(dwPOS, pIMG);

                dwPOS = (uint)reader.BaseStream.Position;
                nIndex++;

                Debug.WriteLine("Loading IMG: " + nIndex * 100 / nTotal + "%");
                decompressedReader.Close();
            }

            reader.Close();
            fs.Close();
        }

        public void ComplateIMG()
        {
            Debug.WriteLine("Complating IMG.");
            for (int i = 0; i < m_mapIMG.Count(); i++)
            {
                KeyValuePair<uint, IMAGESET> pIMG = m_mapIMG.ElementAt(i);
                if (pIMG.Value != null)
                {
                    int nCount = pIMG.Value.m_vImage.Count();
                    for(int j = 0; j < nCount; j++)
                    {
                        IMAGE imgsrc;
                        if (m_mapIMGSRC.TryGetValue(pIMG.Value.m_vImage[j].dwImageID[0], out imgsrc))
                        {
                            pIMG.Value.m_vImage[j] = imgsrc;
                        }
                    }
                }
            }
            Debug.WriteLine("Complating IMG Finished.");
        }
    }
}
