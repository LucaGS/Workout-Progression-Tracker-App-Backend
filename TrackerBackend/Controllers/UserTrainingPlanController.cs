using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using TrackerBackend;

namespace TrackerBackend
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserTrainingPlanController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UserTrainingPlanController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Add a new exercise to the Userexcercise table (exercise ID auto-generated)
        [HttpPost]
        public IActionResult AddUserExcercise([FromBody] UserTrainingPlan newUserTrainingPlan)
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
                using (var cmd = new NpgsqlCommand("INSERT INTO Usertrainingplan (userid,trainingplanname) VALUES (@UserId, @TrainingPlanname) RETURNING trainingplanid", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", newUserTrainingPlan.UserId);
                    cmd.Parameters.AddWithValue("@TrainingPlanname", newUserTrainingPlan.TrainingPlanName);

                    // Retrieve the auto-generated exercise ID
                    int generatedtrainingplanid = (int)cmd.ExecuteScalar();

                    return Ok(new { Message = "Exercise added successfully!", trainingplanid = generatedtrainingplanid });
                }
            }
        }

        [HttpGet("{userId}")]
        public IActionResult GetTrainingPlans(int userId)
        {
  
            var trainingPlans = new List<UserTrainingPlan>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand($"SELECT trainingplanid, trainingplanname FROM usertrainingplan WHERE userid = {userId}", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            trainingPlans.Add(new UserTrainingPlan
                            {
                                TrainingPlanId = reader.GetInt32(0),
                                TrainingPlanName = reader.GetString(1)
                                
                                
                            });
                        }
                    }
                }
            }

            return Ok(trainingPlans);
        }
        // Delete a training plan by its ID and user ID
        [HttpDelete("{userId}/{trainingPlanId}")]
        public IActionResult DeleteTrainingPlan(int userId, int trainingPlanId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Delete the training plan from the Usertrainingplan table
                using (var cmd = new NpgsqlCommand("DELETE FROM usertrainingplan WHERE userid = @UserId AND trainingplanid = @TrainingPlanId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@TrainingPlanId", trainingPlanId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Ok(new { Message = "Training plan deleted successfully!" });
                    }
                    else
                    {
                        return NotFound(new { Message = "Training plan not found." });
                    }
                }
            }
        }


    }
}
