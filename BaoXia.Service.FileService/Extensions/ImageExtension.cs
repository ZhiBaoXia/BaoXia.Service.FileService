namespace BaoXia.Service.FileService.Extensions
{
    public static class ImageExtension
    {
        /*
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>")]
        public static MemoryStream SaveToStream(
               this Image image,
               ImageFormat imageFormat)
        {
                var memoryStream = new MemoryStream();
                ////////////////////////////////////////////////
                image.Save(memoryStream, imageFormat);
                memoryStream.Position = 0;
                // memoryStream.Close();
                ////////////////////////////////////////////////
                return memoryStream;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification = "<挂起>")]
        public static byte[] SaveToBytes(
                this Image image,
                ImageFormat imageFormat)
        {
                byte[]? imageBytes;
                using var memoryStream = new MemoryStream();
                ////////////////////////////////////////////////
                image.Save(memoryStream, imageFormat);
                // !!!
                imageBytes = new byte[memoryStream.Length];
                memoryStream.Position = 0;
                memoryStream.Read(imageBytes, 0, imageBytes.Length);
                // !!!
                memoryStream.Close();
                ////////////////////////////////////////////////
                return imageBytes;
        }
        */
    }
}
