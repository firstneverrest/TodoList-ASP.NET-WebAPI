using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TodoApi.Models;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoApiController : ControllerBase
    {
        private readonly ILogger<TodoApiController> _logger;

        public TodoApiController(ILogger<TodoApiController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var db = new AMCDbContext();
            var todoLists = db.Activities.Select(s => s);
            return Ok(todoLists);
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(uint id)
        {
            var db = new AMCDbContext();
            var todoLists = db.Activities.Where(s => s.Id == id).Select(s => s);

            if (!todoLists.Any()) return NotFound();
            
            return Ok(todoLists);
        }
    }
}
