![image](https://user-images.githubusercontent.com/65511890/155548059-d8aed5b1-b576-4c1b-8370-8d0eb6b9141a.png)

# Auth

**Authentication for PHP, but actually for C#.**

Port of PHP-Auth for C#.

Currently only supports Postgresql, the Shim.PDO wrapper should be easy enough to replicate for other databases.

Uses Npgsql for the Postgresql connection.

## Simple usage

```
// Establish database connection
var PDO = new Delight.Shim.PDO("Host=<server IP here>;Database=<database name here>",
          "<my_postgresql_username>",
          "<my_postgresql_password>");

// Create an Auth-friendly wrapper object
var DB = new Delight.Db.PdoDatabase(PDO);
// Create an Auth object
var Auth = new Auth(DB);
      
// If you want to set any cookies for the Auth session, use the Auth._COOKIE object.

try
{
  // Auth.login is the same as the Auth::login method in PHP-Auth
  // If it fails, it will throw an Exception
  Auth.login(username, password);
  
  Console.WriteLine("Logged in successfully!");
        
  // If you want to access any cookies that may have been set by Auth, use the Auth._COOKIE object.
}
catch (Delight.Auth.InvalidPasswordException)
{
  Console.WriteLine("Bad username/password");
}
catch (Exception exc)
{
  Console.WriteLine("Error: " + exc.Message);
}

PDO.Disconnect();
```

## License

This project is licensed under the terms of the [MIT License](https://opensource.org/licenses/MIT).
