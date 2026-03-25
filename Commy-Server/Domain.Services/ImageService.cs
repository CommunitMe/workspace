using Domain.Models.Enums;
using Domain.Services.Abstractions;
using Domain.Services.Models;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Drawing;

namespace Domain.Services
{
    public class ImageService : IImageService
    {
        private readonly ILogger<EventService> logger;

        public ImageService(
            ILogger<EventService> logger
        )
        {
            this.logger = logger;
        }

        public List<string> GetAllImageUIDsByType(ImageLocationType imageType)
        {
            string directoryPath = GetImagesDirectoryPath(imageType);
            var result = Directory.GetFiles(directoryPath).Select(path => Path.GetFileNameWithoutExtension(path)).ToList();
            return result;
        }

        public void SaveImage(ImageLocationType imageType, string uid, Stream imageStream)
        {
            string imageFilePath = GetFilePath(imageType, uid);

            var fileStream = File.Create(imageFilePath);
            imageStream.CopyTo(fileStream);
            fileStream.Close();
        }

        public void DeleteImage(ImageLocationType imageType, string uid)
        {
            string imageFilePath = GetFilePath(imageType, uid);
            File.Delete(imageFilePath);
        }

        public Stream? GetImageByUID(ImageLocationType profile, string uid, int? width, int? height)
        {
            string filePath = GetFilePath(profile, uid);
            Stream? result = null;
            try
            {
                using (SKStreamAsset asset = SKFileStream.OpenStream(filePath))
                {
                    result = ResizeImage(asset, width, height);
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to get image from '{0}'", filePath);
            }

            return result;
        }

        public Stream GenerateCircle(string firstName, string lastName, ProfileCircleProperties circleProperties)
        {
            var avatarString = string.Format("{0}{1}", firstName[0], lastName[0]).ToUpper();
            var bitmap = new SKBitmap(circleProperties.width, circleProperties.height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);

            var midy = canvas.LocalClipBounds.Size.ToSizeI().Height / 2;
            var midx = canvas.LocalClipBounds.Size.ToSizeI().Width / 2;
            var radius = midx;

            var circleFill = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeJoin = SKStrokeJoin.Miter,
                Color = SKColor.Parse(circleProperties.bgColor)
            };
            canvas.DrawCircle(midx, midy, radius, circleFill);


            var family = SKTypeface.FromFamilyName(circleProperties.fontFamily, SKFontStyleWeight.Normal, SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright);
            var textSize = circleProperties.textSize ?? midx / 1.2f;
            var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = SKColor.Parse(circleProperties.textColor),
                TextSize = textSize,
                TextAlign = SKTextAlign.Center,
                Typeface = family
            };
            var rect = new SKRect();
            paint.MeasureText(avatarString, ref rect);
            canvas.DrawText(avatarString, midx, midy + 5, paint);
            var skImage = SKImage.FromBitmap(bitmap);
            var result = (skImage.Encode(SKEncodedImageFormat.Png, 100)).AsStream();
            return result;
        }

        private Stream ResizeImage(SKStreamAsset asset, int? width, int? height)
        {
            using SKBitmap bitmap = SKBitmap.Decode(asset);
            SKImageInfo scaledSize = GetScaledSize(bitmap, width, height);
            using SKBitmap scaledBitmap = new SKBitmap(scaledSize);
            bitmap.ScalePixels(scaledBitmap, SKFilterQuality.High);
            using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
            using SKData data = scaledImage.Encode();

            Stream output = new MemoryStream();
            data.SaveTo(output);
            output.Seek(0, 0);

            return output;
        }

        private string GetFilePath(ImageLocationType locationType, string uid)
        {
            string imagesDirectory = GetImagesDirectoryPath(locationType);
            return Path.Combine(imagesDirectory, uid + ".jpg");
        }

        private string GetImagesDirectoryPath(ImageLocationType locationType)
        {
            string imageDir = locationType switch
            {
                ImageLocationType.ServiceProvider => "service-provider",
                ImageLocationType.MarketItem => "market-item",
                ImageLocationType.Commmunity => "community",
                ImageLocationType.Coupon => "coupon",
                ImageLocationType.Event => "event",
                ImageLocationType.Profile => "profile",
                ImageLocationType.Category => "categories",
                _ => throw new Exception("Unsupported image location type"),
            };

            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", imageDir);
        }

        private SKImageInfo GetScaledSize(SKBitmap bitmap, int? width, int? height)
        {
            if (!width.HasValue && !height.HasValue)
            {
                return new SKImageInfo(bitmap.Width, bitmap.Height);
            }

            int w = width.HasValue ? width.Value : height!.Value;
            int h = height.HasValue ? height.Value : width!.Value;

            return new SKImageInfo(w, h);
        }
    }
}
