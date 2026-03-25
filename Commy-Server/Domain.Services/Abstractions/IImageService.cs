using Domain.Models.Enums;
using Domain.Services.Models;

namespace Domain.Services.Abstractions
{
	public interface IImageService
	{
		void DeleteImage(ImageLocationType imageType, string uid);
		List<string> GetAllImageUIDsByType(ImageLocationType imageType);
		Stream? GetImageByUID(ImageLocationType profile, string uid, int? width, int? height);
		Stream GenerateCircle(string firstName, string lastName, ProfileCircleProperties circleProperties);
        void SaveImage(ImageLocationType imageType, string uid, Stream imageStream);
	}
}
