using System;

namespace SpeedPicture
{
    /// <summary>
    /// Structure, that holds RGBA values of color.
    /// </summary>
    public struct SColor
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;     // Alpha channel

        /// <summary>
        /// Initializes color variable.
        /// </summary>
        /// <param name="r">Red value of color</param>
        /// <param name="g">Green value of color</param>
        /// <param name="b">Blue value of color</param>
        public SColor(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 255;
        }

        /// <summary>
        /// Initializes color variable.
        /// </summary>
        /// <param name="r">Red value of color</param>
        /// <param name="g">Green value of color</param>
        /// <param name="b">Blue value of color</param>
        /// <param name="a">Alpha (transparency) value of color</param>
        public SColor(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    
        public void SetColor(byte r, byte g, byte b)
        {
            SetColor(r,g,b,0);
        }

        public void SetColor(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }

    /// <summary>
    /// Coordinates of point in image.
    /// </summary>
    public struct SPoint
    {
        public int x;
        public int y;

        /// <summary>
        /// Initializes values for point.
        /// </summary>
        /// <param name="x">X coordinate for point</param>
        /// <param name="y">Y coordinate for point</param>
        public SPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>
    /// This class is used for rendering objects into the image (PBImage class).
    /// </summary>
    public class Painter
    {
        PBImage image;
        SColor penColor;

        /// <summary>
        /// Initialization of Painter class.
        /// </summary>
        /// <param name="image">Target image, where drawing will be performed.</param>
        public Painter(ref PBImage image)
        {
            this.image = image;
        }

        public void SetPenColor(SColor color)
        {
            penColor = color;
        }

        /// <summary>
        /// Draws pixel with prefered color into the image.
        /// Pencolor property is used as color value for pixel.
        /// </summary>
        /// <param name="x">Pixel X position</param>
        /// <param name="y">Pixel Y position</param>
        public void PutPixel(int x, int y)
        {
            if (image.transparent)
            {
                int ap = image.GetPositionAlpha(x,y);
                image.alpha[ap] = penColor.a;
            }

            int p = image.GetPosition(x,y);
            image.pixels[p] = penColor.r;
            image.pixels[p+1] = penColor.g;
            image.pixels[p+2] = penColor.b;
        }

        /// <summary>
        /// Draws pixel with prefered color into the image.
        /// </summary>
        /// <param name="x">Pixel X position</param>
        /// <param name="y">Pixel Y position</param>
        /// <param name="color">Pixel color</param>
        public void PutPixel(int x, int y, SColor color)
        {
            if (image.transparent)
            {
                int ap = image.GetPositionAlpha(x,y);
                image.alpha[ap] = color.a;
            }

            int p = image.GetPosition(x,y);
            image.pixels[p] = color.r;
            image.pixels[p+1] = color.g;
            image.pixels[p+2] = color.b;
        }
        
        /// <summary>
        /// Draws line into the image.
        /// </summary>
        /// <param name="x1">First point X position</param>
        /// <param name="y1">First point Y position</param>
        /// <param name="x2">Second point X position</param>
        /// <param name="y2">Second point Y position</param>
        public void Line(int x1, int y1, int x2, int y2)
        {
            int startX = x1;
            int startY = y1;
            int sizeY = y2 - y1;
            int sizeX = x2 - x1;

            int cycleStepX = 1;
            int cycleStepY = 1;

            if (sizeX<0) cycleStepX = -1; else cycleStepX = 1;
            if (sizeY<0) cycleStepY = -1; else cycleStepY = 1;

            sizeX = Math.Abs(sizeX);
            sizeY = Math.Abs(sizeY);

            if (sizeX>sizeY)
            {
                int roundCount = Math.Abs(sizeX);
                float step = (float)sizeY / sizeX;
                //Console.WriteLine("Step podla X: " + step.ToString());      
                for (int x=0;x<roundCount;x++)
                {
                    PutPixel(startX + (x * cycleStepX), startY + (int)((x * cycleStepY) * step));
                }
            }
            else
            {
                int roundCount = Math.Abs(sizeY);
                float step = (float)sizeX / sizeY;
                for (int y=0;y<roundCount;y++)
                {
                    PutPixel(startX + (int)((y * cycleStepX) * step), startY + (y * cycleStepY));
                }
            }
        }
    
        /// <summary>
        /// Draws rectangle into image.
        /// </summary>
        /// <param name="x">X - position of rectangle</param>
        /// <param name="y">Y - position of rectangle</param>
        /// <param name="width">Width of rectangle</param>
        /// <param name="height">Height of rectangle</param>
        public void Rectangle(int x, int y, int width, int height)
        {
            Line(x,y,x+width,y);
            Line(x,y,x,y+height);
            Line(x+width,y,x+width,y+height);
            Line(x,y+height,x+width,y+height);
        }
   
        // Algorithm from:
        // https://www.geeksforgeeks.org/midpoint-ellipse-drawing-algorithm/?ref=rp
        // Updated by me :-)
        /// <summary>
        /// Draw ellipse into the image.
        /// </summary>
        /// <param name="px">X - position of the ellipse</param>
        /// <param name="py">Y - position of the ellipse</param>
        /// <param name="a">Size in X axe</param>
        /// <param name="b">Size in Y axe</param>
        public void Ellipse(int px, int py, int a, int b)
        {
            double rx = a;
            double ry = b;  
            double xc = px; 
            double yc = py;

            double dx, dy, d1, d2, x, y; 
            x = 0; 
            y = ry; 
  
            // Initial decision parameter of region 1 
            d1 = (ry * ry) - (rx * rx * ry) + 
                            (0.25f * rx * rx); 
            dx = 2 * ry * ry * x; 
            dy = 2 * rx * rx * y; 
      
            // For region 1 
            while (dx < dy) 
            { 
                PutPixel((int)(x + xc), (int)(y + yc));
                PutPixel((int)(-x + xc), (int)(y + yc));
                PutPixel((int)(x + xc), (int)(-y + yc));
                PutPixel((int)(-x + xc), (int)(-y + yc));

                // Checking and updating value of 
                // decision parameter based on algorithm 
                if (d1 < 0)  
                { 
                    x++; 
                    dx = dx + (2 * ry * ry); 
                    d1 = d1 + dx + (ry * ry); 
                } 
                else
                { 
                    x++; 
                    y--; 
                    dx = dx + (2 * ry * ry); 
                    dy = dy - (2 * rx * rx); 
                    d1 = d1 + dx - dy + (ry * ry); 
                } 
            } 
  
            // Decision parameter of region 2 
            d2 = ((ry * ry) * ((x + 0.5f) * (x + 0.5f))) 
                + ((rx * rx) * ((y - 1) * (y - 1))) 
                - (rx * rx * ry * ry); 
  
            // Plotting points of region 2 
            while (y >= 0) 
            { 
                PutPixel((int)(x + xc), (int)(y + yc));
                PutPixel((int)(-x + xc), (int)(y + yc));
                PutPixel((int)(x + xc), (int)(-y + yc));
                PutPixel((int)(-x + xc), (int)(-y + yc));
                // Checking and updating parameter 
                // value based on algorithm 
                if (d2 > 0) 
                { 
                    y--; 
                    dy = dy - (2 * rx * rx); 
                    d2 = d2 + (rx * rx) - dy; 
                } 
                else 
                { 
                    y--; 
                    x++; 
                    dx = dx + (2 * ry * ry); 
                    dy = dy - (2 * rx * rx); 
                    d2 = d2 + dx - dy + (rx * rx); 
                } 
            } 
        } 
    
        /// <summary>
        /// Renders (open or closed) polygon into the image.
        /// </summary>
        /// <param name="points">List of points of polygon</param>
        /// <param name="open">Is it closed, or open polygon?</param>
        public void Polygon(SPoint[] points, bool open)
        {
            for (int a=0;a<points.Length-1;a++)
            {
                Line(points[a].x, points[a].y,points[a+1].x, points[a+1].y);
            }

            if (!open) 
            {
                // Closing line
                int last = points.Length - 1;
                Line(points[0].x, points[0].y,points[last].x, points[last].y);
            }
        }
    
        /// <summary>
        /// Draws image into target image, omiting any transparency. 
        /// </summary>
        /// <param name="x">X-position of image, that is drawn</param>
        /// <param name="y">Y-position of image, that is drawn</param>
        /// <param name="inImage">Image, that will be drawn (without any transparency)</param>
        public void DrawImage(int x, int y, PBImage inImage)
        {
            SColor tmpColor = new SColor();
            tmpColor.a = 0;

            bool error = true;
            if ((x>=0) && (x<image.width))
            {
                if ((y>=0) && (y<image.height))
                {
                    error = false;
                }   
            }

            if (!error)
            {
                int rightPoint = x + inImage.width;
                int bottomPoint = y + inImage.height;
                
                if (rightPoint>image.width) rightPoint = image.width;
                if (bottomPoint>image.height) bottomPoint = image.height;

                int sizeX = rightPoint - x - 1;
                int sizeY = bottomPoint - y - 1;

                // Here is "Copy code"
                // No ALPHA channel, no transparency
                for (int sy=0; sy<sizeY; sy++)
                {
                    for (int sx=0; sx<sizeX; sx++)
                    {
                        int dx = sx + x;
                        int dy = sy + y;
                        
                        int pos = inImage.GetPosition(sx,sy);
                        
                        tmpColor.r = inImage.pixels[pos];
                        tmpColor.g = inImage.pixels[pos+1];
                        tmpColor.b = inImage.pixels[pos+2];

                        //if (image.transparent) tmpColor.a = image.pixels[image.GetPosition(dx,dy)];
                        //tmpColor = inImage.GetPixel(sx,sy);
                        PutPixel(dx,dy,tmpColor);     
                    }
                }
            }
        }

        /// <summary>
        /// Draws image into target image, using key-color method. 
        /// </summary>
        /// <param name="x">X-position of image, that is drawn</param>
        /// <param name="y">Y-position of image, that is drawn</param>
        /// <param name="inImage">Image, that will be drawn</param>
        /// <param name="keyColor">Color, that will be omited from drawing</param>
        public void DrawImageKey(int x, int y, PBImage inImage, SColor keyColor)
        {
            SColor tmpColor = new SColor();
            tmpColor.a = 0;

            bool error = true;
            if ((x>=0) && (x<image.width))
            {
                if ((y>=0) && (y<image.height))
                {
                    error = false;
                }   
            }

            if (!error)
            {
                int rightPoint = x + inImage.width;
                int bottomPoint = y + inImage.height;
                
                if (rightPoint>image.width) rightPoint = image.width;
                if (bottomPoint>image.height) bottomPoint = image.height;

                int sizeX = rightPoint - x - 1;
                int sizeY = bottomPoint - y - 1;

                // Here is "Copy code"
                // No ALPHA channel, but key transparency
                for (int sy=0; sy<sizeY; sy++)
                {
                    for (int sx=0; sx<sizeX; sx++)
                    {
                        int dx = sx + x;
                        int dy = sy + y;
                        
                        int pos = inImage.GetPosition(sx,sy);
                       
                        tmpColor.r = inImage.pixels[pos];
                        tmpColor.g = inImage.pixels[pos+1];
                        tmpColor.b = inImage.pixels[pos+2];

                        bool condition = (keyColor.r != tmpColor.r);
                        condition = condition && (keyColor.g != tmpColor.g);
                        condition = condition && (keyColor.b != tmpColor.b);
                        if (condition) 
                        {
                            //if (image.transparent) tmpColor.a = image.pixels[image.GetPosition(dx,dy)];
                            PutPixel(dx,dy,tmpColor);
                        }     
                    }
                }
            }
        }
    
        /// <summary>
        /// Draws image into target image, using alpha blending method.
        /// Transparency is computed from alpha channel values.
        /// </summary>
        /// <param name="x">X-position of image, that is drawn</param>
        /// <param name="y">Y-position of image, that is drawn</param>
        /// <param name="inImage">Image, that will be drawn with it's transparency</param>
        public void DrawImageAlpha(int x, int y, PBImage inImage)
        {
            SColor tmpColor = new SColor();
            tmpColor.a = 0;
            bool error = true;
            if ((x>=0) && (x<image.width))
            {
                if ((y>=0) && (y<image.height))
                {
                    error = false;
                }   
            }

            if (!error)
            {
                int rightPoint = x + inImage.width;
                int bottomPoint = y + inImage.height;
                
                if (rightPoint>image.width) rightPoint = image.width;
                if (bottomPoint>image.height) bottomPoint = image.height;

                int sizeX = rightPoint - x - 1;
                int sizeY = bottomPoint - y - 1;

                // Here is "Copy code"
                // ALPHA channel, with transparency
                for (int sy=0; sy<sizeY; sy++)
                {
                    for (int sx=0; sx<sizeX; sx++)
                    {
                        int dx = sx + x;
                        int dy = sy + y;
                        
                        int pos = inImage.GetPosition(sx,sy);
                        int posa = inImage.GetPositionAlpha(sx,sy);
                        int posTarget = image.GetPosition(dx,dy);   

                        double alpha = 0;
                        if (inImage.alpha[posa]>0) alpha = (double)inImage.alpha[posa]/255;
                        double ialpha = 1 - alpha;

                        tmpColor.r = (byte)((image.pixels[posTarget] * ialpha) + (inImage.pixels[pos] * alpha));
                        tmpColor.g = (byte)((image.pixels[posTarget+1] * ialpha) + (inImage.pixels[pos+1] * alpha));
                        tmpColor.b = (byte)((image.pixels[posTarget+2] * ialpha) + (inImage.pixels[pos+2] * alpha));

                        //if (image.transparent) tmpColor.a = image.pixels[image.GetPosition(dx,dy)];
                        PutPixel(dx,dy,tmpColor);     
                    }
                }
            }
        }
    
        public void FloodFill(int x, int y, SColor fillColor, SColor defaultColor)  
        {
            //Console.WriteLine("Pos {0} : {1}",x.ToString(), y.ToString());
            int pos = image.GetPosition(x,y);
            //Console.WriteLine(pos.ToString());
            SColor pixelColor = image.GetPixel(x,y);
            //Console.Write(pixelColor.r.ToString()+", ");
            //Console.Write(pixelColor.g.ToString()+", ");
            //Console.WriteLine(pixelColor.b.ToString());

            if ((pixelColor.r == defaultColor.r) && 
            (pixelColor.g == defaultColor.g) &&
            (pixelColor.b == defaultColor.b))
            {  
                PutPixel(x,y,fillColor);  
                FloodFill(x+1,y,fillColor,defaultColor);  
                FloodFill(x-1,y,fillColor,defaultColor);  
                FloodFill(x,y+1,fillColor,defaultColor);  
                FloodFill(x,y-1,fillColor,defaultColor);  
            }  
        }  
    
        /*
        public void FilledRectangle(int x, int y, int width, int height, SColor fillColor)
        {
            Rectangle(x,y,width,height);

        }
        */
    }
}