using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using TrackerBackend; // Stelle sicher, dass du den Namespace für deine Klassen importierst

namespace TrackerBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StartedTrainingController : Controller
    {
        private readonly IConfiguration _configuration;

        public StartedTrainingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("StartedTraining/{userId}/{trainingPlanId}")]
        public IActionResult GetStartedTrainings(int userId, int trainingPlanId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            var trainings = new List<StartedTraining>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // SQL-Abfrage zum Abrufen der Startedexcercisesets 
                using (var cmd = new NpgsqlCommand(@"SELECT startedtrainingid ,userid, trainingplanid FROM startedtraining WHERE userid = @UserId AND trainingplanid = @TrainingPlanId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@TrainingPlanId", trainingPlanId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var training = new StartedTraining
                            {
                                startedTrainingId = reader.GetInt16(0),
                                userId= reader.GetInt16(1),
                                trainingPlanId=reader.GetInt16(2),
                                
                           

                            };
                            trainings.Add(training);
                        }
                    }
                }
            }

            if (trainings.Count == 0)
            {
                return NotFound(new { Message = "Keine Startedexcercisesets für den angegebenen Benutzer und Trainingsplan gefunden." });
            }
            foreach (var training in trainings)
            {
                var excercises = GetStartedExcercisesFromDatabase(userId, training.startedTrainingId);
                training.SetExcercises(excercises);
                training.SetTime(workoutStartTime(training.startedTrainingId, userId));
                training.setName(GetWorkoutName(trainingPlanId,userId));
            }
            return Ok(trainings);
        }


        DateTime workoutStartTime(int startedtrainingId, int userid)
        {
            var time = new DateTime();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // SQL-Abfrage zum Abrufen der Startedexcercisesets 
                using (var cmd = new NpgsqlCommand("SELECT excercisetime FROM startedexcerciseset WHERE @userid = userid AND @startedtrainingid = startedtrainingid LIMIT 1;", conn))
                {
                    cmd.Parameters.AddWithValue("@userid",userid );
                    cmd.Parameters.AddWithValue("@startedtrainingid", startedtrainingId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            time = reader.GetDateTime(0);
                            
                        }
                    }
                }
            }
            return time;
        }
        string GetWorkoutName(int trainingplanid, int userid)
        {
            string name = "";
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // SQL-Abfrage zum Abrufen der Startedexcercisesets 
                using (var cmd = new NpgsqlCommand("SELECT trainingplanname FROM usertrainingplan WHERE @userid = userid AND @trainingplanid = trainingplanid;", conn))
                {
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@trainingplanid", trainingplanid);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            name = reader.GetString(0);

                        }
                    }
                }
            }
            return name;
        }

        // Beispielfunktion zum Abrufen von Daten (ersetze dies durch deine tatsächliche Implementierung)
        List<StartedExcercise> GetStartedExcercisesFromDatabase(int userId, int startedTrainingID)
        {   
            List <StartedExcercise> excercises = new List<StartedExcercise>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // SQL-Abfrage zum Abrufen der Startedexcercisesets 
                using (var cmd = new NpgsqlCommand(@"SELECT DISTINCT ON(se.excerciseid)
                                        se.excerciseid,
                                        ue.excercisename
                                    FROM
                                        startedexcerciseset se
                                    JOIN
                                        userexcercise ue ON se.excerciseid = ue.excerciseid
                                    WHERE
                                        se.startedtrainingid = @startedTrainingId AND se.userid = @userId 
                                    ORDER BY
                                        se.excerciseid,
                                        se.excercisetime;", conn))
                {
                    cmd.Parameters.AddWithValue("@startedTrainingId", startedTrainingID);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var excercise = new StartedExcercise
                            {   
                                
                                excerciseid = reader.GetInt16(0),
                                excercisename = reader.GetString(1),
                                
                                
                            };
                            excercises.Add(excercise);
                        }
                    }
                }
            }
            foreach(var excercise in excercises)
            {
                var excercisesets = GetStartedSets(excercise.excerciseid, startedTrainingID);
                excercise.SetExerciseSets(excercisesets);
            }
            return excercises;
        }

         List<StartedSet> GetStartedSets(int excerciseid, int startedtrainingplanid)
        {
            List<StartedSet> sets = new List<StartedSet>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // SQL-Abfrage zum Abrufen der Startedexcercisesets 
                using (var cmd = new NpgsqlCommand("SELECT startedexcerciseid, weight, reps FROM startedexcerciseset WHERE @excerciseid = excerciseid AND startedtrainingid = @startedtraingplanid", conn))
                {
                    cmd.Parameters.AddWithValue("@excerciseid", excerciseid);
                    cmd.Parameters.AddWithValue("@startedtraingplanid", startedtrainingplanid);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {   
                            var set = new StartedSet
                            {   
                                startedexcercisesetid = reader.GetInt16(0),
                                
                                weight = reader.GetDouble(1),
                                reps = reader.GetInt32(2),
                            };
                            sets.Add(set);
                        }
                    }
                }
            }
            return sets;
        }
    }
}