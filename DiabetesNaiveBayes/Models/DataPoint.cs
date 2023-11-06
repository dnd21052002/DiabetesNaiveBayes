namespace DiabetesNaiveBayes.Models
{
    public class DataPoint
    {
        public int Pregnancies { get; set; }
        public int Glucose { get; set; }
        public int BloodPressure { get; set; }
        public int SkinThickness { get; set; }
        public int Insulin { get; set; }
        public int BMI { get; set; }
        public int DiabetesPedigreeFunction { get; set; }
        public int Age { get; set; }
        public bool Outcome { get; set; }
    }
}
