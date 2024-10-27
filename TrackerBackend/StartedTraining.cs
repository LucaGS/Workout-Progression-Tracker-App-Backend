namespace TrackerBackend
{
    public class StartedTraining
    {
        public string? trainingPlaName { get; set; }
        public int  startedTrainingId { get; set; }
        public int userId { get; set; }
        public int trainingPlanId { get; set; }
        public DateTime? excercisetime { get; set; }
        public List<StartedExcercise>? excercises { get; set; }
        public void SetExcercises(List<StartedExcercise> excercises)

        {
            this.excercises = excercises;
        }
        public void SetTime (DateTime etime)
        {
            this.excercisetime = etime;
        }

        public void setName(string name) { this.trainingPlaName = name; }
    }
}
