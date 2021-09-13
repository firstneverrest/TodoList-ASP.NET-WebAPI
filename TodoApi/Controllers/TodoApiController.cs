using System;
using System.Text;
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

        public TodoApiController(ILogger<TodoApiController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("activities")]
        public IActionResult Get([FromHeader] string Authorization)
        {
            // validate token
            try {
                string[] authorization = Authorization.Split(' ');
                string token = authorization[1];
                string userid = JWTAuthentication.ValidateJwtToken(token);
                if (userid == null) return StatusCode(401, new {message = "Invalid Token"});
           } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
            }

            var db = new AMCDbContext();
            var todoLists = db.Activities.Select(s => s);
            return Ok(todoLists);
        }

        [HttpGet]
        [Route("activities/{id}")]
        public IActionResult Get(uint id, [FromHeader] string Authorization)
        {
            // validate token
            try {
                string[] authorization = Authorization.Split(' ');
                string token = authorization[1];
                string userid = JWTAuthentication.ValidateJwtToken(token);
                if (userid == null) return StatusCode(401, new {message = "Invalid Token"});
            } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
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
            // validate token
            try {
                string[] authorization = Authorization.Split(' ');
                string token = authorization[1];
                string userid = JWTAuthentication.ValidateJwtToken(token);
                if (userid == null) return StatusCode(401, new {message = "Invalid Token"});
           } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
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
            // validate token
            try {
                string[] authorization = Authorization.Split(' ');
                string token = authorization[1];
                string userid = JWTAuthentication.ValidateJwtToken(token);
                if (userid == null) return StatusCode(401, new {message = "Invalid Token"});
           } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
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
            // validate token
            try {
                string[] authorization = Authorization.Split(' ');
                string token = authorization[1];
                string userid = JWTAuthentication.ValidateJwtToken(token);
                if (userid == null) return StatusCode(401, new {message = "Invalid Token"});
           } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
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
        public IActionResult GetToken([FromBody] Account account)
        {
            if (account.userid == null || account.password == null) return BadRequest();

            try {
                var db = new AMCDbContext();
                var user = db.Users.Where(s => s.Id == account.userid).Select(s => s);
                if (!user.Any()) return NotFound();
                var u = user.First();
                
                // check password with hash function
                bool isVerified = HashFunction.CheckPassword(account.password, u.Salt, u.Password);
                if (!isVerified) return Unauthorized();


                // send token if the username and password is true
                var token = JWTAuthentication.GenerateJwtToken(account.userid);
                return Ok(new { token = token });

            } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
            }

        }

        [HttpPost]
        [Route("signup")]
        public IActionResult SignUp([FromBody] Account account)
        {
            (string salt, string hash) hashedAndSalt = HashFunction.CreateHashAndSalt(account.password);
            string salt = hashedAndSalt.salt;
            string hash = hashedAndSalt.hash;  

            try {
                var db = new AMCDbContext();
                db.Users.Add(new User(){
                    Id = account.userid,
                    Password = hash,
                    Salt = salt,
                });
                db.SaveChanges();
            } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
            }

            return StatusCode(201);
        }

    }
}
