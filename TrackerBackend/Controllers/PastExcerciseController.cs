using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Collections.Generic;

namespace TrackerBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PastExcerciseController : Controller
    {
        private readonly IConfiguration _configuration;

        public PastExcerciseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("ExcercisesAvg/{userId}/{excerciseId}")]
        public IActionResult GetPastWorkoutExcercisesAvg(int userId, int excerciseId)
        {
            var ExcerciseSets = new List<Startedexcerciseset>();
            var avgWorkoutSets = new List<WorkoutExcercisesAvg>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT * FROM startedexcerciseset WHERE excerciseid = @excerciseId AND userid = @userId", conn))
                {
                    cmd.Parameters.AddWithValue("@excerciseId", excerciseId);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ExcerciseSets.Add(new Startedexcerciseset
                            {
                                startedexcerciseid = reader.GetInt16(0),
                                startedtrainingid = reader.GetInt16(1),
                                userid = reader.GetInt16(2),
                                trainingplanid = reader.GetInt16(3),
                                excercisetime = reader.GetDateTime(4),
                                set = reader.GetInt16(5),
                                weight = reader.GetDouble(6),
                                excerciseid = reader.GetInt32(7),
                                reps = reader.GetInt32(8),
                            });
                        }
                    }
                }
            }

            int avgIndex = -1;
            int currentstartedtrainingid = -1;

            foreach (Startedexcerciseset set in ExcerciseSets)
            {
                if (set.startedtrainingid != currentstartedtrainingid)
                {
                    currentstartedtrainingid = set.startedtrainingid;
                    avgWorkoutSets.Add(new WorkoutExcercisesAvg
                    {
                        excerciseID = set.excerciseid,
                        date = set.excercisetime,
                        sets = new List<Startedexcerciseset> { set }
                    });
                    avgIndex = avgWorkoutSets.Count - 1;
                }
                else
                {
                    avgWorkoutSets[avgIndex].sets.Add(set);
                }
            }

            foreach (WorkoutExcercisesAvg avgWorkoutSet in avgWorkoutSets)
            {
                avgWorkoutSet.CalculateAvg();
            }

            return Ok(avgWorkoutSets);
        }
    }
}
