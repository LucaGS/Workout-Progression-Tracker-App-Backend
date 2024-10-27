using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Collections.Generic;
using TrackerBackend;

namespace TrackerBackend
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserExcerciseController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserExcerciseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet("startedexercisesets/{userId}/{trainingPlanId}")]
        public IActionResult GetStartedexcercisesets(int userId, int trainingPlanId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            var sets = new List<Startedexcerciseset>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // SQL-Abfrage zum Abrufen der Startedexcercisesets 
                using (var cmd = new NpgsqlCommand("SELECT * FROM Startedexcerciseset WHERE userid = @UserId AND trainingplanid = @TrainingPlanId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@TrainingPlanId", trainingPlanId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var set = new Startedexcerciseset
                            {
                                startedexcerciseid = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                                startedtrainingid = reader.GetInt32(1),
                                userid = reader.GetInt32(2),
                                trainingplanid = reader.GetInt32(3),
                                excercisetime = reader.GetDateTime(4),
                                set = reader.GetInt32(5),
                                weight = reader.GetDouble(6),
                                excerciseid = reader.GetInt32(7),
                                reps = reader.GetInt16(8)
                            };
                            sets.Add(set);
                        }
                    }
                }
            }

            if (sets.Count == 0)
            {
                return NotFound(new { Message = "Keine Startedexcercisesets für den angegebenen Benutzer und Trainingsplan gefunden." });
            }

            return Ok(sets);
        }
        // Add a new exercise to the Userexcercise table (exercise ID auto-generated)
        [HttpPost]
        public IActionResult AddUserExcercise([FromBody] UserExcercise newExcercise)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Insert the new exercise into the Userexcercise table without specifying excerciseid
                using (var cmd = new NpgsqlCommand("INSERT INTO Userexcercise (userid, excercisename, trainingplanid, excercisesets) VALUES (@UserId, @ExcerciseName, @TrainingPlanid, @Excercisesets) RETURNING excerciseid", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", newExcercise.userid);
                    cmd.Parameters.AddWithValue("@ExcerciseName", newExcercise.excercisename);
                    cmd.Parameters.AddWithValue("@TrainingPlanid", newExcercise.trainingplanid);
                    cmd.Parameters.AddWithValue("@Excercisesets", newExcercise.excercisesets);
                    // Retrieve the auto-generated exercise ID
                    int generatedExcerciseId = (int)cmd.ExecuteScalar();

                    return Ok(new { Message = "Exercise added successfully!", ExcerciseId = generatedExcerciseId });
                }
            }
        }

        // Get all exercises by user ID and training plan ID
        [HttpGet("{userId}/{trainingPlanId}")]
        public IActionResult GetExercisesByUserAndTrainingPlan(int userId, int trainingPlanId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            var exercises = new List<UserExcercise>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Query to get all exercises for the specified user ID and training plan ID
                using (var cmd = new NpgsqlCommand("SELECT excerciseid, userid, excercisename, trainingplanid, excercisesets FROM Userexcercise WHERE userid = @UserId AND trainingplanid = @TrainingPlanId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@TrainingPlanId", trainingPlanId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var exercise = new UserExcercise
                            {
                                excerciseid = reader.GetInt32(0),
                                userid = reader.GetInt32(1),
                                excercisename = reader.GetString(2),
                                trainingplanid = reader.GetInt32(3),
                                excercisesets = reader.GetInt32(4)
                            };
                            exercises.Add(exercise);
                        }
                    }
                }
            }

            if (exercises.Count == 0)
            {
                return NotFound(new { Message = "No exercises found for the specified user and training plan." });
            }

            return Ok(exercises);
        }

        // Delete an exercise by userId, exerciseId, and trainingPlanId
        [HttpDelete("{userId}/{excerciseId}/{trainingPlanId}")]
        public IActionResult DeleteUserExcercise(int userId, int excerciseId, int trainingPlanId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Query to delete the specified exercise
                using (var cmd = new NpgsqlCommand("DELETE FROM Userexcercise WHERE userid = @UserId AND excerciseid = @ExcerciseId AND trainingplanid = @TrainingPlanId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@ExcerciseId", excerciseId);
                    cmd.Parameters.AddWithValue("@TrainingPlanId", trainingPlanId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new { Message = "Exercise not found or already deleted." });
                    }

                    return Ok(new { Message = "Exercise deleted successfully." });
                }
            }
        }
    }
}
