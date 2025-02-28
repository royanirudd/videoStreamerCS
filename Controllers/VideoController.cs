using Microsoft.AspNetCore.Mvc;
using VideoStreamer.Utils;

namespace VideoStreamer.Controllers
{
    [ApiController]
    [Route("api")]
    public class VideoController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public VideoController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet("videos")]
        public IActionResult GetAvailableVideos()
        {
            var videoPath = Path.Combine(_environment.WebRootPath, "videos");
            var videos = Directory.GetFiles(videoPath, "*.mp4")
                                .Select(Path.GetFileName)
                                .ToList();
            return Ok(videos);
        }

        [HttpGet("stream/{filename}")]
        public IActionResult StreamVideo(string filename)
        {
            try
            {
                VideoUtilities.ValidateVideoFile(filename);
                var videoPath = Path.Combine(_environment.WebRootPath, "videos", filename);
                var fileInfo = new FileInfo(videoPath);
                var fileLength = fileInfo.Length;

                var rangeHeader = Request.Headers["Range"].ToString();

                if (!string.IsNullOrEmpty(rangeHeader))
                {
                    var parts = rangeHeader.Replace("bytes=", "").Split('-');
                    var start = long.Parse(parts[0]);
                    var end = parts.Length > 1 && !string.IsNullOrEmpty(parts[1])
                        ? long.Parse(parts[1])
                        : fileLength - 1;

                    var chunkSize = end - start + 1;

                    Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileLength}");
                    Response.Headers.Add("Accept-Ranges", "bytes");
                    Response.ContentType = "video/mp4";
                    Response.StatusCode = 206;

                    return File(new FileStream(videoPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                        "video/mp4",
                        enableRangeProcessing: true);
                }

                Response.Headers.Add("Content-Length", fileLength.ToString());
                Response.ContentType = "video/mp4";
                return File(new FileStream(videoPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                    "video/mp4",
                    enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                return ex switch
                {
                    ArgumentException => BadRequest(new { error = ex.Message }),
                    FileNotFoundException => NotFound(new { error = ex.Message }),
                    _ => StatusCode(500, new { error = "An unexpected error occurred" })
                };
            }
        }
    }
}
