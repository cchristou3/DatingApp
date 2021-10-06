using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using NLog;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private static ILogger _logger = NLog.LogManager.LoadConfiguration("NLog.config").GetCurrentClassLogger();

        private readonly DataContext _dataContext;
        public UsersController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        // api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            _logger.Info("GET GetUsers");            
            return await _dataContext.Users.ToListAsync();
        }

        // api/users/1
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id)
        {
            _logger.Info("GET GetUser");
            return await _dataContext.Users.FindAsync(id);
        }
    }

}