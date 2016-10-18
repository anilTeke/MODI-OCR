using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MODI;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

public partial  class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e) { }

    protected void Upload(object sender, EventArgs e)
    {
        string filePath = Server.MapPath(Path.GetFileName(FileUpload1.PostedFile.FileName));
        FileUpload1.SaveAs(filePath);
        string extractText = this.ExtractTextFromImageBasic(filePath); //ExtractTextFromImageBasic : string indexof & subsitution, ExtractTextFromImage : Image Process
        lblText.Text = extractText.Replace(Environment.NewLine, "<br />");
    }

    private string ExtractTextFromImageBasic(string filePath)
    {
        Document modiDocument = new Document();
        modiDocument.Create(filePath);
        modiDocument.OCR(MiLANGUAGES.miLANG_ENGLISH);
        MODI.Image modiImage = (modiDocument.Images[0] as MODI.Image);

        string extractedText = modiImage.Layout.Text;
        modiDocument.Close();

        int index = (int) extractedText.IndexOf("No") + 3;
        extractedText = extractedText.Substring(index, 10);

        return extractedText;
    }

    private string ExtractTextFromImage(string filePath)
    {
        Bitmap bmp = new Bitmap(filePath);
        string croppedImageFile = "", resizeImageFile = "";
        try
        {
            // 1: (319 x 200), (190, 107, 50, 15)
            //bmp = bmp.Clone(new Rectangle(190, 107, 50, 15), System.Drawing.Imaging.PixelFormat.DontCare);
            //filePath = "C:\\Users\\Kullanici\\Desktop\\Image\\Kart\\1.bmp";

            // Sample 2: (455x 313), (261, 197, 80, 17)
            bmp = bmp.Clone(new Rectangle(261, 197, 80, 17), System.Drawing.Imaging.PixelFormat.DontCare);
            //bmp = Resize(bmp);

            croppedImageFile = "C:\\Users\\Kullanici\\Desktop\\Image\\Kart\\2.bmp";
            resizeImageFile = "C:\\Users\\Kullanici\\Desktop\\Image\\Kart\\22.bmp";
            bmp.Save(croppedImageFile);
            Resize(croppedImageFile, resizeImageFile, 20);

            //bmp = (Bitmap)Bitmap.FromFile(resizeImageFile);

            //bmp = SetGrayscale(bmp);
            //bmp = RemoveNoise(bmp);
            //filePath = "C:\\Users\\Kullanici\\Desktop\\Image\\Kart\\2.bmp";
            //bmp.Save(resizeImageFile);
        }
        catch (Exception ex) { string se = ex.ToString(); }

        //filePath = VaryQualityLevel(filePath);
        
        Document modiDocument = new Document();
        modiDocument.Create(resizeImageFile);
        modiDocument.OCR(MiLANGUAGES.miLANG_SYSDEFAULT); // hata veriyor.
        MODI.Image modiImage = (modiDocument.Images[0] as MODI.Image);

        string extractedText = modiImage.Layout.Text;
        modiDocument.Close();
        return extractedText;

    } // ExtractTextFromImage



    public Bitmap Resize(Bitmap bmp)
    {
        int scale = 20;
        int width = scale * bmp.Width;
        int height = scale * bmp.Height;
        Bitmap newImage = new Bitmap(width, height);
        using (Graphics gr = Graphics.FromImage(bmp))
        {
            gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
            gr.DrawImage(bmp, new Rectangle(width, height, newImage.Width, newImage.Height));
        }
        return newImage;
    }//int * width & height

    public void Resize(string imageFile, string outputFile, double scaleFactor)
    {
        using (var srcImage = System.Drawing.Image.FromFile(imageFile))
        {
            var newWidth = (int)(srcImage.Width * scaleFactor);
            var newHeight = (int)(srcImage.Height * scaleFactor);
            using (var newImage = new Bitmap(newWidth, newHeight))
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(srcImage, new Rectangle(0, 0, newWidth, newHeight));
                newImage.Save(outputFile);
            }
        }
    }

    //stackoverflow : "Sharpen on a Bitmap using C#" niaher
    public static Bitmap Sharpen(Bitmap image)
    {
        Bitmap sharpenImage = (Bitmap)image.Clone();

        int filterWidth = 3;
        int filterHeight = 3;
        int width = image.Width;
        int height = image.Height;

        // Create sharpening filter.
        double[,] filter = new double[filterWidth, filterHeight];
        filter[0, 0] = filter[0, 1] = filter[0, 2] = filter[1, 0] = filter[1, 2] = filter[2, 0] = filter[2, 1] = filter[2, 2] = -1;
        filter[1, 1] = 9;

        double factor = 1.0;
        double bias = 0.0;

        Color[,] result = new Color[image.Width, image.Height];

        // Lock image bits for read/write.
        BitmapData pbits = sharpenImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

        // Declare an array to hold the bytes of the bitmap.
        int bytes = pbits.Stride * height;
        byte[] rgbValues = new byte[bytes];

        // Copy the RGB values into the array.
        System.Runtime.InteropServices.Marshal.Copy(pbits.Scan0, rgbValues, 0, bytes);

        int rgb;
        // Fill the color array with the new sharpened color values.
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                double red = 0.0, green = 0.0, blue = 0.0;

                for (int filterX = 0; filterX < filterWidth; filterX++)
                {
                    for (int filterY = 0; filterY < filterHeight; filterY++)
                    {
                        int imageX = (x - filterWidth / 2 + filterX + width) % width;
                        int imageY = (y - filterHeight / 2 + filterY + height) % height;

                        rgb = imageY * pbits.Stride + 3 * imageX;

                        red += rgbValues[rgb + 2] * filter[filterX, filterY];
                        green += rgbValues[rgb + 1] * filter[filterX, filterY];
                        blue += rgbValues[rgb + 0] * filter[filterX, filterY];
                    }
                    int r = Math.Min(Math.Max((int)(factor * red + bias), 0), 255);
                    int g = Math.Min(Math.Max((int)(factor * green + bias), 0), 255);
                    int b = Math.Min(Math.Max((int)(factor * blue + bias), 0), 255);

                    result[x, y] = Color.FromArgb(r, g, b);
                }
            }
        }

        // Update the image with the sharpened pixels.
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                rgb = y * pbits.Stride + 3 * x;

                rgbValues[rgb + 2] = result[x, y].R;
                rgbValues[rgb + 1] = result[x, y].G;
                rgbValues[rgb + 0] = result[x, y].B;
            }
        }

        // Copy the RGB values back to the bitmap.
        System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, pbits.Scan0, bytes);
        // Release image bits.
        sharpenImage.UnlockBits(pbits);

        return sharpenImage;
    }

    //stackoverflow : "image processing to improve tesseract OCR accuracy. Resize, SetGrayscale, RemoveNoise" Sathyaraj P
    //It calls in order SetGrayScale and RemoveNoise
    public Bitmap Resize(Bitmap bmp, int newWidth, int newHeight)
    {

        Bitmap temp = (Bitmap)bmp;

        Bitmap bmap = new Bitmap(newWidth, newHeight, temp.PixelFormat);

        double nWidthFactor = (double)temp.Width / (double)newWidth;
        double nHeightFactor = (double)temp.Height / (double)newHeight;

        double fx, fy, nx, ny;
        int cx, cy, fr_x, fr_y;
        Color color1 = new Color();
        Color color2 = new Color();
        Color color3 = new Color();
        Color color4 = new Color();
        byte nRed, nGreen, nBlue;

        byte bp1, bp2;

        for (int x = 0; x < bmap.Width; ++x)
        {
            for (int y = 0; y < bmap.Height; ++y)
            {

                fr_x = (int)Math.Floor(x * nWidthFactor);
                fr_y = (int)Math.Floor(y * nHeightFactor);
                cx = fr_x + 1;
                if (cx >= temp.Width) cx = fr_x;
                cy = fr_y + 1;
                if (cy >= temp.Height) cy = fr_y;
                fx = x * nWidthFactor - fr_x;
                fy = y * nHeightFactor - fr_y;
                nx = 1.0 - fx;
                ny = 1.0 - fy;

                color1 = temp.GetPixel(fr_x, fr_y);
                color2 = temp.GetPixel(cx, fr_y);
                color3 = temp.GetPixel(fr_x, cy);
                color4 = temp.GetPixel(cx, cy);

                // Blue
                bp1 = (byte)(nx * color1.B + fx * color2.B);

                bp2 = (byte)(nx * color3.B + fx * color4.B);

                nBlue = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                // Green
                bp1 = (byte)(nx * color1.G + fx * color2.G);

                bp2 = (byte)(nx * color3.G + fx * color4.G);

                nGreen = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                // Red
                bp1 = (byte)(nx * color1.R + fx * color2.R);

                bp2 = (byte)(nx * color3.R + fx * color4.R);

                nRed = (byte)(ny * (double)(bp1) + fy * (double)(bp2));

                bmap.SetPixel(x, y, System.Drawing.Color.FromArgb(255, nRed, nGreen, nBlue));
            }
        }
        
        bmap = SetGrayscale(bmap);
        bmap = RemoveNoise(bmap);

        return bmap;
    } 
    public Bitmap SetGrayscale(Bitmap img)
    {

        Bitmap temp = (Bitmap)img;
        Bitmap bmap = (Bitmap)temp.Clone();
        Color c;
        for (int i = 0; i < bmap.Width; i++)
        {
            for (int j = 0; j < bmap.Height; j++)
            {
                c = bmap.GetPixel(i, j);
                byte gray = (byte)(.299 * c.R + .587 * c.G + .114 * c.B);

                bmap.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
            }
        }
        return (Bitmap)bmap.Clone();

    }
    public Bitmap RemoveNoise(Bitmap bmap)
    {

        for (var x = 0; x < bmap.Width; x++)
        {
            for (var y = 0; y < bmap.Height; y++)
            {
                var pixel = bmap.GetPixel(x, y);
                if (pixel.R < 162 && pixel.G < 162 && pixel.B < 162)
                    bmap.SetPixel(x, y, Color.Black);
            }
        }

        for (var x = 0; x < bmap.Width; x++)
        {
            for (var y = 0; y < bmap.Height; y++)
            {
                var pixel = bmap.GetPixel(x, y);
                if (pixel.R > 162 && pixel.G > 162 && pixel.B > 162)
                    bmap.SetPixel(x, y, Color.White);
            }
        }

        return bmap;
    }

    private string OCR(string fileToOCR)
    {
        MODI.Document md = new MODI.Document();

        md.Create(fileToOCR);

        md.OCR(MODI.MiLANGUAGES.miLANG_ENGLISH, true, true);

        MODI.Image img = (MODI.Image)md.Images[0];

        MODI.Layout layout = img.Layout;

        layout = img.Layout;

        string result = layout.Text;

        md.Close(false);

        return result;

    }

    public static Stream ConvertImage(Stream originalStream, ImageFormat format)
    {
        var image = System.Drawing.Image.FromStream(originalStream);

        var stream = new MemoryStream();
        image.Save(stream, format);
        stream.Position = 0;
        return stream;
    }// var outputStream = gifStream.ConvertImage(ImageFormat.Png);

    private string VaryQualityLevel(string filePath)
    {
        // Get a bitmap.
        Bitmap bmp = new Bitmap(filePath);
        ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

        // Create an Encoder object based on the GUID for the Quality parameter category.
        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

        // Create an EncoderParameters object.
        // An EncoderParameters object has an array of EncoderParameter objects. In this case, there is only one EncoderParameter object in the array.
        EncoderParameters myEncoderParameters = new EncoderParameters(1);

        // Save the bitmap as a JPG file with zero quality level compression.
        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
        myEncoderParameters.Param[0] = myEncoderParameter;
        filePath = "C:\\Users\\Kullanici\\Desktop\\Image\\Kart\\TestPhotoQualityHundred.jpg";
        bmp.Save(filePath, jgpEncoder, myEncoderParameters);

        return filePath;
    }//It calls GetEncoder and converts bmitmap to jpeg.
    private ImageCodecInfo GetEncoder(ImageFormat format)
    {

        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }
        return null;
    }

}