using System.Net.Http;
using System.Web.Configuration;
using System.Threading.Tasks;
using CognitiveServicesBot.Model;
using Newtonsoft.Json;
using System;
using System.Net;

namespace CognitiveServicesBot.Services
{
	[Serializable]
	public class AzureSearchService
	{
		private static readonly string QueryString = $"https://{WebConfigurationManager.AppSettings["SearchName"]}.search.windows.net/indexes/{WebConfigurationManager.AppSettings["IndexName"]}/docs?api-key={WebConfigurationManager.AppSettings["SearchKey"]}&api-version=2016-09-01&";

		public async Task<FacetResult> FetchFacets()
		{
			using (var httpClient = new HttpClient())
			{
				string facetQuey = $"{QueryString}facet=Category";
				string response = await httpClient.GetStringAsync(facetQuey);
				return JsonConvert.DeserializeObject<FacetResult>(response);
			}
		}
		public async Task<SearchResult> FilterByCategory(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException("Cannot search with a null value");

			using (var httpClient = new HttpClient())
			{
				string query = $"{QueryString}search={value}&searchFields=Category";
				string response = await httpClient.GetStringAsync(query);
				return JsonConvert.DeserializeObject<SearchResult>(response);
			}
		}

		public async Task<SearchResult> FilterByAPI(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException("Cannot search with a null value");

			using (var httpClient = new HttpClient())
			{
				string parsedAPI = WebUtility.UrlEncode(value.Replace(" ", "+"));
				string query = $"{QueryString}search='{parsedAPI}'&searchFields=Api"; 
				string response = await httpClient.GetStringAsync(query);
				return JsonConvert.DeserializeObject<SearchResult>(response);
			}
		}

		public async Task<SearchResult> FilterByFeature(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException("Cannot search with a null value");

			using (var httpClient = new HttpClient())
			{
				string parsedAPI = WebUtility.UrlEncode(value.Replace(" ", "+"));
				string query = $"{QueryString}search='{parsedAPI}'&searchFields=Api";
				string response = await httpClient.GetStringAsync(query);
				return JsonConvert.DeserializeObject<SearchResult>(response);
			}
		}

		public async Task<SearchResult> Search(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException("Cannot search with a null value");

			using (var httpClient = new HttpClient())
			{
				string parsedSearch = WebUtility.UrlEncode(value);
				string query = $"{QueryString}search='{parsedSearch}'";
				string response = await httpClient.GetStringAsync(query);
				return JsonConvert.DeserializeObject<SearchResult>(response);
			}
		}

		public async Task<Value> GetFeature(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException("Cannot search with a null value");

			using (var httpClient = new HttpClient())
			{
				string parsedAPI = WebUtility.UrlEncode(value.Replace(" ", "+"));
				string query = $"{QueryString}search='{parsedAPI}'&searchFields=Name";
				string response = await httpClient.GetStringAsync(query);
				var results = JsonConvert.DeserializeObject<SearchResult>(response);
				return (results.value.Length >= 1) ? results.value[0] : null;
			}
		}

		public async Task<string> CheckCategoryExists(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException("Cannot search with a null value");

			using (var httpClient = new HttpClient())
			{
				string query = $"{QueryString}search='{value}'&searchFields=Category&$top=1";
				string response = await httpClient.GetStringAsync(query);
				var results = JsonConvert.DeserializeObject<SearchResult>(response);
				return (results.value.Length == 1) ? results.value[0].Category : string.Empty;
			}
		}

		public async Task<string> CheckAPIExists(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException("Cannot search with a null value");

			using (var httpClient = new HttpClient())
			{
				string parsedName = WebUtility.UrlEncode(value.Replace(" ", "+"));
				string query = $"{QueryString}search='{parsedName}'&searchFields=Api&$top=1";
				string response = await httpClient.GetStringAsync(query);
				var results = JsonConvert.DeserializeObject<SearchResult>(response);
				return (results.value.Length == 1) ? results.value[0].API : string.Empty;
			}
		}
	}
}