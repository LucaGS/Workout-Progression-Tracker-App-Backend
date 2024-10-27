namespace TrackerBackend
{
    public class StartedExcercise
    {
        public string? excercisename { get; set; }
        public int excerciseid { get; set; }
        public List<StartedSet>? excerciseSets { get; set; }
        public void SetExerciseSets(List<StartedSet> exerciseSets)
        {
            this.excerciseSets = exerciseSets;
        }
    }


}
