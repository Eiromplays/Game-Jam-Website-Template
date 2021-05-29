using System.Text.RegularExpressions;

namespace GameJam.Api.Extensions
{
    public class VideoEmbedExtension
    {
        //http://stackoverflow.com/questions/3652046/c-sharp-regex-to-get-video-id-from-youtube-and-vimeo-by-url
        static readonly Regex YoutubeVideoRegex =
            new Regex(@"youtu(?:\.be|be\.com)/(?:(.*)v(/|=)|(.*/)?)([a-zA-Z0-9-_]+)", RegexOptions.IgnoreCase);

        static readonly Regex VimeoVideoRegex = new Regex(@"vimeo\.com/(?:.*#|.*/videos/)?([0-9]+)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // Use as
        // string youtubeLink = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        // var embedCode = youtubeLink.UrlToEmbedCode();
        public static string UrlToEmbedCode(string url, bool format = false)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var youtubeMatch = YoutubeVideoRegex.Match(url);

            if (youtubeMatch.Success)
            {
                return format
                    ? GetYoutubeEmbedCode(youtubeMatch.Groups[youtubeMatch.Groups.Count - 1].Value)
                    : $"https://www.youtube-nocookie.com/embed/{youtubeMatch.Groups[youtubeMatch.Groups.Count - 1].Value}";
            }

            var vimeoMatch = VimeoVideoRegex.Match(url);
            if (vimeoMatch.Success)
            {
                return format
                    ? GetVimeoEmbedCode(vimeoMatch.Groups[1].Value)
                    : $"https://player.vimeo.com/video/{vimeoMatch.Groups[1].Value}";
            }

            // If it doesn't match any of the above it will return the url.
            return url;
        }

        const string YoutubeEmbedFormat = "<iframe type=\"text/html\" class=\"embed-responsive-item\" src=\"https://www.youtube-nocookie.com/embed/{0}\"></iframe>";

        private static string GetYoutubeEmbedCode(string youtubeId)
        {
            return string.Format(YoutubeEmbedFormat, youtubeId);
        }

        const string VimeoEmbedFormat = "<iframe src=\"https://player.vimeo.com/video/{0}\" class=\"embed-responsive-item\" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>";
        private static string GetVimeoEmbedCode(string vimeoId)
        {
            return string.Format(VimeoEmbedFormat, vimeoId);
        }
    }
}
