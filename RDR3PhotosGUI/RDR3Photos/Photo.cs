using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace RDR3PhotosGUI.RDR3Photos
{
    public class Photo
    {
        //First 0x120 bytes
        public PhotoInfo Info;

        public PhotoData PhotoData;
        public JsonData JsonData;
        public ExtraData ExtraData;

        public Photo(byte[] data)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                Info = new PhotoInfo(reader);
                PhotoData = new PhotoData(reader);
                JsonData = new JsonData(reader);
                ExtraData = new ExtraData(reader);
            }
        }
    }

    public class PhotoInfo
    {
        public int Unknown { get; set; }
        public byte[] Data { get; set; }
        public byte[] UnknownData { get; set; }

        public PhotoInfo(BinaryReader reader)
        {
            Unknown = reader.ReadInt32();
            Data = reader.ReadBytes(0x100);
            UnknownData = reader.ReadBytes(0x1C);
        }

        public string GetString()
        {
            return Encoding.Unicode.GetString(Data).Replace("\0", string.Empty);
        }

        public DateTime GetDate()
        {
            return DateTime.ParseExact(this.GetString().Split('-')[1].Trim(), "MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }

    public class PhotoData
    {
        private int JPEG = 0x4745504A; // JPEG tag

        public int Tag { get; set; }
        public int Lenght { get; set; }
        public int Unknown { get; set; }
        public byte[] Image { get; set; }

        public PhotoData(BinaryReader reader)
        {
            Tag = reader.ReadInt32();

            if (Tag != JPEG)
            {
                Console.WriteLine($"Invalid photo data");
                return;
            }

            Lenght = reader.ReadInt32();
            Unknown = reader.ReadInt32();
            Image = reader.ReadBytes(this.Lenght);
        }

        public void SaveToDisk(string path)
        {
            File.WriteAllBytes(path, Image);
        }
    }

    public class JsonData
    {
        private int JSON = 0x4E4F534A; // JSON tag

        public int Tag { get; set; }
        public int Lenght { get; set; }
        public byte[] Data { get; set; }

        public JsonData(BinaryReader reader)
        {
            Tag = reader.ReadInt32();

            if (Tag != JSON)
            {
                Console.WriteLine($"Invalid JSON data");
                return;
            }

            Lenght = reader.ReadInt32();
            Data = reader.ReadBytes(this.Lenght);
        }

        public void SaveToDisk(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(JsonConvert.DeserializeObject(Data.GetNullTerminatedString()), Formatting.Indented));
        }
    }

    public class ExtraData
    {
        private int TITL = 0x4C544954; // TITL

        public int TitleTag { get; set; }
        public int TitleLenght { get; set; }
        public byte[] TitleData { get; set; }


        private int DESC = 0x43534544; // DESC

        public int DescTag { get; set; }
        public int DescLenght { get; set; }
        public byte[] DescData { get; set; }

        public ExtraData(BinaryReader reader)
        {
            TitleTag = reader.ReadInt32();
            if (TitleTag != TITL)
            {
                Console.WriteLine($"Invalid TITL data");
                return;
            }

            TitleLenght = reader.ReadInt32();
            TitleData = reader.ReadBytes(this.TitleLenght);


            DescTag = reader.ReadInt32();
            if (DescTag != DESC)
            {
                Console.WriteLine($"Invalid DESC data");
                return;
            }

            DescLenght = reader.ReadInt32();
            DescData = reader.ReadBytes(this.TitleLenght);
        }

    }
}
