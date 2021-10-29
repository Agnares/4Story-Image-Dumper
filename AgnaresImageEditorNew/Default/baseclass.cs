using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Drawing;

namespace AgnaresImageEditorNew.Default
{
    public static class baseclass
    {
        public static byte[] dec_data;
        public static string basepath = "E:\\Admin\\GameFiles\\4Nova";

        public static string ReadNumBytesString(BinaryReader reader)
        {
            string strToReturn = "";
            int nSize = reader.ReadInt32();
            for (int i = 0; i < nSize; i++)
                strToReturn += Convert.ToChar(reader.ReadByte());
            return strToReturn;
        }

        public static byte[] Decompress(byte[] data)
        {
            MemoryStream inputStream = new MemoryStream(data);
            MemoryStream resultStream = new MemoryStream();
            GZipStream dcprStream = new GZipStream(inputStream, CompressionMode.Decompress);

            dcprStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }

        public static BinaryReader ucprStream(BinaryReader reader)
        {
            uint dec_dwORIGIN = reader.ReadUInt32();
            uint dec_dwLENGTH = reader.ReadUInt32();

            dec_data = Decompress(reader.ReadBytes(Convert.ToInt32(dec_dwLENGTH)));

            return new BinaryReader(new MemoryStream(dec_data));
        }
    }
}
