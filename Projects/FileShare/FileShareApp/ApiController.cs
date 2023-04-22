using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace FileShareApp
{
    public class ApiController : WebApiController
    {
        public ApiController() : base()
        { }

        [Route(HttpVerbs.Get, "/test")]
        public int GetTestResponse()
        {



            return 12345;
        }
    }
}

