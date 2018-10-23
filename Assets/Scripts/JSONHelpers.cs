using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Assets.IL.Scripts.Common
{
	public static class JSONHelper
	{
		public static string GetStandardizedDatasetString(TextAsset jsonText, int dataset,
			string DatasetKey = "Dataset", string DatasetsKey = "Datasets")
		{
			var data = JSON.Parse(jsonText.text);
			var datasets = data[DatasetsKey];
			JSONNode datasetJson = null;

			for (var i = 0; i < datasets.Count; i++)
			{
				if (datasets[i][DatasetKey].AsInt != dataset) continue;
				datasetJson = datasets[i];
				break;
			}

			return datasetJson.ToString();
		}

		/// <summary>
		/// Assumes json data is in the new Generic Readable Format { "Datasets" : {"1": [{"Question" : {"FieldName": "Value", "FieldName": "Value"}}, {"Question" : {"FieldName": "Value", "FieldName": "Value"}}]}}
		/// </summary>
		/// <typeparam name="T">DataType that matches the questions format</typeparam>
		/// <param name="jsonText"></param>
		/// <param name="dataset"></param>
		/// <param name="activity"></param>
		/// <returns>List of question data objects</returns>
		public static List<T> GetDatasetQuestionData<T>(TextAsset jsonText, int dataset, string activity)
		{
			var data = JSON.Parse(jsonText.text);
			var datasets = data["Datasets"];

			var datasetJson = datasets[dataset.ToString()];
			if (datasetJson != null && datasetJson.Count > 0)
			{
				return SetupQuestions<T>(datasetJson);
			}

			UnityEngine.Debug.LogWarning(string.Format("Missing dataset data for {0}. Dataset[{1}]", activity, dataset));
			return new List<T>();
		}

		/// <summary>
		/// Takes a json node that has a list of {Question : {"FieldName":"Value", "Fieldname":"Value"}} objects. Deserializes them into a list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="datasetNode"></param>
		/// <returns>List of question data objects</returns>
		public static List<T> SetupQuestions<T>(JSONNode datasetNode)
		{
			List<T> questions = new List<T>(datasetNode.Count);
			for (int i = 0; i < datasetNode.Count; i++)
			{
				questions.Add(JsonUtility.FromJson<T>(datasetNode[i]["Question"].ToString()));
			}
			return questions;
		}
	}
}
