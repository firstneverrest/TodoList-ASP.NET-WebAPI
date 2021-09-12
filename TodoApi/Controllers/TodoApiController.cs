using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TodoApi.Models;
using TodoApi.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoApiController : ControllerBase
    {
        private readonly ILogger<TodoApiController> _logger;
        private readonly IJWTAuthentication jWTAuthentication;

        public TodoApiController(ILogger<TodoApiController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("activities")]
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
        [Route("activities/{id}")]
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
        [Route("activities")]
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

            return StatusCode(201);
        }

        [HttpPut]
        [Route("activities/{id}")]
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
                var todoList = db.Activities.Where(s => s.Id == id).Select(s => s);
                if (!todoList.Any()) return NotFound();
                var td = todoList.First();
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
        [Route("activities/{id}")]
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

        [HttpGet]
        [Route("tokens")]
        public IActionResult GetToken([FromBody] string userid, string password)
        {
            if (userid == null || password == null) return BadRequest();

            try {
                var db = new AMCDbContext();
                // var todoList = db.Users.Find(userid);
                var user = db.Users.Where(s => s.Id == userid).Select(s => s);
                if (!user.Any()) return NotFound();
                var u = user.First();
                
                // check password with hash function
                bool isVerified = HashFunction.CheckPassword(password, u.Salt, u.Password);
                if (!isVerified) return Unauthorized();

                // send token if the username and password is true
                var token = jWTAuthentication.GenerateJwtToken(userid);
                return Ok(new { token = token });

            } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
            }

        }

        [HttpPost]
        [Route("signup")]
        public IActionResult SignUp([FromBody] Account account)
        {
            string[] hashedAndSalt = HashFunction.CreateHashAndSalt(account.password);
            string salt = hashedAndSalt[0].ToString();
            string hash = hashedAndSalt[1]; 

            try {
                var db = new AMCDbContext();
                db.Users.Add(new User(){
                    Id = account.userid,
                    Password = hash,
                    Salt = salt
                });
                db.SaveChanges();
            } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
            }

            return StatusCode(201);
        }

    }
}
