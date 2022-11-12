# Dapper-Layers-Generator

*Bulk insert with MySqlBulkCopy is ready in the main branch (use it with batching send 500/1000/1500 rows). Will make a release when bulkupdate is ready too..*

Generate all the ez and boring DAL accesses you need at the begining with dapper and you will be able to focus on the complex parts of your app!
At the end, you will keep the control and can extend the code.

Read DB definitions from:

- [x] MySql / MariaDB
- [ ] MsSql (will be next)
- [ ] PostGreSQL
- [ ] Oracle

Generated C# code based on your db definitions:

- [x] Poco / Entities for all tables
- [x] DbContext / DbContext factory (simple)
- [x] DAL base implementation (multi-dbs)
- [x] DAL MySql / Maria DB specifics
- [ ] DAL MsSql DB specifics (will be next)
- [ ] DAL PostGreSQL specifics
- [ ] DAL Oracle specifics

Already implemented DAL methods in the generated code:

- [x] GetAllAsync()
- [x] GetByPkAsync(), composite PK ok.
- [x] GetByListOfPKAsync(), get by list of composite PKs (not done for the moment, need tmp table)
- [x] GetByUniqueIndexAsync()
- [x] AddAsync()
- [x] AddMultiAsync() loop
- [x] AddBulkAsync(), with MySqlBulkCopy implementation
- [x] UpdateAsync()
- [ ] UpdateMultiAsync() loop (next)
- [ ] UpdateBulkAsync(), with MySqlBulkCopy implementation and temp table (next)
- [x] DeleteAsync()
- [ ] DeletebyPkListAsync() (after update)
- [ ] DeleteBulkAsync() (after update)

## Config and first simple start

Set the mandatory configs in appsettings or environnement or usersecrets

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=information_schema;Uid=root;Pwd=root;"
  },

  "DB": {
    "Schemas": "testschema",
    "Provider": "MySql"
  }
}
```

- ConnectionStrings:Default => a MySql/Mariadb connection string with read right on information_schema
- DB:Schemas => the schemas/db name you want to read the definitions from to be able to generate your layers code
- DB:Provider => only MySql supported now.

## Run the app

![MainMenu](doc/img/main.jpg)

===> Go on "Main settings"

![MainSettings](doc/img/main-settings.JPG)

Set at least the following configs

- (3) Author name: your name
- (4) Target project namespace: the main namespace for your generated code
- (8) Target project path: physical path (where you want to generate) => don't forget the last backslash "\\"

Check all the path specified exist on your file system. The generator will not create the folder structure.

Press (q) to go back to the main menu.

## On the main menu, choose "Save"

![Save](doc/img/save.jpg)

Complete path/filename.json where you want to save your config.

## Generate

![Generate](doc/img/generate.jpg)

After your first code generation, return, load your config file and test all the available settings in (General settings / adavanced settings). You can go deeper as column config. I let you discover.

## Generated code usage

In your net6 app/asp.net, register the dbcontext :

```cs
IServiceCollection _services = new ServiceCollection()
    .AddSingleton(_config)
    .AddTransient<IDbContext, DbContextMySql>();
```

and call your new generated dal in your controller/service or app :

```cs
var clients = await dal.ClientRepo.GetAllAsync();
```

with transaction :

```cs
using var trans = await dal.OpenTransactionAsync();
int newClientId = await dal.ClientRepo.AddAsync(new Client() { Firstname = "John", Lastname = "Smith", City = "Paris" });
int newFailureId = await dal.FailureRepo.AddAsync(new Failure() { Description="Fail to pass the door", ClientId = newClientId });
dal.CommitTransaction();
```

## If you want to extent the generated code, create a new file and declare a partial class with the same name as the class you want to extent !

Open an issue if you have specific questions or if you detect an issue !

And love for

https://github.com/DapperLib/Dapper

https://github.com/spectreconsole/spectre.console

https://github.com/Humanizr/Humanizer

