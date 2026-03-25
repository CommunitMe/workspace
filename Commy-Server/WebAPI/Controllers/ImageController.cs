using Domain.Models.Enums;
using Domain.Services.Abstractions;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using WebAPI.ViewModels;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/image")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class ImageController : ControllerBase
    {
        private const long MAX_CONTENT_SIZE = 5000000;

        private readonly ILogger<ImageController> logger;
        private readonly IImageService imageService;

        public ImageController(
            ILogger<ImageController> logger,
            IImageService imageService)
        {
            this.logger = logger;
            this.imageService = imageService;
        }

        [HttpGet("{locationType}/{uid}")]
        [AllowAnonymous]
        public IActionResult GetImage(string locationType, string uid, [FromQuery(Name = "w")] int? width, [FromQuery(Name = "h")] int? height)
        {
            if (!Enum.TryParse(locationType, out ImageLocationType imageLocationType))
            {
                logger.LogError("Invalid image location type '{0}'", locationType);
                return UnprocessableEntity();
            }

            Stream? imageStream = imageService.GetImageByUID(imageLocationType, uid, width, height);

            if (imageStream == null)
            {
                logger.LogError("Failed to get image for uid {0}", uid);
                return UnprocessableEntity();
            }

            var result = new FileStreamResult(imageStream, new MediaTypeHeaderValue("image/jpeg"));
            return result;
        }

        [HttpGet("{locationType}/uris")]
        public IActionResult GetImageURIsByType(string locationType)
        {
            if (!Enum.TryParse(locationType, out ImageLocationType imageLocationType))
            {
                logger.LogError("Invalid image location type '{0}'", locationType);
                return UnprocessableEntity();
            }

            var imageUIDs = this.imageService.GetAllImageUIDsByType(imageLocationType);

            var result = imageUIDs.Select(uid => new ImageDetails
            {
                UID = uid,
                GetURI = GetImageGetURI(Request, imageLocationType, uid),
                DeleteURI = $"{Request.Scheme}://{Request.Host}/communitme/api/image/{locationType}/{uid}"
            });

            return Ok(result);
        }

        [HttpDelete("{locationType}/{uid}")]
        public IActionResult DeleteImage(string locationType, string uid)
        {
            if (!Enum.TryParse(locationType, out ImageLocationType imageLocationType))
            {
                logger.LogError("Invalid image location type '{0}'", locationType);
                return UnprocessableEntity();
            }

            this.imageService.DeleteImage(imageLocationType, uid);

            return Ok();
        }

        [HttpPost("{locationType}")]
        public async Task<IActionResult> UploadImage(string locationType)
        {
            if (!Enum.TryParse(locationType, out ImageLocationType imageLocationType))
            {
                logger.LogError("Invalid image location type '{0}'", locationType);
                return UnprocessableEntity();
            }

            if (Request.ContentLength > MAX_CONTENT_SIZE)
            {
                logger.LogError("Request content size '{0}' is larger than maximum", locationType);
                return UnprocessableEntity();
            }

            string newFileUID;
            var existingFileUIDs = this.imageService.GetAllImageUIDsByType(imageLocationType);

            do
            {
                newFileUID = Guid.NewGuid().ToString();
            }
            while (existingFileUIDs.Contains(newFileUID));

            try
            {
                var formCollection = await Request.ReadFormAsync();
                var file = formCollection.Files.FirstOrDefault();

                if (file == null)
                {
                    logger.LogError("Failed to get uploaded file");
                    return UnprocessableEntity();
                }

                var fileStream = file.OpenReadStream();
                this.imageService.SaveImage(imageLocationType, newFileUID, fileStream);
                fileStream.Close();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to upload image to location type '{0}'", locationType);
                return UnprocessableEntity();
            }

            return Ok(new ImageUploadResponse
            {
                UID = newFileUID
            });
        }

        public static string GetImageGetURI(HttpRequest request, ImageLocationType imageLocationType, object uid)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string imageLocationTypeString = imageLocationType.ToString();

            if (uid == null)
            {
                throw new ArgumentNullException(nameof(uid), "Image unique identifier cannot be null");
            }

            string? uidString = uid.ToString();

            if (string.IsNullOrEmpty(imageLocationTypeString))
                throw new ArgumentException("Invalid image location type", nameof(imageLocationType));

            if (string.IsNullOrEmpty(imageLocationTypeString))
                throw new ArgumentException("Invalid image unique identifier", nameof(uidString));

            return $"{request.Scheme}://{request.Host}/communitme/api/image/{imageLocationTypeString}/{uidString}";
        }
    }
}
