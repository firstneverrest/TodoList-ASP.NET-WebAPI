using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TodoApi.Models;
using TodoApi.Utils;
using TodoApi.DTOs;

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
            var activities = db.Activities.Select(s => s).OrderBy(a => a.When);

            if (!activities.Any()) return NoContent();
            return Ok(activities);
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
            var activity = db.Activities.Where(s => s.Id == id).Select(s => s);

            if (!activity.Any()) return NotFound();
            
            return Ok(activity);
        }

        [HttpPost]
        [Route("activities")]
        public IActionResult Post([FromBody] ActivityDTO Dto, [FromHeader] string Authorization)
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
                Activity activity = new Activity();
                activity.Name = Dto.Name;
                activity.When = Dto.When;
                db.Activities.Add(activity);
                db.SaveChanges();
            } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
            }

            return StatusCode(201);
        }

        [HttpPut]
        [Route("activities/{id}")]
        public IActionResult Put([FromBody] ActivityDTO Dto, [FromHeader] string Authorization, uint id)
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
                var activity = db.Activities.Where(s => s.Id == id).Select(s => s);
                if (!activity.Any()) return NotFound();
                var td = activity.First();
                td.Id = id;
                td.Name = Dto.Name;
                td.When = Dto.When;
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
                var activity = db.Activities.Find(id);
                db.Activities.Remove(activity);
                db.SaveChanges();
            } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
            }

            return Ok();
        }

        [HttpPost]
        [Route("tokens")]
        public IActionResult Login([FromBody] AccountDTO Dto)
        {
            if (Dto.userid == null || Dto.password == null) return BadRequest();

            try {
                var db = new AMCDbContext();
                var user = db.Users.Where(s => s.Id == Dto.userid).Select(s => s);
                if (!user.Any()) return Unauthorized();
                var u = user.First();
                
                // check password with hash function
                bool isVerified = HashFunction.CheckPassword(Dto.password, u.Salt, u.Password);
                if (!isVerified) return Unauthorized();


                // send token if the username and password is true
                var token = JWTAuthentication.GenerateJwtToken(Dto.userid);
                return StatusCode(201, new { token = token });

            } catch (Exception e) {
                return StatusCode(500, new {message = e.ToString()});
            }

        }

        [HttpPost]
        [Route("signup")]
        public IActionResult SignUp([FromBody] AccountDTO Dto)
        {
            (string salt, string hash) hashedAndSalt = HashFunction.CreateHashAndSalt(Dto.password);
            string salt = hashedAndSalt.salt;
            string hash = hashedAndSalt.hash;  

            try {
                var db = new AMCDbContext();
                db.Users.Add(new User(){
                    Id = Dto.userid,
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
