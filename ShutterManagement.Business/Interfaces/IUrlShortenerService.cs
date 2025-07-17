using ShutterManagement.Core.ViewModels;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface IUrlShortenerService
    {
        /// <summary>
        /// Shorten a URL using Bitly API
        /// </summary>
        /// <param name="longUrl">The original URL to shorten</param>
        /// <returns>Shortened URL or original URL if shortening fails</returns>
        Task<IResponseVm<string>> ShortenUrlAsync(string longUrl);

        /// <summary>
        /// Get analytics for a shortened URL (for future enhancement)
        /// </summary>
        /// <param name="shortenedUrl">The shortened URL</param>
        /// <returns>Analytics data</returns>
        Task<IResponseVm<object>> GetUrlAnalyticsAsync(string shortenedUrl);
    }
} 