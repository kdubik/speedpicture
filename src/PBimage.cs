using System;
using System.IO;

namespace SpeedPicture
{
    public class PBImage
    {
        public int width { get; private set; }
        public int height { get; private set; }
        
        //public int bitsPerPixel { get; private set; }
        //public int pixelSize { get; private set; }       // Kolko bytov zabera 1 pixel 
        public bool transparent { get; private set; }
        public byte[] pixels;   // RGB pixels
        public byte[] alpha;   // Alpha channel pixels

        // Additional information
        int scanLineSize;   // How long is line in image? (width * 3) bytes

        public PBImage()
        {
        }
        
        public PBImage(string fileName)
        {
            if (fileName.ToLower().Contains(".pb"))
            {
                // KAM
                //LoadFromPB(fileName);
            }

            if (fileName.ToLower().Contains(".bmp"))
            {
                LoadFromBMP(fileName, true);
            }
        }
        
        public PBImage(bool transparent, int width, int height)
        {
            Create(transparent, width, height);
        }

        public void Create(bool transparent, int width, int height)
        {
            this.width = width;
            this.height = height;
            this.transparent = transparent;

            scanLineSize = width * 3;
            int pixelsCount = scanLineSize * height;
            this.pixels = new byte[pixelsCount];

            if (transparent) 
            {
                pixelsCount = width * height;
                alpha = new byte[pixelsCount];
            }
            else
            {
                transparent = false;
                alpha = null;
            }              
        }

        public int Clear(byte a, byte r, byte g, byte b)
        {
            int res = 0;

            res = Clear(r,g,b);

            // Alpha channel
            if (alpha.Length>0)
            {
                for (int p=0; p<alpha.Length; p++)
                    alpha[p] = a;
            }
            else res = 2;   // Error: Image has no Alpha channel

            return res;
        }

        public int Clear(byte r, byte g, byte b)
        {
            int res = 0;

            if (pixels.Length>0)
            {
                for (int p=0; p<pixels.Length; p+=3)
                {    
                    pixels[p] = r;
                    pixels[p+1] = g;
                    pixels[p+2] = b;
                }
            }
            else res = 1;   // Error: Image not initialized

            return res;
        }
   
        public int Clear(SColor color)
        {
            return Clear(color.a,color.r,color.g,color.b);
        }

        private byte[] ReversePixelsOrder(byte[] inPixels, int inPixelSize)
        {
            int size = inPixels.Length;
            byte[] tmpPixels = new byte[size];
            int scanLine = width * inPixelSize;
            
            for (int a=0; a<height; a++)
            {
                int pos = a * scanLine;
                int source = (height - a - 1) * scanLine;

                // Revert BGRA pixel to ARGB
                for (int p=0; p<scanLine; p+=inPixelSize)
                {
                    for(int s=0;s<inPixelSize;s++)
                    {
                        tmpPixels[pos + p +s] = inPixels[source + p + (inPixelSize - 1 - s)];
                    }
                }
            }
            return tmpPixels;
        }

        private byte[] ReverseAlphaOrder()
        {
            int size = alpha.Length;
            byte[] tmpAlpha = new byte[size];
            
            for (int a=0; a<height; a++)
            {
                int pos = a * width;
                int source = (height - a - 1) * width;

                // Revert BGRA pixel to ARGB
                for (int p=0; p<width; p++)
                {
                    tmpAlpha[pos + p] = alpha[source + width - p - 1];
                }
            }
            return tmpAlpha;
        }

        public void ReversePixels()
        {
            pixels = ReversePixelsOrder(pixels,3);
            //if (transparent) alpha = ReverseAlphaOrder();
            if (transparent) alpha = ReversePixelsOrder(alpha,1);
        }

        public void LoadFromBMP(string fileName, bool showInfo)
        {
            string headerField = "";
            string bmpDescription = "Unknown type";
            uint BMPsize;           
            uint startingAddress;
            uint sizeOfHeader;
            ushort bitsPerPixel;

            var fs = new FileStream(fileName, FileMode.Open);
            var len = (int)fs.Length;
            byte[] data = new byte[len];
            fs.Read(data, 0, len);

            // Analyze           
            headerField += Convert.ToChar(data[0]);
            headerField += Convert.ToChar(data[1]);
            if (headerField=="BM") bmpDescription = "Windows 3.1x, 95, NT, ... etc.";
            if (headerField=="BA") bmpDescription = "OS/2 struct bitmap array";
            if (headerField=="CI") bmpDescription = "OS/2 struct color icon";
            if (headerField=="CP") bmpDescription = "OS/2 const color pointer";
            if (headerField=="IC") bmpDescription = "OS/2 struct icon";
            if (headerField=="PT") bmpDescription = "OS/2 pointer";         

            BMPsize = (uint)Tools.CreateInt(data[5],data[4],data[3],data[2]);                     
            startingAddress = (uint)Tools.CreateInt(data[13],data[12],data[11],data[10]);
           
            // Bitmapcoreheader / V4
            sizeOfHeader = (uint)Tools.CreateInt(data[17],data[16],data[15],data[14]);
            width = (ushort)Tools.CreateInt(data[21],data[20],data[19],data[18]);
            height = (ushort)Tools.CreateInt(data[25],data[24],data[23],data[22]);
            ushort planesCount = Tools.CreateWord(data[27],data[26]);
            bitsPerPixel = Tools.CreateWord(data[29],data[28]);
            UInt32 compression = Tools.CreateUInt(data[33],data[32],data[31],data[30]);
            UInt32 imgSize = Tools.CreateUInt(data[37],data[36],data[35],data[34]);

            // if (bitsPerPixel == 24) pixelSize = 3;
            transparent = false;
            if (bitsPerPixel == 32) transparent = true;

            // Load pixels
            scanLineSize = width * 3;
            int pixelCount = width * height * 3;
            // pixelCount = (int)imgSize;

            pixels = new byte[pixelCount];
            uint sa = startingAddress;

            if (bitsPerPixel == 32)
            {
                int alphaCount = width * height;
                alpha = new byte[alphaCount];

                // 32 BIT version
                // IMG size je nutne, aby sme precitali vsetky pixely
                int targetPosition = 0;
                int aPosition = 0;
                for (int a = 0; a<imgSize; a+=4)
                {                       
                    pixels[targetPosition] = data[sa+a];     // RGB data - B
                    pixels[targetPosition+1] = data[sa+a+1];     // RGB data - G
                    pixels[targetPosition+2] = data[sa+a+2];     // RGB data - R
                    alpha[aPosition] = data[sa+a+3];     // RGB data - A

                    targetPosition += 3;
                    aPosition ++;
                }
            } 

            if (bitsPerPixel == 24)
            {
                // 24 BIT version   
                for (int a = 0; a<pixelCount; a+=3)
                {
                    for (int p=0; p<3; p++)
                    {
                        pixels[a+p] = data[sa+a+p]; 
                    }
                }
            }

            // Rotate pixels to correct order
            ReversePixels();

            if (showInfo)
            {
                Console.WriteLine("\nFile name: {0}", fileName);
                Console.WriteLine("Header identification: {0}", headerField);
                Console.WriteLine("Identified type: {0}\n", bmpDescription);
          
                Console.WriteLine("BMP size = {0} Kb ({1}b)",(BMPsize / 1024).ToString(), BMPsize.ToString());
                Console.WriteLine("Pixel starting address = {0}", startingAddress.ToString());

                // Bitmapcoreheader
                Console.WriteLine("Header size: {0}", sizeOfHeader.ToString());
                Console.WriteLine("Image size: {0}x{1}", width.ToString(), height.ToString());
                Console.WriteLine("Bits per pixel: {0}", bitsPerPixel.ToString());
                Console.WriteLine("IMG size: {0}", imgSize.ToString());
                Console.WriteLine("Planes count: {0}", planesCount.ToString());
                Console.WriteLine("Compression: {0}", compression.ToString());
                //if (compression!=0) Console.WriteLine("Error: Compression type not supported!");              
                if (width % 4 != 0) Console.WriteLine("Error: Wrong width of image! (not multiply of 4)");
            }
        }

        public void LoadFromBMP(string fileName)
        {
            LoadFromBMP(fileName, false);
        }

        public void LoadFromBMPGray(string fileName, bool showInfo)
        {
            string headerField = "";
            string bmpDescription = "Unknown type";
            uint BMPsize;           
            uint startingAddress;
            uint sizeOfHeader;
            ushort bitsPerPixel;

            var fs = new FileStream(fileName, FileMode.Open);
            var len = (int)fs.Length;
            byte[] data = new byte[len];
            fs.Read(data, 0, len);

            // Analyze           
            headerField += Convert.ToChar(data[0]);
            headerField += Convert.ToChar(data[1]);
            if (headerField=="BM") bmpDescription = "Windows 3.1x, 95, NT, ... etc.";
            if (headerField=="BA") bmpDescription = "OS/2 struct bitmap array";
            if (headerField=="CI") bmpDescription = "OS/2 struct color icon";
            if (headerField=="CP") bmpDescription = "OS/2 const color pointer";
            if (headerField=="IC") bmpDescription = "OS/2 struct icon";
            if (headerField=="PT") bmpDescription = "OS/2 pointer";         

            BMPsize = (uint)Tools.CreateInt(data[5],data[4],data[3],data[2]);                     
            startingAddress = (uint)Tools.CreateInt(data[13],data[12],data[11],data[10]);
           
            // Bitmapcoreheader / V4
            sizeOfHeader = (uint)Tools.CreateInt(data[17],data[16],data[15],data[14]);
            width = (ushort)Tools.CreateInt(data[21],data[20],data[19],data[18]);
            height = (ushort)Tools.CreateInt(data[25],data[24],data[23],data[22]);
            ushort planesCount = Tools.CreateWord(data[27],data[26]);
            bitsPerPixel = Tools.CreateWord(data[29],data[28]);
            UInt32 compression = Tools.CreateUInt(data[33],data[32],data[31],data[30]);
            UInt32 imgSize = Tools.CreateUInt(data[37],data[36],data[35],data[34]);

            // bitsPerPixel == 8
            transparent = false;

            // Load pixels
            scanLineSize = width * 3;
            int pixelCount = width * height * 3;
            int dataCount = (int)imgSize;
            // pixelCount = (int)imgSize;

            pixels = new byte[pixelCount];
            uint sa = startingAddress;
            Console.WriteLine("IMG staring address: {0}", startingAddress);

            if (bitsPerPixel == 8)
            {
                // 8 BIT version   
                for (int a = 0; a<dataCount; a++)
                {
                    for (int p=0; p<3; p++)
                    {
                        pixels[(a*3)+p] = data[sa+a]; 
                    }
                }
            }

            // Rotate pixels to correct order
            ReversePixels();

            if (showInfo)
            {
                Console.WriteLine("\nFile name: {0}", fileName);
                Console.WriteLine("Header identification: {0}", headerField);
                Console.WriteLine("Identified type: {0}\n", bmpDescription);
          
                Console.WriteLine("BMP size = {0} Kb ({1}b)",(BMPsize / 1024).ToString(), BMPsize.ToString());
                Console.WriteLine("Pixel starting address = {0}", startingAddress.ToString());

                // Bitmapcoreheader
                Console.WriteLine("Header size: {0}", sizeOfHeader.ToString());
                Console.WriteLine("Image size: {0}x{1}", width.ToString(), height.ToString());
                Console.WriteLine("Bits per pixel: {0}", bitsPerPixel.ToString());
                Console.WriteLine("IMG size: {0}", imgSize.ToString());
                Console.WriteLine("Planes count: {0}", planesCount.ToString());
                Console.WriteLine("Compression: {0}", compression.ToString());
                //if (compression!=0) Console.WriteLine("Error: Compression type not supported!");              
                if (width % 4 != 0) Console.WriteLine("Error: Wrong width of image! (not multiply of 4)");
            }
        }


        public void LoadAlphaFromBMPGray(string fileName, bool showInfo)
        {
            string headerField = "";
            string bmpDescription = "Unknown type";
            uint BMPsize;           
            uint startingAddress;
            uint sizeOfHeader;
            ushort bitsPerPixel;

            var fs = new FileStream(fileName, FileMode.Open);
            var len = (int)fs.Length;
            byte[] data = new byte[len];
            fs.Read(data, 0, len);

            // Analyze           
            headerField += Convert.ToChar(data[0]);
            headerField += Convert.ToChar(data[1]);
            if (headerField=="BM") bmpDescription = "Windows 3.1x, 95, NT, ... etc.";
            if (headerField=="BA") bmpDescription = "OS/2 struct bitmap array";
            if (headerField=="CI") bmpDescription = "OS/2 struct color icon";
            if (headerField=="CP") bmpDescription = "OS/2 const color pointer";
            if (headerField=="IC") bmpDescription = "OS/2 struct icon";
            if (headerField=="PT") bmpDescription = "OS/2 pointer";         

            BMPsize = (uint)Tools.CreateInt(data[5],data[4],data[3],data[2]);                     
            startingAddress = (uint)Tools.CreateInt(data[13],data[12],data[11],data[10]);
           
            // Bitmapcoreheader / V4
            sizeOfHeader = (uint)Tools.CreateInt(data[17],data[16],data[15],data[14]);
            width = (ushort)Tools.CreateInt(data[21],data[20],data[19],data[18]);
            height = (ushort)Tools.CreateInt(data[25],data[24],data[23],data[22]);
            ushort planesCount = Tools.CreateWord(data[27],data[26]);
            bitsPerPixel = Tools.CreateWord(data[29],data[28]);
            UInt32 compression = Tools.CreateUInt(data[33],data[32],data[31],data[30]);
            UInt32 imgSize = Tools.CreateUInt(data[37],data[36],data[35],data[34]);

            // bitsPerPixel == 8
            transparent = true;

            // Load pixels
            int pixelCount = width * height;

            alpha = new byte[pixelCount];
            uint sa = startingAddress;
            Console.WriteLine("IMG staring address: {0}", startingAddress);
   
            for (int a = 0; a<pixelCount; a++)
            {
                    alpha[a] = data[sa+a]; 
            }

            // Rotate pixels to correct order
            alpha = ReversePixelsOrder(alpha,1);

            if (showInfo)
            {
                Console.WriteLine("\nFile name: {0}", fileName);
                Console.WriteLine("Header identification: {0}", headerField);
                Console.WriteLine("Identified type: {0}\n", bmpDescription);
          
                Console.WriteLine("BMP size = {0} Kb ({1}b)",(BMPsize / 1024).ToString(), BMPsize.ToString());
                Console.WriteLine("Pixel starting address = {0}", startingAddress.ToString());

                // Bitmapcoreheader
                Console.WriteLine("Header size: {0}", sizeOfHeader.ToString());
                Console.WriteLine("Image size: {0}x{1}", width.ToString(), height.ToString());
                Console.WriteLine("Bits per pixel: {0}", bitsPerPixel.ToString());
                Console.WriteLine("IMG size: {0}", imgSize.ToString());
                Console.WriteLine("Planes count: {0}", planesCount.ToString());
                Console.WriteLine("Compression: {0}", compression.ToString());
                if ((compression!=0) && (compression!=3)) Console.WriteLine("Error: Compression type not supported!");              
                if (width % 4 != 0) Console.WriteLine("Error: Wrong width of image! (not multiply of 4)");
            }
        }

        public void SaveToBMP24(string fileName)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                // 1. --- Write BMP header (14 bytes) ----
                // 14 bytes
                writer.Write('B');
                writer.Write('M');

                // File size, treba vypocitat (4 bytes)
                UInt32 fileSize = (UInt32) (54 + pixels.Length);
                writer.Write(fileSize);
                //Console.WriteLine(fileSize.ToString());

                // 4 bytes reserved
                writer.Write((UInt32)0);

                // Offset addres, where bitmap can be found (4 bytes)
                writer.Write((UInt32)54);

                // 2. --- DIB Header ---
                // 2.1 --- BITMAPINFOHEADER header ---

                // Size of this header (40 bytes) - 4 bytes
                writer.Write((UInt32)40);

                // Width (signed int - 4 bytes)
                writer.Write((Int32)width);

                // Height (signed int - 4 bytes)
                writer.Write((Int32)height);

                // Color planes (2 bytes), has to be 1
                writer.Write((UInt16)1);

                // Bits per pixel (2 bytes) - 8,16,24,32
                writer.Write((UInt16)24);

                // Compression (signed int - 4 bytes)
                // 0 - means, no compression
                writer.Write((UInt32)0);

                // Image size - RAW data (pixels) size, 4 bytes
                writer.Write((UInt32)pixels.Length);

                // Resolution horizontal (pixels/meter) (4 bytes) - 72 DPI
                writer.Write((Int32)2835);

                // Resolution vertical (pixels/meter) (4 bytes) - 72 DPI
                writer.Write((Int32)2835);

                // Colors in palette size, 4 bytes
                writer.Write((UInt32)0);

                // Important Colors count, 4 bytes
                writer.Write((UInt32)0);

                // PIXELS, 24 BIT
                // 1. reverse order (BMP file contains mirrored image)
                byte[] tmpPixels = ReversePixelsOrder(pixels,3);

                // 2. save reversed image
                writer.Write(tmpPixels);
            }
        }

        // NOT WORKING CORRECTLY YET       
        public void SaveToBMP32(string fileName)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                // 1. --- Write BMP header (14 bytes) ----
                // 14 bytes
                writer.Write('B');
                writer.Write('M');

                // File size, treba vypocitat (4 bytes)
                UInt32 fileSize = (UInt32) (54 + pixels.Length);
                if (transparent) fileSize += (UInt32)alpha.Length;
                writer.Write(fileSize);
                Console.WriteLine(fileSize.ToString());

                // 4 bytes reserved
                writer.Write((UInt32)0);

                // Offset addres, where bitmap can be found (4 bytes)
                writer.Write((UInt32)54);

                // 2. --- DIB Header ---
                // 2.1 --- BITMAPINFOHEADER header ---

                // Size of this header (40 bytes) - 4 bytes
                writer.Write((UInt32)40);

                // Width (signed int - 4 bytes)
                writer.Write((Int32)width);

                // Height (signed int - 4 bytes)
                writer.Write((Int32)height);

                // Color planes (2 bytes), has to be 1
                writer.Write((UInt16)1);

                // Bits per pixel (2 bytes) - 8,16,24,32
                int bitsPerPixel = 24;
                if (transparent) bitsPerPixel = 32;
                writer.Write((UInt16)bitsPerPixel);

                // Compression (signed int - 4 bytes)
                // 0 - means, no compression
                UInt32 tmpCompress = 0;
                //if (transparent) tmpCompress = 3;
                writer.Write((UInt32)tmpCompress);

                // Image size - RAW data (pixels) size, 4 bytes
                int tmpRAWsize = pixels.Length;
                if (transparent) tmpRAWsize += alpha.Length;
                writer.Write((UInt32)tmpRAWsize);

                // Resolution horizontal (pixels/meter) (4 bytes) - 72 DPI
                writer.Write((Int32)2835);

                // Resolution vertical (pixels/meter) (4 bytes) - 72 DPI
                writer.Write((Int32)2835);

                // Colors in palette size, 4 bytes
                writer.Write((UInt32)0);

                // Important Colors count, 4 bytes
                writer.Write((UInt32)0);

                if (transparent)
                {
                    byte[] pixelField = new byte[alpha.Length + pixels.Length];
                    // 32 BIT version
                    // IMG size je nutne, aby sme precitali vsetky pixely

                    // Mix RGB + Alpha togather
                    int targetPosition = 0;
                    int pixelsPosition = 0;
                    //int aPosition = 0;
                    for (int a = 0; a<alpha.Length; a++)
                    { 
                        targetPosition = a * 4;
                        pixelsPosition = a * 3;  
                        
                        // ARGB to BGRA
                        pixelField[targetPosition] = alpha[a];
                        pixelField[targetPosition+1] = pixels[pixelsPosition];   // Red
                        pixelField[targetPosition+2] = pixels[pixelsPosition+1]; // Green
                        pixelField[targetPosition+3] = pixels[pixelsPosition+2];   // BLue
                        
                    }

                    // 2. reverse order (BMP file contains mirrored image)
                    //pixelField = ReversePixelsOrderAlpha(pixelField);
                    pixelField = ReversePixelsOrder(pixelField,4);

                    // 3. save reversed image
                    writer.Write(pixelField);
                }
                else
                {
                    // PIXELS, 24 BIT
                    // 1. reverse order (BMP file contains mirrored image)
                    byte[] tmpPixels = ReversePixelsOrder(pixels,3);

                    // 2. save reversed image
                    writer.Write(tmpPixels);
                }
            }
        }
       
       /*
        public void SaveToPB(string fileName)
        {                 
            int pixelCount = width * height * 3;        
            byte ver = 2;
            
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {     
                writer.Write(ver);
                writer.Write((byte)bitsPerPixel);
                writer.Write((ushort)width);
                writer.Write((ushort)height);
                writer.Write((int)pixelCount);
                writer.Write(pixels, 0, pixelCount);
            }
        }

        public void LoadFromPB(string fileName)
        {                         
            byte ver = 2;
            int pixelCount;
            
            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {     
                ver = reader.ReadByte();
                bitsPerPixel = reader.ReadByte();
                width = (int)reader.ReadUInt16();
                height = (int)reader.ReadUInt16();
                pixelCount = reader.ReadInt32();
                reader.Read(pixels, 0, pixelCount);

                if (bitsPerPixel==32)
                {
                    transparent = true;
                }
                else
                    transparent = false;
            }
        }
        */
        public int GetPosition(int x, int y)
        {
            int res = 0;
            int lineSize = width * 3;
            //if (width == 1366) lineSize = lineSize + 2;

            res = (y * lineSize) + ( x* 3);
            return res;
        }
        
         public int GetPositionAlpha(int x, int y)
        {
            int res = (y * width) + x;
            return res;
        }
        public SColor GetPixel(int x, int y)
        {
            int pos = GetPosition(x,y);
            SColor res = new SColor();

                res.a = 0;
                res.r = pixels[pos];
                res.g = pixels[pos+1];
                res.b = pixels[pos+2];
            
            if (transparent)
            {
                int apos = GetPositionAlpha(x,y);
                res.a = alpha[pos];
            }

            return res;
        }
    
        public void ShowPBInfo()
        {
            Console.WriteLine("");
            Console.WriteLine("Size: {0}x{1}", width.ToString(), height.ToString());
            Console.WriteLine("ScanLineWidth: {0}", scanLineSize.ToString());
            //Console.WriteLine("Bits per pixel: {0}, Pixel size:{1}", bitsPerPixel.ToString(), pixelSize.ToString());
            Console.WriteLine("Transparent: {0}", transparent.ToString());
            Console.WriteLine("Size of RGB pixels: {0}", pixels.Length.ToString());
            if (transparent)
            Console.WriteLine("Size of Alpha channel pixels: {0}", alpha.Length.ToString());
            else
            Console.WriteLine("No alpha channel is present.");
        }
    }
}