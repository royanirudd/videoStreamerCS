namespace VideoStreamer.Utils
{
    public static class VideoUtilities
    {
        private static readonly string[] AllowedExtensions = { ".mp4", ".webm", ".mov" };
        private static readonly string VideoDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos");

        public static string GetVideoPath(string filename)
        {
            var sanitizedFilename = Path.GetFileName(filename);
            return Path.Combine(VideoDirectory, sanitizedFilename);
        }

        public static bool ValidateVideoFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("Filename cannot be empty");

            var extension = Path.GetExtension(filename).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException($"Invalid video format. Allowed formats: {string.Join(", ", AllowedExtensions)}");

            var fullPath = GetVideoPath(filename);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Video file not found: {filename}");

            return true;
        }
    }
}
