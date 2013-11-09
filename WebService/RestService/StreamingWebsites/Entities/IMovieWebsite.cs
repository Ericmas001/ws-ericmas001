using RestService.StreamingWebsites.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestService.StreamingWebsites.Entities
{
    public interface IMovieWebsite
    {
        Task<IEnumerable<ListedMovie>> SearchAsync(string keywords);

        Task<IEnumerable<ListedMovie>> StartsWithAsync(string letter);

        Task<Movie> MovieAsync(string movieId);

        Task<StreamingInfo> StreamAsync(string website, string args);

        string MovieURL(string movieId);
    }
}