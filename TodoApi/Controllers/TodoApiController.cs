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
    [Route("[controller]/activities")]
    public class TodoApiController : ControllerBase
    {
        private readonly ILogger<TodoApiController> _logger;

        public TodoApiController(ILogger<TodoApiController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get([FromHeader] string Authorization)
        {
            // find token
            try {
                string[] authorization = Authorization.Split(' ');
                string token = authorization[1];
                if (token == null) return StatusCode(401, new {message = "Invalid Token"});
            } catch {
                return StatusCode(401, new {message = "Invalid Token"});
            }

            var db = new AMCDbContext();
            var todoLists = db.Activities.Select(s => s);
            return Ok(todoLists);
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(uint id, [FromHeader] string Authorization)
        {
            // find token
            try {
                string[] authorization = Authorization.Split(' ');
                string token = authorization[1];
                if (token == null) return StatusCode(401, new {message = "Invalid Token"});
            } catch {
                return StatusCode(401, new {message = "Invalid Token"});
            }

            var db = new AMCDbContext();
            var todoLists = db.Activities.Where(s => s.Id == id).Select(s => s);

            if (!todoLists.Any()) return NotFound();
            
            return Ok(todoLists);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Activity todo, [FromHeader] string Authorization)
        {
            // find token
            try {
                string[] authorization = Authorization.Split(' ');
                string token = authorization[1];
                if (token == null) return StatusCode(401, new {message = "Invalid Token"});
            } catch {
                return StatusCode(401, new {message = "Invalid Token"});
            }

            try {
                var db = new AMCDbContext();
                db.Activities.Add(todo);
                db.SaveChanges();
            } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
            }

            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        public IActionResult Put([FromBody] Activity todo, [FromHeader] string Authorization, uint id)
        {
            // find token
            try {
                string[] authorization = Authorization.Split(' ');
                string token = authorization[1];
                if (token == null) return StatusCode(401, new {message = "Invalid Token"});
            } catch {
                return StatusCode(401, new {message = "Invalid Token"});
            }

            try {
                var db = new AMCDbContext();
                var todoLists = db.Activities.Where(s => s.Id == id).Select(s => s);
                if (!todoLists.Any()) return NotFound();
                var td = todoLists.First();
                td.Id = id;
                td.Name = todo.Name;
                td.When = todo.When;
                db.SaveChanges();
            } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
            }

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public IActionResult Delete([FromHeader] string Authorization, uint id)
        {
            // find token
            try {
                string[] authorization = Authorization.Split(' ');
                string token = authorization[1];
                if (token == null) return StatusCode(401, new {message = "Invalid Token"});
            } catch {
                return StatusCode(401, new {message = "Invalid Token"});
            }

            try {
                var db = new AMCDbContext();
                var todoList = db.Activities.Find(id);
                db.Activities.Remove(todoList);
                db.SaveChanges();
            } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
            }

            return Ok();
        }


    }
}
