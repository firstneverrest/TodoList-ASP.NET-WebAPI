# Todo List Web API

Web API with ASP.NET Core created for todo list application. This Web API include GET, POST, PUT, DELETE, connects with MariaDB, store password with hash & salt, and authentication with JWT.

## Set up Database

Select MariaDB as a SQL database server. MariaDB is an open-source relational database management system(RDBMS) like MySQL.

1. Install [MariaDB](https://downloads.mariadb.org/)
2. Install [Xampp](https://www.apachefriends.org/download.html) to run database server on localhost for Web API development
3. Open Xampp, run Apache and mySQL. If the port error is occurred, you can change the port in Config section.
4. Create database (todolist) and two tables: user and activity

## Set up ASP.NET Web API

1. Install [.NET 5.0 SDK from official website](https://dotnet.microsoft.com/download).
2. run `dotnet --version` to check whether dotnet is already installed or not.
3. run these command to create web api project

```
dotnet new webapi -o myproject
cd myproject
```

4. Install entity framework to enable ASP.NET Web API project to connect to MariaDB.

```
dotnet tool update --global dotnet-ef

dotnet add package Microsoft.EntityFrameworkCore.Design

dotnet add package Pomelo.EntityFrameworkCore.MySql

dotnet ef dbcontext scaffold "server=localhost;port=3307;user=root;password=todolist;database=todolist" Pomelo.EntityFrameworkCore.MySql -c AMCDbContext -o Models
```

5. After scaffolding, the models folder is created depends on your database table list.

## Auto-increment on primary key (id) in Entity Framework

From: [EntityFramework Tutorial](https://www.entityframeworktutorial.net/code-first/databasegenerated-dataannotations-attribute.aspx)
Then, go to phpMyAdmin and check the id column to auto-increment.

## Hash and Salt

Hash and Salt is used to protect the password that store in the database. Generally, password is stored in plain text which is not secured at all. Therefore, salt and hash concept are life savior to secure the password with these steps:

1. Random salt in Byte[] type
2. Add password in plain text and random salt to the hash function
3. The result of hash function is hash in Base64
4. Store both hash as password and salt in the database instead of plain text.
5. When you want to verify, get password from user and salt to the hash function.
6. The result should be matched with the hash that kept in database.
7. If both hash from database and from hash function are the same, password is true.

```c#
// HashFunction.cs
using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace TodoApi.Utils
{
    public static class HashFunction
    {
         public static (string, string) CreateHashAndSalt(string password)
        {
            byte[] salt = new byte[128/8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256/8));


            (string salt, string hashed) results = (Convert.ToBase64String(salt), hashed);
            return results;
        }

        public static bool CheckPassword(string password, string salt, string hash)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256/8));

            if (hash != hashed) return false;

            return true;
        }
    }
}
```

## Install JWT for authentication

```
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

```c#
// JWTAuthentication.cs
using System;
using System.Text;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace TodoApi.Utils
{
    public static class JWTAuthentication
    {
        public static string GenerateJwtToken(string userid)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // var tokenKey = Encoding.ASCII.GetBytes(key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, userid)}),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddHours(3),
                IssuedAt = DateTime.UtcNow,
                Issuer = "chitsanupong",
                Audience = "public",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1234567812345678")), SecurityAlgorithms.HmacSha256Signature),
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("1234567812345678")),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = "chitsanupong",
                    ValidAudience = "public",
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userid = jwtToken.Claims.First(x => x.Type == "unique_name").Value;

                // return account id from JWT token if validation successful
                return userid;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }
    }
}
```

### Generate and Validate JWT Reference

- [ASP.NET Core Authentication with JWT (JSON Web Token)](https://www.youtube.com/watch?v=vWkPdurauaA)
- [ASP.NET Core 3.1 - Create and Validate JWT Tokens + Use Custom JWT Middleware](https://jasonwatmore.com/post/2020/07/21/aspnet-core-3-create-and-validate-jwt-tokens-use-custom-jwt-middleware)
- [Create And Validate JWT Token In .NET 5.0](https://www.c-sharpcorner.com/article/jwt-validation-and-authorization-in-net-5-0/)

## GET Request

```c#
// Get all todo list
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

// Get todo list depends on id
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
```

## Post Request

```c#
// Create a todo list
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
```

## PUT Request

```c#
// Update a todo list
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
```

## Delete Request

```c#
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
```

## Login and Get Token

```c#
[HttpPost]
[Route("tokens")]
public IActionResult Login([FromBody] Account account)
{
    if (account.userid == null || account.password == null) return BadRequest();

    try {
        var db = new AMCDbContext();
        var user = db.Users.Where(s => s.Id == account.userid).Select(s => s);
        if (!user.Any()) return Unauthorized();
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
```

## Sign up

```c#
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
```

## Cors Setting

```
dotnet add package Microsoft.AspNet.WebApi.Cors
```

```c#
// add variable
readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

public void ConfigureServices(IServiceCollection services)
{

    // add services.AddCors
    services.AddCors(options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins,
            builder =>
            {
                builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
            });
    });

    services.AddControllers();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "TodoApi", Version = "v1" });
    });
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoApi v1"));
    }

    app.UseHttpsRedirection();

    app.UseRouting();

    // Add app.UseCors
    app.UseCors(MyAllowSpecificOrigins);

    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}
```

### Reference

- [Enabling CORS in ASP.NET Web API](https://medium.com/easyread/enabling-cors-in-asp-net-web-api-4be930f97a5c)
- [Enable Cross-Origin Requests (CORS) in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-5.0#cors-policy-options-1)

## Problem Solving

- Message: "System.InvalidOperationException: Unable to track an instance of type 'Activity' because it does not have a primary key...".
  - Solve: In AMCDbContext.cs file, you need to remove `entity.HasNoKey();` because it causes data from Web API does not have a primary key.
