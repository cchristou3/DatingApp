using NLog;
using Microsoft.AspNetCore.Mvc;
using API.Helpers;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(LogUserActivity))]
    public class BaseApiController : ControllerBase
    {
        protected static ILogger _logger = NLog.LogManager.LoadConfiguration("NLog.config").GetCurrentClassLogger();
    }
}