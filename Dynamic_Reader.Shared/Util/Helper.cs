using System.Linq;

namespace Dynamic_Reader.Util
{
    public static class Helper
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
        private static readonly string[] CoverIds =
        {
            "cover", "cover-image", "cover1", "cover.jpg",
            "cover.jpeg", "coverimagestandard", "coverpic", "coverimage"
        };
        public static bool IsImage(string fileName)
        {
            return ImageExtensions.Any(item => fileName.ToLower().EndsWith(item));
        }

        public static bool IsCover(string id)
        {
            return CoverIds.Any(item => id.ToLower() == item);
        }
    }
}
