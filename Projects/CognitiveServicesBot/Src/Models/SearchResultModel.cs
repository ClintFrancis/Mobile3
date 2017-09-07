using Newtonsoft.Json;

namespace CognitiveServicesBot.Model
{
	//Data model for search
	public class SearchResult
	{
		[JsonProperty("@odata.context")]
		public string odatacontext { get; set; }
		public Value[] value { get; set; }
	}

	//Data model for fetching facets
	public class FacetResult
	{
		[JsonProperty("@odata.context")]
		public string odatacontext { get; set; }
		[JsonProperty("@search.facets")]
		public SearchFacets searchfacets { get; set; }
		public Value[] value { get; set; }
	}

	public class Value
	{
		[JsonProperty("@search.score")]
		public float searchscore { get; set; }
		public string imageURL { get; set; }
		public string documentationURL { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public string API { get; set; }
		public string Description { get; set; }
		public string id { get; set; }
		public string rid { get; set; }
	}

	public class SearchFacets
	{
		[JsonProperty("Category@odata.type")]
		public string Categoryodatatype { get; set; }
		public Category[] Category { get; set; }
	}

	public class Category
	{
		public int count { get; set; }
		public string value { get; set; }
	}
}