using Microsoft.AspNetCore.Mvc;
using Npgsql;
namespace TrackerBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserWorkoutController : Controller
    {
        private readonly IConfiguration _configuration;

        public UserWorkoutController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("AddStartedTraining")]
        public IActionResult AddStartedTraining([FromBody] StartedTraining newStartedTraining)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Insert the new entry into the startedtraining table and return the generated startedtrainingid
                using (var cmd = new NpgsqlCommand("INSERT INTO startedtraining (userid, trainingplanid) VALUES (@UserId, @TrainingPlanId) RETURNING startedtrainingid", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", newStartedTraining.userId);
                    cmd.Parameters.AddWithValue("@TrainingPlanId", newStartedTraining.trainingPlanId);

                    // Execute the command and retrieve the newly created startedTrainingId
                    newStartedTraining.startedTrainingId = (int)cmd.ExecuteScalar();
                }
            }

            // Return the newly created entry with the auto-generated startedTrainingId
            return CreatedAtAction(nameof(GetStartedTrainingById), new { id = newStartedTraining.startedTrainingId }, newStartedTraining);
        }

        [HttpGet("{id}")]
        public IActionResult GetStartedTrainingById(int id)
        {
            // You can add logic here to retrieve the started training by its ID
            return Ok();
        }
        [HttpPost("AddStartedExcerciseSet")]
        public IActionResult AddStartedExcerciseSet([FromBody] Startedexcerciseset newStartedexcerciseset)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Insert the new entry into the startedexcerciseset table and return the generated startedexcerciseid
                using (var cmd = new NpgsqlCommand("INSERT INTO startedexcerciseset (startedtrainingid, userid, trainingplanid, set, weight, excerciseid, reps) VALUES (@startedtrainingid, @userid, @trainingplanid, @set, @weight, @excerciseid, @reps) RETURNING startedexcerciseid", conn))
                {
                    cmd.Parameters.AddWithValue("@startedtrainingid", newStartedexcerciseset.startedtrainingid); // Corrected parameter name
                    cmd.Parameters.AddWithValue("@userid", newStartedexcerciseset.userid);
                    cmd.Parameters.AddWithValue("@trainingplanid", newStartedexcerciseset.trainingplanid);
                    cmd.Parameters.AddWithValue("@set", newStartedexcerciseset.set);
                    cmd.Parameters.AddWithValue("@weight", newStartedexcerciseset.weight);
                    cmd.Parameters.AddWithValue("@excerciseid", newStartedexcerciseset.excerciseid);
                    cmd.Parameters.AddWithValue("@reps", newStartedexcerciseset.reps);
                    // Execute the command and retrieve the newly created startedexcerciseid
                    newStartedexcerciseset.startedexcerciseid = (int)cmd.ExecuteScalar();
                }
            }

            // Return the newly created entry with the auto-generated startedexcerciseid
            return CreatedAtAction(nameof(GetStartedExcerciseSetById), new { id1 = newStartedexcerciseset.startedexcerciseid }, newStartedexcerciseset);
        }

        [HttpGet("{id1}")]
        public IActionResult GetStartedExcerciseSetById(int id)
        {
            // You can add logic here to retrieve the started exercise set by its ID
            return Ok();
        }

    }
}
