using Aspose.Imaging.FileFormats.Png;
using Aspose.Imaging.ImageOptions;
using Aspose.Imaging;
using Aspose.Imaging;
using Aspose.Imaging.FileFormats.Png;
using Aspose.Imaging.ImageOptions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingUtils
{
    public static class JpegConversion
    {
        public static byte[] ConvertJpegToBmp(Stream pictureStream)
        {
            using (var image = Image.Load(pictureStream))
            {
                BmpOptions options = new ();
                MemoryStream ms = new();
                image.Save(ms, options);
                return ms.ToArray();
            }
        }

        public static (Dictionary<string, VectorRasterizationOptions> Import, Dictionary<string, ImageOptionsBase> Export) GetAvailableImageFormats()
        {
            ////////////////////////////////
            ///Raster and vector formats to that we can export images
            ////////////////////////////////


            //Raster image formats that support both - save and load and their default save options
            Dictionary<string, ImageOptionsBase> rasterFormatsThatSupportExportAndImport = new Dictionary<string, ImageOptionsBase>()
            {
                { "bmp", new BmpOptions()},
                { "gif", new GifOptions()},
                { "dicom", new DicomOptions()},
                { "jpg", new JpegOptions()},
                { "jpeg", new JpegOptions()},
                { "jpeg2000", new Jpeg2000Options() },
                { "j2k", new Jpeg2000Options { Codec = Aspose.Imaging.FileFormats.Jpeg2000.Jpeg2000Codec.J2K } },
                { "jp2", new Jpeg2000Options { Codec = Aspose.Imaging.FileFormats.Jpeg2000.Jpeg2000Codec.Jp2 }},
                { "png",new PngOptions(){ ColorType = PngColorType.TruecolorWithAlpha} },
                { "apng", new ApngOptions()},
                { "tiff", new TiffOptions(Aspose.Imaging.FileFormats.Tiff.Enums.TiffExpectedFormat.Default)},
                { "tif", new TiffOptions(Aspose.Imaging.FileFormats.Tiff.Enums.TiffExpectedFormat.Default)},
                { "tga", new TgaOptions()},
                { "webp", new WebPOptions()}
            };

            //Vector image formats that support both - save and load, their default save options
            //and their rasterization options when exporting to another vector image
            Dictionary<string, (ImageOptionsBase, VectorRasterizationOptions)> vectorFormatsThatSupportExportAndImport
                = new Dictionary<string, (ImageOptionsBase, VectorRasterizationOptions)>()
            {
                { "emf", (new EmfOptions(),new EmfRasterizationOptions()) },
                { "svg", (new SvgOptions(), new SvgRasterizationOptions())},
                { "wmf", (new WmfOptions(), new WmfRasterizationOptions())},
                { "emz", (new EmfOptions(){ Compress = true }, new EmfRasterizationOptions())},
                { "wmz", (new WmfOptions(){ Compress = true }, new WmfRasterizationOptions())},
                { "svgz", (new SvgOptions(){ Compress = true }, new SvgRasterizationOptions())},
            };

            ////////////////////////////////
            ///Raster and vector formats from which we can load images
            ////////////////////////////////

            //Formats that can be only saved (supported only save to this formats)
            Dictionary<string, ImageOptionsBase> formatsOnlyForExport = new Dictionary<string, ImageOptionsBase>()
            {
                { "psd", new PsdOptions()},
                { "dxf", new DxfOptions(){ TextAsLines = true,ConvertTextBeziers = true} },
                { "pdf", new PdfOptions()},
                { "html", new Html5CanvasOptions()},
            };

            //Raster formats that can be only loaded            
            List<string> formatsOnlyForImport = new List<string>()
            {
                "djvu", "dng", "dib"
            };

            //Vector formats only for loading and their rasterization options when exporting to another vector format
            Dictionary<string, VectorRasterizationOptions> vectorFormatsOnlyForImport = new Dictionary<string, VectorRasterizationOptions>()
            {
                {"eps", new EpsRasterizationOptions()},
                {"cdr", new CdrRasterizationOptions() },
                {"cmx", new CmxRasterizationOptions() },
                {"otg", new OtgRasterizationOptions() },
                {"odg", new OdgRasterizationOptions() }
            };

            //Get total set of formats to what we can export images
            Dictionary<string, ImageOptionsBase> exportFormats = vectorFormatsThatSupportExportAndImport
                .ToDictionary(s => s.Key, s => s.Value.Item1)
                .Union(formatsOnlyForExport)
                .Concat(rasterFormatsThatSupportExportAndImport)
                .ToDictionary(s => s.Key, s => s.Value);

            //Get total set of formats that can be loaded
            Dictionary<string, VectorRasterizationOptions> importFormats = vectorFormatsOnlyForImport
                .Union(formatsOnlyForImport.ToDictionary(s => s, s => new VectorRasterizationOptions()))
                .Union(vectorFormatsThatSupportExportAndImport.ToDictionary(s => s.Key, s => s.Value.Item2))
                .ToDictionary(s => s.Key, s => s.Value);

            return (Import: importFormats, Export: exportFormats);
        }
    }
}
