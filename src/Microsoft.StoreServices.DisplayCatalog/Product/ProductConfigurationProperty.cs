﻿// Generated by Xamasoft JSON Class Generator
// http://www.xamasoft.com/json-class-generator

using System.Text.Json.Serialization;

namespace Microsoft.StoreServices.DisplayCatalog.Product
{
    public class ProductConfigurationProperty
    {
        [JsonPropertyName("Key")]
        public string Key;

        [JsonPropertyName("Title")]
        public string Title;

        [JsonPropertyName("ShortDescription")]
        public object ShortDescription;

        [JsonPropertyName("Description")]
        public object Description;

        [JsonPropertyName("Values")]
        public Value[] Values;
    }
}