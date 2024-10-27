namespace TrackerBackend
{
    public class UserExcercise
    {
        public required int userid {  get; set; }
        public int? excerciseid { get; set; }
        public required string  excercisename { get; set; }
        public required  int trainingplanid { get; set; }
        public required int excercisesets { get; set; }
    }
}
