namespace TrackerBackend
{
    public class WorkoutExcercisesAvg
    {
        public string? excerciseName {  get; set; }
        public int excerciseID {  get; set; }
        public double? avg { get; set; }
        public DateTime? date { get; set; }    
        public List<Startedexcerciseset> sets { get; set; }

        public void CalculateAvg()
        {
            
            List<double> effectivePowers = new List<double>();
            foreach (Startedexcerciseset set in sets)
            {
                double effectivePower = set.reps * set.weight;
                effectivePowers.Add(effectivePower);
            }
            this.avg = effectivePowers.Average();
        }
    }
    
}
