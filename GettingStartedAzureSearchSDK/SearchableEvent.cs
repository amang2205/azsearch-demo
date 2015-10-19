using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using System;
using System.Collections.Generic;
using System.Text;

namespace GettingStarted
{
    public class SearchableEvent
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset DateAdded { get; set; }
        public string[] Tags { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string[] Applications { get; set; }
        public string Location { get; set; }
        public GeographyPoint GeoLocation { get; set; }
        public int? Rating { get; set; }

        public static IList<Field> GetSearchableEventFields()
        {
            return new List<Field>()
            {                
                new Field() { Name = "key", Type= DataType.String, IsKey = true, IsSearchable = true, IsFilterable = true, IsSortable = true, IsFacetable = true, IsRetrievable = true },
                new Field() { Name = "name", Type = DataType.String, IsFilterable = false, IsSearchable = true, IsSortable = true, IsFacetable = true, IsRetrievable = true }, 
                new Field() { Name = "category", Type = DataType.String, IsSearchable = true, IsFilterable = true, IsSortable = true, IsFacetable = true, IsRetrievable = true  },
                new Field() { Name = "description", Type = DataType.String, IsSearchable = true, IsFilterable = true, IsSortable = true, IsFacetable = true, IsRetrievable = true  },
                new Field() { Name = "location", Type = DataType.String, IsSearchable = true, IsFilterable = true, IsSortable = true, IsFacetable = true, IsRetrievable = true  },
                new Field() { Name = "date", Type = DataType.DateTimeOffset, IsSearchable = false, IsFilterable = true, IsSortable = true, IsFacetable = true, IsRetrievable = true  },
                new Field() { Name = "tags", Type = DataType.Collection(DataType.String), IsSortable = false, IsSearchable = true, IsFilterable = true, IsFacetable = true, IsRetrievable = true  },
                new Field() { Name = "geolocation", Type = DataType.GeographyPoint, IsSearchable = false, IsFilterable = true, IsSortable = true, IsRetrievable = true  },
                new Field() { Name = "dateadded", Type = DataType.DateTimeOffset, IsSearchable = false, IsFilterable = true, IsSortable = true, IsFacetable = true, IsRetrievable = true  },
                new Field() { Name = "rating", Type = DataType.Int32, IsSearchable = false, IsFilterable = true, IsSortable = true, IsFacetable = true, IsRetrievable = true }
            };
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(string.Format("\tKey: {0}", this.Key));
            if (!string.IsNullOrWhiteSpace(this.Name)) { sb.Append(string.Format("\n\tName: {0}", this.Name)); }
            if (this.Date != null && this.Date != default(DateTimeOffset)) { sb.Append(string.Format("\n\tDate: {0}", this.Date)); }
            if (this.DateAdded != null && this.DateAdded != default(DateTimeOffset)) { sb.Append(string.Format("\n\tDateAdded: {0}", this.DateAdded)); }
            if (!string.IsNullOrWhiteSpace(this.Category)) { sb.Append(string.Format("\n\tCategory: {0}", this.Category)); }
            if (!string.IsNullOrWhiteSpace(this.Description)) { sb.Append(string.Format("\n\tDescription: {0}", this.Description)); }
            if (!string.IsNullOrWhiteSpace(this.Location)) { sb.Append(string.Format("\n\tLocation: {0}", this.Location)); }
            if (this.GeoLocation != null) { sb.Append(string.Format("\n\tGeolocation: {0}", this.GeoLocation)); }
            if (this.Rating != null) { sb.Append(string.Format("\n\tRating: {0}", this.Rating)); }

            return sb.ToString();
        }
    }
}