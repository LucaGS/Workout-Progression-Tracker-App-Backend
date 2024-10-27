using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using TrackerBackend;
using BCrypt.Net;

[ApiController]
[Route("api/[controller]")]
public class AppUserController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AppUserController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Get all users
    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = new List<AppUser>();
        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();

            using (var cmd = new NpgsqlCommand("SELECT * FROM AppUser", conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new AppUser
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Password = reader.GetString(2)
                        });
                    }
                }
            }
        }

        return Ok(users);
    }

    // Get a single user by userId
    [HttpGet("{userId}")]
    public IActionResult GetUserById(int userId)
    {
        AppUser user = null;
        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();

            using (var cmd = new NpgsqlCommand("SELECT * FROM AppUser WHERE UserId = @userId", conn))
            {
                cmd.Parameters.AddWithValue("@userId", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new AppUser
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Password = reader.GetString(2)
                        };
                    }
                }
            }
        }

        if (user == null)
        {
            return NotFound($"User with ID {userId} not found.");
        }

        return Ok(user);
    }

    // Add a new user (UserId is auto-incremented)
    [HttpPost("signup")]
    public IActionResult Signup([FromBody] AppUser newUser)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();

            // Check if the username already exists
            using (var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM AppUser WHERE username = @Username", conn))
            {
                checkCmd.Parameters.AddWithValue("@Username", newUser.Username);
                var count = (long)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    return Conflict("Username already exists.");
                }
            }

            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newUser.Password);

            // Insert the new user with the hashed password
            using (var cmd = new NpgsqlCommand("INSERT INTO AppUser (username, userpassword) VALUES (@Username, @Password) RETURNING userid", conn))
            {
                cmd.Parameters.AddWithValue("@Username", newUser.Username);
                cmd.Parameters.AddWithValue("@Password", hashedPassword);

                // Execute the command and retrieve the newly created UserId
                newUser.UserId = (int)cmd.ExecuteScalar();
            }
        }

        // Return the newly created user with the auto-generated UserId
        return CreatedAtAction(nameof(GetUserById), new { userId = newUser.UserId }, newUser);
    }

    // Login an existing user
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
        {
            return BadRequest("Username and password are required.");
        }

        AppUser user = null;
        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();

            // Retrieve user by username
            using (var cmd = new NpgsqlCommand("SELECT * FROM AppUser WHERE username = @Username", conn))
            {
                cmd.Parameters.AddWithValue("@Username", loginRequest.Username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new AppUser
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Password = reader.GetString(2)  // Hashed password from the database
                        };
                    }
                }
            }
        }

        // Verify the password
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
        {
            return Unauthorized("Invalid username or password.");
        }

        // Return the UserId in the response
        return Ok(new { Message = "Login successful.", UserId = user.UserId });
    }
}
