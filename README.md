# Todo List Web API

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
