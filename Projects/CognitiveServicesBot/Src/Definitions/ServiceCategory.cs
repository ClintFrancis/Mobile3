using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CognitiveServicesBot
{
    public static class ServiceCategory
    {
        public const string Vision = "Vision";
        public const string Speech = "Speech";
        public const string Language = "Language";
        public const string Knowledge = "Knowledge";
        public const string Search = "Search";

        public static List<string> All = new List<string>() { Vision, Speech, Language, Knowledge, Search };
    }
}