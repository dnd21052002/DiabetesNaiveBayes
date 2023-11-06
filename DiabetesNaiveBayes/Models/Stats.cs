namespace DiabetesNaiveBayes.Models
{
	public class Stats
	{
		public double PTrue { get; set; }
		public double PFalse { get; set; }
		public List<GroupStats> PregStats { get; set; }
		public List<GroupStats> GlucoseStats { get; set; }
		public List<GroupStats> BloodPressureStats { get; set; }
		public List<GroupStats> SkinThicknessStats { get; set; }
		public List<GroupStats> InsulinStats { get; set; }
		public List<GroupStats> BMIStats { get; set; }
		public List<GroupStats> DiabetesPedigreeFunctionStats { get; set; }
		public List<GroupStats> AgeStats { get; set; }
	}

	public class GroupStats
	{
		public int Key { get; set; }
		public int TrueCount { get; set; }
		public int FalseCount { get; set; }
		public double P_True { get; set; }
		public double P_False { get; set; }
	}
}
