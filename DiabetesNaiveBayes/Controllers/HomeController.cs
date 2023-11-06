using System.Diagnostics;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using DiabetesNaiveBayes.Models;
using Microsoft.AspNetCore.Mvc;

namespace DiabetesNaiveBayes.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		public string filePath = @"C:\Users\ndiepfs\Documents\diabetes_pre.csv";
		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		private List<DataPoint> ReadDataFromCsv(string filePath)
		{
			var reader = new StreamReader(filePath);
			var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
			var data = csv.GetRecords<DataPoint>().ToList();

			return data;
		}

		private (List<DataPoint>, List<DataPoint>) SplitData(List<DataPoint> data)
		{
			// Code to split data into train and test sets
			int trainDataCount = (int)(data.Count * 0.8); // 80% là tập train

			// Sắp xếp ngẫu nhiên tập dữ liệu
			Random rng = new Random();
			data = data.OrderBy(item => rng.Next()).ToList();

			// Chia thành tập train và tập test
			List<DataPoint> trainData = data.Take(trainDataCount).ToList();
			List<DataPoint> testData = data.Skip(trainDataCount).ToList();
			return (trainData, testData);
		}

		private Stats CalculateStats(List<DataPoint> trainData)
		{
			// Code to calculate statistics
			Stats stats = new Stats();

			int trueCountTrain = trainData.Count(item => item.Outcome == true);
			int falseCountTrain = trainData.Count(item => item.Outcome == false);
			stats.PTrue = (double)trueCountTrain / trainData.Count();
			stats.PFalse = (double)falseCountTrain / trainData.Count();

			stats.PregStats = CalculateGroupStats(trainData, item => item.Pregnancies, trueCountTrain, falseCountTrain);
			stats.GlucoseStats = CalculateGroupStats(trainData, item => item.Glucose, trueCountTrain, falseCountTrain);
			stats.BloodPressureStats = CalculateGroupStats(trainData, item => item.BloodPressure, trueCountTrain, falseCountTrain);
			stats.SkinThicknessStats = CalculateGroupStats(trainData, item => item.SkinThickness, trueCountTrain, falseCountTrain);
			stats.InsulinStats = CalculateGroupStats(trainData, item => item.Insulin, trueCountTrain, falseCountTrain);
			stats.BMIStats = CalculateGroupStats(trainData, item => item.BMI, trueCountTrain, falseCountTrain);
			stats.DiabetesPedigreeFunctionStats = CalculateGroupStats(trainData, item => item.DiabetesPedigreeFunction, trueCountTrain, falseCountTrain);
			stats.AgeStats = CalculateGroupStats(trainData, item => item.Age, trueCountTrain, falseCountTrain);

			return stats;
		}

		private List<GroupStats> CalculateGroupStats(List<DataPoint> data, Func<DataPoint, int> keySelector, int trueCount, int falseCount)
		{
			var groups = data.GroupBy(keySelector).OrderBy(item => item.Key);
			return groups.Select(group => new GroupStats
			{
				Key = group.Key,
				TrueCount = group.Count(item => item.Outcome == true),
				FalseCount = group.Count(item => item.Outcome == false),
				P_True = (double)group.Count(item => item.Outcome == true) / trueCount,
				P_False = (double)group.Count(item => item.Outcome == false) / falseCount
			}).ToList();
		}

		private (int, int) TestModel(List<DataPoint> testData, Stats stats)
		{
			int correctCount = 0;
			int incorrectCount = 0;
			foreach (var item in testData)
			{
				bool predict = Predict(item, stats);
				if (predict == item.Outcome) correctCount++;
				else incorrectCount++;
			}
			return (correctCount, incorrectCount);
		}

		private bool Predict(DataPoint item, Stats stats)
		{
			double? pr_True = stats.PTrue * GetProbability(item.Pregnancies, stats.PregStats, true)
				* GetProbability(item.Glucose, stats.GlucoseStats, true)
				* GetProbability(item.BloodPressure, stats.BloodPressureStats, true)
				* GetProbability(item.SkinThickness, stats.SkinThicknessStats, true)
				* GetProbability(item.Insulin, stats.InsulinStats, true)
				* GetProbability(item.BMI, stats.BMIStats, true)
				* GetProbability(item.DiabetesPedigreeFunction, stats.DiabetesPedigreeFunctionStats, true)
				* GetProbability(item.Age, stats.AgeStats, true);
			double? pr_False = stats.PFalse * GetProbability(item.Pregnancies, stats.PregStats, false)
				* GetProbability(item.Glucose, stats.GlucoseStats, false)
				* GetProbability(item.BloodPressure, stats.BloodPressureStats, false)
				* GetProbability(item.SkinThickness, stats.SkinThicknessStats, false)
				* GetProbability(item.Insulin, stats.InsulinStats, false)
				* GetProbability(item.BMI, stats.BMIStats, false)
				* GetProbability(item.DiabetesPedigreeFunction, stats.DiabetesPedigreeFunctionStats, false)
				* GetProbability(item.Age, stats.AgeStats, false);
			return pr_True > pr_False;
		}

		private double GetProbability(int value, List<GroupStats> groupStats, bool outcome)
		{
			return outcome ? groupStats.FirstOrDefault(x => x.Key == value)?.P_True ?? 0 : groupStats.FirstOrDefault(x => x.Key == value)?.P_False ?? 0;
		}

		public IActionResult Index()
		{
			var data = ReadDataFromCsv(filePath);
			var (trainData, testData) = SplitData(data);
			var stats = CalculateStats(trainData);
			var (correctCount, incorrectCount) = TestModel(testData, stats);
			double accuracy = (double)correctCount / testData.Count();

			ViewBag.pTrue = stats.PTrue;
			ViewBag.pFalse = stats.PFalse;
			ViewBag.PregStats = stats.PregStats;
			ViewBag.GlucoseStats = stats.GlucoseStats;
			ViewBag.BloodPressureStats = stats.BloodPressureStats;
			ViewBag.SkinThicknessStats = stats.SkinThicknessStats;
			ViewBag.InsulinStats = stats.InsulinStats;
			ViewBag.BMIStats = stats.BMIStats;
			ViewBag.DiabetesPedigreeFunctionStats = stats.DiabetesPedigreeFunctionStats;
			ViewBag.AgeStats = stats.AgeStats;
			ViewBag.correctCount = correctCount;
			ViewBag.incorrectCount = incorrectCount;
			ViewBag.accuracy = accuracy;
			return View();
		}

		[HttpPost]
		public JsonResult Predict(DataPoint model)
		{
			var data = ReadDataFromCsv(filePath);
			var (trainData, testData) = SplitData(data);
			var stats = CalculateStats(trainData);

			bool prediction = Predict(model, stats);
			if (prediction)
			{
				return Json(new { result = "Bị tiểu đường" });
			}
			else
			{
				return Json(new { result = "Không bị tiểu đường" });
			}

		}
	}
}
