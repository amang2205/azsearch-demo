using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Spatial;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GettingStarted
{
    class Program
    {
        private static SearchServiceClient _searchClient;
        private static SearchIndexClient _indexClient;
        private static string currentIndexName;

        private const ConsoleColor EnabledColor = ConsoleColor.White; // color for items that are expected to succeed
        private const ConsoleColor DisabledColor = ConsoleColor.DarkGray; // color for items that are expected to fail

        static void Main(string[] args)
        {
            // initialize new Azure Service
            string searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
            string apiKey = ConfigurationManager.AppSettings["SearchServiceApiKey"];
            _searchClient = new SearchServiceClient(searchServiceName, new SearchCredentials(apiKey));

            Task.WaitAll(MenuLoopAsync());
        }

        /// <summary>
        /// Main program loop.
        /// </summary>
        private async static Task MenuLoopAsync()
        {
            // Loop until the user chose "Exit".
            bool continueLoop;
            do
            {
                await PrintIndexStateAsync();
                Console.WriteLine();

                await PrintMenuAsync();
                Console.WriteLine();

                continueLoop = await GetMenuChoiceAndExecuteAsync();
                Console.WriteLine();
            }
            while (continueLoop);
        }

        /// <summary>
        /// Gets the user's chosen menu item and executes it.
        /// </summary>
        /// <returns>true if the program should continue executing.</returns>
        private async static Task<bool> GetMenuChoiceAndExecuteAsync()
        {
            while (true)
            {
                int inputValue = ConsoleUtils.ReadIntegerInput("Enter an option [0-14] and press ENTER: ");

                switch (inputValue)
                {
                    case 1: // Create index
                        Console.Clear();
                        await CreateIndexAsync();
                        return true;
                    case 2: // Add documents
                        Console.Clear();
                        await AddDocumentsAsync();
                        return true;
                    case 3: // Count index
                        Console.Clear();
                        await CountIndexAsync();
                        return true;
                    case 4: // Query index (simple ALL)
                        Console.Clear();
                        await QueryAsync();
                        return true;
                    case 5: // Query index (simple ANY)
                        Console.Clear();
                        await QueryAsync(2);
                        return true;
                    case 6: // Query index (with facets)
                        Console.Clear();
                        await QueryAsync(3);
                        return true;
                    case 7: // Update index
                        Console.Clear();
                        await UpdateIndexAsync();
                        return true;
                    case 8: // Query index (using scoring profiles)
                        Console.Clear();
                        await QueryAsync(4);
                        return true;
                    case 9: // Update index
                        Console.Clear();
                        await UpdateIndexGeoScoringProfileAsync();
                        return true;
                    case 10: // Query index (using geolocation scoring profile)
                        Console.Clear();
                        await QueryAsync(5);
                        return true;
                    case 11: // Index Update (freshness + tag scoring profile)
                        Console.Clear();
                        await UpdateIndexFreshnessAndTagScoringProfileAsync();
                        return true;
                    case 12: // Query index -ALL(using freshness + tag scoring profile)
                        Console.Clear();
                        await QueryAsync(6);
                        return true;
                    case 13: // Document lookup
                        Console.Clear();
                        await LookupAsync();
                        return true;
                    case 14: // Delete all
                        Console.Clear();
                        await DeleteIndexAsync();
                        return true;
                    case 0: // Exit
                        return false;
                }
            }
        }

        /// <summary>
        /// Asynchronously creates a new index
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        private static async Task CreateIndexAsync()
        {
            try
            {
                Console.WriteLine("What's the index's name: ");
                currentIndexName = Console.ReadLine();

                var eventSearchableFields = SearchableEvent.GetSearchableEventFields();
                var eventIndex = new Index()
                {
                    Name = currentIndexName,
                    Fields = SearchableEvent.GetSearchableEventFields(),
                    Suggesters = new List<Suggester>
                    {
                        new Suggester()
                        {
                            Name = "sg",
                            SearchMode = SuggesterSearchMode.AnalyzingInfixMatching,
                            SourceFields = new List<string> {"name"}
                        }
                    }
                };
                var result = await _searchClient.Indexes.CreateAsync(eventIndex);

                if (result.Index != null
                    && result.StatusCode == System.Net.HttpStatusCode.OK)
                    ConsoleUtils.WriteColor(ConsoleColor.Green, "Index {0} added successfully.", currentIndexName);
            }
            catch (Exception ex)
            {
                ConsoleUtils.WriteColor(ConsoleColor.Red, "{0}", ex.ToString());
            }
        }

        /// <summary>
        /// Writes the current indexes state to the console
        /// </summary>
        private async static Task PrintIndexStateAsync()
        {
            ConsoleUtils.WriteColor(ConsoleColor.Yellow, "Current indexes state:", null);

            if (_searchClient != null)
            {
                foreach (var index in await _searchClient.Indexes.ListAsync())
                {
                    ConsoleUtils.WriteColor(ConsoleColor.Yellow, "\t{0} ({1})", index.Name, (await _searchClient.Indexes.GetStatisticsAsync(index.Name)).DocumentCount);
                }
            }
            else
                ConsoleUtils.WriteColor(ConsoleColor.Yellow, "\tService doesn't contain any indexes.", null);
        }

        /// <summary>
        /// Writes the program menu.
        /// </summary>
        private static async Task PrintMenuAsync()
        {
            ConsoleColor otherMenuItemColor; // color for other menu items
            if ((await _searchClient.Indexes.ListAsync()).Count() == 0)
                otherMenuItemColor = DisabledColor;
            else
                otherMenuItemColor = EnabledColor;

            ConsoleUtils.WriteColor(EnabledColor, "1. Create index");
            ConsoleUtils.WriteColor(otherMenuItemColor, "2. Add documents");
            ConsoleUtils.WriteColor(otherMenuItemColor, "3. Count index");
            ConsoleUtils.WriteColor(otherMenuItemColor, "4. Query index - ALL (simple)");
            ConsoleUtils.WriteColor(otherMenuItemColor, "5. Query index - ANY (simple)");
            ConsoleUtils.WriteColor(otherMenuItemColor, "6. Query index - ALL (with facets)");
            ConsoleUtils.WriteColor(otherMenuItemColor, "7. Index update");
            ConsoleUtils.WriteColor(otherMenuItemColor, "8. Query index - ALL (using scoring profiles)");
            ConsoleUtils.WriteColor(otherMenuItemColor, "9. Index update (geo-location scoring profile)");
            ConsoleUtils.WriteColor(otherMenuItemColor, "10. Query index - ALL (using geo-location scoring profile)");
            ConsoleUtils.WriteColor(otherMenuItemColor, "11. Index update (freshness + tag scoring profile)");
            ConsoleUtils.WriteColor(otherMenuItemColor, "12. Query index - ALL (using freshness + tag scoring profile)");
            ConsoleUtils.WriteColor(otherMenuItemColor, "13. Document lookup");
            ConsoleUtils.WriteColor(otherMenuItemColor, "14. Delete index");
            ConsoleUtils.WriteColor(EnabledColor, "0. Exit");
        }

        /// <summary>
        /// Asynchronously adds random documents
        /// </summary>
        /// <returns></returns>
        private static async Task AddDocumentsAsync()
        {
            try
            {
                #region prepare mock data
                var rand = new Random();

                var loremIpsum = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse interdum purus nec lectus viverra, consequat auctor nunc maximus. Mauris porttitor urna dignissim risus laoreet, ut vulputate purus malesuada. Vestibulum ipsum odio, pharetra eget erat vitae, ullamcorper hendrerit justo. In gravida tincidunt turpis. Proin at sodales justo, a varius justo. Praesent et nunc vel orci congue ullamcorper ac id tortor. Vivamus id semper eros. 
Mauris dictum pulvinar elit et interdum. Nam nec dictum lacus. Sed ut volutpat lorem. Vestibulum vitae porttitor erat, quis pellentesque quam. Sed molestie auctor eros in hendrerit. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Sed bibendum luctus mollis. Aliquam vitae justo placerat, fringilla risus suscipit, interdum nibh. 
Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Phasellus vitae feugiat dolor. Maecenas maximus dignissim finibus. Donec gravida neque dui. Quisque non est bibendum libero suscipit interdum vel vel nulla. Morbi tincidunt rhoncus tellus ac faucibus. Nullam sodales ipsum arcu, vel molestie leo consectetur sit amet. Donec sed ex nec tortor luctus vehicula placerat et ipsum. 
Nullam vel turpis quis velit varius aliquam. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent mattis odio eu ex feugiat vulputate. Suspendisse vel ipsum augue. Nulla a dolor posuere, facilisis massa ac, pellentesque sem. Nunc tincidunt ut justo nec molestie. Nam vulputate nibh non lorem dictum pellentesque. Aliquam erat volutpat. Cras ac magna non mi maximus varius. Cras mauris dolor, bibendum non eleifend nec, tincidunt sit amet nisi. Fusce iaculis elit non felis mollis laoreet. Vestibulum sed metus quis neque vestibulum blandit. Quisque varius, massa ut interdum sodales, erat est faucibus eros, quis malesuada orci nisl ut erat. Proin eget nisi in orci lacinia pulvinar. 
Praesent a odio in est convallis vulputate. Proin semper tellus nisi, sit amet sodales ipsum lobortis id. Mauris imperdiet ante purus, eu finibus arcu vestibulum eu. Fusce ex nisl, tincidunt vel augue a, efficitur porta tortor. Maecenas ut orci dui. Nunc rutrum tempus vehicula. Cras vel vehicula felis, a scelerisque nibh. Interdum et malesuada fames ac ante ipsum primis in faucibus. Quisque id congue nisl. Mauris vitae ipsum ut risus pretium pretium at vel eros. Fusce consectetur erat sit amet urna lobortis varius.";

                var stadiums = new Stadium[]
                {
                    new Stadium(){Name = "MetLife Stadium", GeoLocation = GeographyPoint.Create(40.8135378, -74.0744119)} ,
                    new Stadium(){Name = "Lambeau Field", GeoLocation = GeographyPoint.Create(44.501341, -88.062208)} ,
                    new Stadium(){Name = "AT&T Stadium", GeoLocation = GeographyPoint.Create(32.747284, -97.094494)} ,
                    new Stadium(){Name = "FedEx Field", GeoLocation = GeographyPoint.Create(38.907699, -76.866338)} ,
                    new Stadium(){Name = "Arrowhead Stadium", GeoLocation = GeographyPoint.Create(39.048939, -94.483916)} ,
                    new Stadium(){Name = "Sports Authority Field at Mile High", GeoLocation = GeographyPoint.Create(39.743948, -105.020084)} ,
                    new Stadium(){Name = "Sun Life Stadium", GeoLocation = GeographyPoint.Create(25.957966, -80.23886)} ,
                    new Stadium(){Name = "Bank of America Stadium", GeoLocation = GeographyPoint.Create(35.2259943, -80.8531416)} ,
                    new Stadium(){Name = "Mercedes-Benz Superdome", GeoLocation = GeographyPoint.Create(29.951061, -90.081244)} ,
                    new Stadium(){Name = "FirstEnergy Stadium", GeoLocation = GeographyPoint.Create(41.506054, -81.699548)} ,
                    new Stadium(){Name = "Ralph Wilson Stadium", GeoLocation = GeographyPoint.Create(42.773698, -78.786948)} ,
                    new Stadium(){Name = "Georgia Dome", GeoLocation = GeographyPoint.Create(33.75769, -84.400829)} ,
                    new Stadium(){Name = "NRG Stadium", GeoLocation = GeographyPoint.Create(29.684722, -95.410707)} ,
                    new Stadium(){Name = "Qualcomm Stadium", GeoLocation = GeographyPoint.Create(32.783994, -117.11997)} ,
                    new Stadium(){Name = "LP Field", GeoLocation = GeographyPoint.Create(42.090946, -71.264346)} ,
                    new Stadium(){Name = "Lincoln Financial Field", GeoLocation = GeographyPoint.Create(39.900732, -75.167535)} ,
                    new Stadium(){Name = "Levi's Stadium", GeoLocation = GeographyPoint.Create(37.404108, -121.970274)} ,
                    new Stadium(){Name = "EverBank Field", GeoLocation = GeographyPoint.Create(30.324662, -81.637074)} ,
                    new Stadium(){Name = "CenturyLink Field", GeoLocation = GeographyPoint.Create(47.595152, -122.331639)} ,
                    new Stadium(){Name = "Edward Jones Dome", GeoLocation = GeographyPoint.Create(38.6328042, -90.1884177)} ,
                    new Stadium(){Name = "Raymond James Stadium", GeoLocation = GeographyPoint.Create(27.975959, -82.504133)} ,
                    new Stadium(){Name = "Paul Brown Stadium", GeoLocation = GeographyPoint.Create(40.446765, -80.01576)} ,
                    new Stadium(){Name = "Heinz Field", GeoLocation = GeographyPoint.Create(40.8135378, -74.0744119)} ,
                    new Stadium(){Name = "Ford Field", GeoLocation = GeographyPoint.Create(42.340006, -83.045603)} ,
                    new Stadium(){Name = "University of Phoenix Stadium", GeoLocation = GeographyPoint.Create(33.527625, -112.262559)} ,
                    new Stadium(){Name = "Lucas Oil Stadium", GeoLocation = GeographyPoint.Create(39.760101, -86.163888)},
                    new Stadium(){Name = "Soldier Field", GeoLocation = GeographyPoint.Create(41.862313, -87.616688)},
                    new Stadium(){Name = "O.co Coliseum", GeoLocation = GeographyPoint.Create(37.751595, -122.200546)}
                };

                var teams = new Team[]
                {
                    new Team(){Name = "Falcons", Tags = new string[]{"falcons","atlanta"}},
                    new Team(){Name = "Jaguars", Tags = new string[]{"Jaguars","jacksonville"}},
                    new Team(){Name = "Bengals", Tags = new string[]{"Bengals","cincinnati"}},
                    new Team(){Name = "Colts", Tags = new string[]{"Colts","indianapolis"}},
                    new Team(){Name = "Packers", Tags = new string[]{"packers","green bay"}},
                    new Team(){Name = "Chiefs", Tags = new string[]{"chiefs","kansas"}},
                    new Team(){Name = "Dolphins", Tags = new string[]{"dolphins","Miami"}},
                    new Team(){Name = "Rams", Tags = new string[]{"rams","St. Louis"}},
                    new Team(){Name = "Jets", Tags = new string[]{"new york","jets"}},
                    new Team(){Name = "Eagles", Tags = new string[]{"eagles","philadelphia"}},
                    new Team(){Name = "Patriots", Tags = new string[]{"patriots","new england"}},
                    new Team(){Name = "Giants", Tags = new string[]{"giants","new york"}},
                    new Team(){Name = "Panthers", Tags = new string[]{"panthers","carolina"}},
                    new Team(){Name = "Steelers", Tags = new string[]{"steelers","pittsburg"}},
                    new Team(){Name = "Redskins", Tags = new string[]{"redskins","washington"}},
                    new Team(){Name = "Buccaneers", Tags = new string[]{"buccaneers","tampa bay"}},
                    new Team(){Name = "Bears", Tags = new string[]{"bears","chicago"}},
                    new Team(){Name = "Browns", Tags = new string[]{"browns","cleveland"}},
                    new Team(){Name = "Broncos", Tags = new string[]{"broncos","denver"}},
                    new Team(){Name = "Cowboys", Tags = new string[]{"cowboys","dallas"}},
                    new Team(){Name = "49ers", Tags = new string[]{"49ers","san francisco"}},
                    new Team(){Name = "Texans", Tags = new string[]{"texans","houston"}},
                    new Team(){Name = "Ravens", Tags = new string[]{"ravens","baltimore"}},
                    new Team(){Name = "Saints", Tags = new string[]{"saints","new orleans"}},
                    new Team(){Name = "Vikings", Tags = new string[]{"vikings","minnesota"}},
                    new Team(){Name = "Titans", Tags = new string[]{"titans","tennessee"}},
                    new Team(){Name = "Seahawks", Tags = new string[]{"seahawks","seattle"}},
                    new Team(){Name = "Raiders", Tags = new string[]{"raiders","oakland"}},
                    new Team(){Name = "Cardinals", Tags = new string[]{"cardinals","arizona"}},
                    new Team(){Name = "Chargers", Tags = new string[]{"chargers","san diego"}}
                };

                var indexOperations = new List<IndexAction>();

                for (int i = 0; i < 1000; i++)
                {
                    var randStadium = rand.Next(27);
                    var randTeamHome = rand.Next(30);
                    var randTeamAway = rand.Next(30);
                    var eventName = string.Format("{0} vs {1}", teams[randTeamHome].Name, teams[randTeamAway].Name);
                    var eventTags = new string[4];
                    Array.Copy(teams[randTeamHome].Tags, 0, eventTags, 0, 2);
                    Array.Copy(teams[randTeamAway].Tags, 0, eventTags, 2, 2);

                    var document = new Document();
                    document.Add("category", "sport");
                    document.Add("name", eventName);
                    document.Add("tags", eventTags);
                    document.Add("dateadded", DateTime.Now);
                    document.Add("date", DateTime.Now.AddDays(rand.NextDouble() * 100));
                    document.Add("description", loremIpsum);
                    document.Add("location", stadiums[randStadium].Name);
                    document.Add("geolocation", stadiums[randStadium].GeoLocation);
                    document.Add("key", Guid.NewGuid().ToString());
                    document.Add("rating", rand.Next(10));
                    indexOperations.Add(new IndexAction(IndexActionType.Upload, document));
                }

                #endregion
                ValidateCurrentIndexName();

                Console.WriteLine(string.Format("Adding {0} documents into index {1}...", indexOperations.Count, currentIndexName));

                ValidateIndexClient();
                var result = await _indexClient.Documents.IndexAsync(new IndexBatch(indexOperations));

                if (result != null)
                    ConsoleUtils.WriteColor(ConsoleColor.Green, "Added {0} documents into index {1} successfully.", indexOperations.Count, currentIndexName);
            }
            catch (Exception ex)
            {
                ConsoleUtils.WriteColor(ConsoleColor.Red, "{0}", ex.ToString());
            }
        }

        private static void ValidateIndexClient()
        {
            if (_indexClient == null)
                _indexClient = _searchClient.Indexes.GetClient(currentIndexName);
        }

        /// <summary>
        /// Makes sure that an index name has been currently specified
        /// </summary>
        private static void ValidateCurrentIndexName()
        {
            if (string.IsNullOrWhiteSpace(currentIndexName))
            {
                Console.WriteLine("What's the index's name: ");
                currentIndexName = Console.ReadLine();
            }
        }

        /// <summary>
        /// Asynchronously counts how many documents exist within an index
        /// </summary>
        /// <returns></returns>
        private async static Task CountIndexAsync()
        {
            try
            {
                ValidateCurrentIndexName();

                var result = await _searchClient.Indexes.GetStatisticsAsync(currentIndexName);

                ConsoleUtils.WriteColor(ConsoleColor.Green, "Index {0} has {1} documents.", currentIndexName, result.DocumentCount);
            }
            catch (Exception ex)
            {
                ConsoleUtils.WriteColor(ConsoleColor.Red, "{0}", ex.ToString());
            }
        }

        /// <summary>
        /// Asynchronously deletes an index
        /// </summary>
        /// <returns></returns>
        private static async Task DeleteIndexAsync()
        {
            try
            {
                ValidateCurrentIndexName();

                var result = await _searchClient.Indexes.DeleteAsync(currentIndexName);

                if (result != null
                    && result.StatusCode == HttpStatusCode.NoContent)
                    ConsoleUtils.WriteColor(ConsoleColor.Green, "Index {0} deleted successfully.", currentIndexName);
            }
            catch (Exception ex)
            {
                ConsoleUtils.WriteColor(ConsoleColor.Red, "{0}", ex.ToString());
            }
        }

        /// <summary>
        /// Asynchronously fetches a document from an index
        /// </summary>
        /// <returns></returns>
        private async static Task LookupAsync()
        {
            try
            {
                ValidateCurrentIndexName();

                Console.WriteLine("What's the document's key?");
                var documentKey = Console.ReadLine();

                var fieldsNames = new List<string>();
                foreach (var field in SearchableEvent.GetSearchableEventFields())
                    fieldsNames.Add(field.Name);

                var documentResult = await _indexClient.Documents.GetAsync<SearchableEvent>(documentKey);

                ConsoleUtils.WriteColor(ConsoleColor.Green, "Document {0}:", documentKey);
                Console.WriteLine(documentResult.Document.ToString());
            }
            catch (Exception ex)
            {
                ConsoleUtils.WriteColor(ConsoleColor.Red, "{0}", ex.ToString());
            }
        }

        private static SearchParameters GetSearchParameters(int method = 1)
        {
            SearchParameters returnVal = null;
            switch (method)
            {
                case 1:
                    {
                        #region method 1 - simple ALL
                        var searchTextMode = SearchMode.All;
                        var searchCount = true;
                        var searchSelectFieldsNames = new List<string>() { "key", "name" };
                        returnVal = new SearchParameters()
                        {
                            SearchMode = searchTextMode,
                            IncludeTotalResultCount = searchCount,
                            Select = searchSelectFieldsNames
                        };
                        break;
                        #endregion
                    }
                case 2:
                    {
                        #region method 2 - simple ANY
                        var searchTextMode = SearchMode.Any;
                        var searchCount = true;
                        var searchSelectFieldsNames = new List<string>() { "key", "name" };
                        returnVal = new SearchParameters()
                        {
                            SearchMode = searchTextMode,
                            IncludeTotalResultCount = searchCount,
                            Select = searchSelectFieldsNames
                        };
                        break;
                        #endregion
                    }
                case 3:
                    {
                        #region method 3 - with facets
                        var searchTextMode = SearchMode.All;
                        var searchCount = true;
                        var searchSelectFieldsNames = new List<string>() { "key", "name", "date" };
                        returnVal = new SearchParameters()
                        {
                            SearchMode = searchTextMode,
                            IncludeTotalResultCount = searchCount,
                            Select = searchSelectFieldsNames,
                            Facets = new List<string>
                            {
                                "rating,count:5,sort:count",
                                "date,values:"+DateTimeOffset.Now.AddDays(10).ToString("u")+"|"+DateTimeOffset.Now.AddDays(20).ToString("u")
                            },
                        };
                        break;
                        #endregion
                    }
                case 4:
                    {
                        #region method 4 - using scoring profiles
                        var searchTextMode = SearchMode.All;
                        var searchCount = true;
                        var searchSelectFieldsNames = new List<string>() { "key", "name", "date", "dateadded", "rating" };
                        returnVal = new SearchParameters()
                        {
                            SearchMode = searchTextMode,
                            IncludeTotalResultCount = searchCount,
                            Select = searchSelectFieldsNames,
                            Top = 10,
                            Facets = new List<string>
                            {
                                "rating,count:5,sort:count",
                                "date,values:"+DateTimeOffset.Now.AddDays(10).ToString("u")+"|"+DateTimeOffset.Now.AddDays(20).ToString("u")
                            },
                            ScoringProfile = "default"
                        };
                        break;
                        #endregion
                    }
                case 5:
                    {
                        #region method 5 - using scoring profiles
                        Console.WriteLine("Where are you searching from?");
                        var searchProfileParamVal = Console.ReadLine();
                        var searchableFields = new List<string>() { "name" };
                        var searchTextMode = SearchMode.All;
                        var searchCount = true;
                        var searchSelectFieldsNames = new List<string>() { "key", "name", "date", "location" };
                        returnVal = new SearchParameters()
                        {
                            SearchMode = searchTextMode,
                            IncludeTotalResultCount = searchCount,
                            Select = searchSelectFieldsNames,
                            Top = 10,
                            Facets = new List<string>
                            {
                                "rating,count:5,sort:count",
                                "date,values:"+DateTimeOffset.Now.AddDays(10).ToString("u")+"|"+DateTimeOffset.Now.AddDays(20).ToString("u")
                            },
                            ScoringProfile = "defaultgeo",
                            ScoringParameters = new List<string> { string.Format("mylocation:{0}", searchProfileParamVal) }
                        };
                        break;
                        #endregion
                    }
                case 6:
                    {
                        #region method 6 - using scoring profiles (tag boosting as well)
                        Console.WriteLine("Which is your favourite team?");
                        var favouriteTeamTag = Console.ReadLine();
                        var searchTextMode = SearchMode.All;
                        var searchCount = true;
                        var searchSelectFieldsNames = new List<string>() { "key", "name", "date", "location" };
                        returnVal = new SearchParameters()
                        {
                            SearchMode = searchTextMode,
                            IncludeTotalResultCount = searchCount,
                            Select = searchSelectFieldsNames,
                            Top = 10,
                            Facets = new List<string>
                            {
                                "rating,count:5,sort:count",
                                "date,values:"+DateTimeOffset.Now.AddDays(10).ToString("u")+"|"+DateTimeOffset.Now.AddDays(20).ToString("u")
                            },
                            ScoringProfile = "default",
                            ScoringParameters = new List<string> { string.Format("tagsParameter:{0}", favouriteTeamTag) }
                        };
                        break;
                        #endregion
                    }
            }

            return returnVal;
        }

        /// <summary>
        /// Asynchronously queries an index
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private async static Task QueryAsync(int method = 1)
        {
            try
            {
                ValidateCurrentIndexName();
                ValidateIndexClient();

                Console.WriteLine("What would you like to search?");
                var searchText = Console.ReadLine();

                var searchParams = GetSearchParameters(method);
                var results = await _indexClient.Documents.SearchAsync<SearchableEvent>(searchText, searchParams);

                if (results != null)
                {
                    var sb = new StringBuilder();

                    sb.Append("Results: ");
                    sb.Append(string.Format("{0}/{1}", results.Count(), results.Count));
                    sb.Append(Environment.NewLine);
                    sb.Append("**********************************************************");
                    foreach (var result in results)
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append(string.Format("Score: {0}", result.Score));
                        sb.Append(Environment.NewLine);
                        sb.Append(result.Document.ToString());
                    }
                    if (results.Facets != null)
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append("----------------------------------------------------------");
                        sb.Append(Environment.NewLine);
                        sb.Append("Facets:");
                        sb.Append(Environment.NewLine);
                        foreach (var facet in results.Facets)
                        {
                            sb.Append(string.Format("{0}\n", facet.Key));
                            foreach (var facetValue in facet.Value)
                            {
                                if (facetValue.Type == FacetType.Value)
                                    sb.Append(string.Format("\t{0}", facetValue.Value));
                                else if (facetValue.From != null)
                                    if (facetValue.To != null)
                                        sb.Append(string.Format("\t{0} - {1}", facetValue.From, facetValue.To));
                                    else
                                        sb.Append(string.Format("\t>{0}", facetValue.From));
                                else if (facetValue.To != null)
                                    sb.Append(string.Format("\t<{0}", facetValue.To));
                                sb.Append(string.Format(" ({0})\n", facetValue.Count));
                            }
                        }
                    }
                    sb.Append(Environment.NewLine);
                    sb.Append("**********************************************************");
                    Console.WriteLine(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                ConsoleUtils.WriteColor(ConsoleColor.Red, "{0}", ex.ToString());
            }
        }

        /// <summary>
        /// Asynchronously updates the index
        /// </summary>
        /// <returns></returns>
        private async static Task UpdateIndexAsync()
        {
            try
            {
                ValidateCurrentIndexName();

                var function1 = new FreshnessScoringFunction()
                {
                    FieldName = "dateadded",
                    Boost = 200,
                    Parameters = new FreshnessScoringParameters(new TimeSpan(0, 0, 5, 0)),
                    Interpolation = ScoringFunctionInterpolation.Logarithmic
                };

                //var function2 = new MagnitudeScoringFunction()
                //{
                //    Boost = 1000,
                //    Parameters = new MagnitudeScoringParameters(9, 10),
                //    FieldName = "rating",
                //    Interpolation = ScoringFunctionInterpolation.Constant
                //};

                var scoringProfile1 = new ScoringProfile()
                {
                    Name = "default",
                    FunctionAggregation = ScoringFunctionAggregation.Sum,
                    Functions = new List<ScoringFunction> { function1 },
                };

                var eventIndex = (await _searchClient.Indexes.GetAsync(currentIndexName)).Index;
                eventIndex.ScoringProfiles = new List<ScoringProfile> { scoringProfile1 };
                var result = await _searchClient.Indexes.CreateOrUpdateAsync(eventIndex);
                if (result != null
                    && result.Index != null
                    && result.StatusCode == HttpStatusCode.OK)
                    ConsoleUtils.WriteColor(ConsoleColor.Green, "Index {0} updated successfully.", currentIndexName);
            }
            catch (Exception ex)
            {
                ConsoleUtils.WriteColor(ConsoleColor.Red, "{0}", ex.ToString());
            }
        }

        /// <summary>
        /// Asynchronously updates the index
        /// </summary>
        /// <returns></returns>
        private async static Task UpdateIndexFreshnessAndTagScoringProfileAsync()
        {
            try
            {
                ValidateCurrentIndexName();

                var function1 = new FreshnessScoringFunction()
                {
                    FieldName = "dateadded",
                    Boost = 200,
                    Parameters = new FreshnessScoringParameters(new TimeSpan(0, 0, 5, 0)),
                    Interpolation = ScoringFunctionInterpolation.Logarithmic
                };

                var function2 = new TagScoringFunction()
                {
                    Boost = 500,
                    FieldName = "tags",
                    Interpolation = ScoringFunctionInterpolation.Linear,
                    Parameters = new TagScoringParameters("tagsParameter")
                };

                var scoringProfile1 = new ScoringProfile()
                {
                    Name = "default",
                    FunctionAggregation = ScoringFunctionAggregation.Sum,
                    Functions = new List<ScoringFunction> { function1, function2 },
                };

                var eventIndex = (await _searchClient.Indexes.GetAsync(currentIndexName)).Index;
                eventIndex.ScoringProfiles = new List<ScoringProfile> { scoringProfile1 };
                var result = await _searchClient.Indexes.CreateOrUpdateAsync(eventIndex);
                if (result != null
                    && result.Index != null
                    && result.StatusCode == HttpStatusCode.OK)
                    ConsoleUtils.WriteColor(ConsoleColor.Green, "Index {0} updated successfully.", currentIndexName);
            }
            catch (Exception ex)
            {
                ConsoleUtils.WriteColor(ConsoleColor.Red, "{0}", ex.ToString());
            }
        }

        /// <summary>
        /// Asynchronously updates to index to use geolocation function 
        /// </summary>
        /// <returns></returns>
        private async static Task UpdateIndexGeoScoringProfileAsync()
        {
            try
            {
                ValidateCurrentIndexName();
                var function1 = new DistanceScoringFunction()
                {
                    Boost = 10000,
                    Parameters = new DistanceScoringParameters("mylocation", 150),
                    FieldName = "geolocation",
                    Interpolation = ScoringFunctionInterpolation.Constant
                };

                var scoringProfile1 = new ScoringProfile()
                {
                    Name = "defaultgeo",
                    FunctionAggregation = ScoringFunctionAggregation.Sum,
                    Functions = new List<ScoringFunction> { function1 },
                };

                var eventIndex = (await _searchClient.Indexes.GetAsync(currentIndexName)).Index;
                eventIndex.ScoringProfiles = new List<ScoringProfile> { scoringProfile1 };
                var result = await _searchClient.Indexes.CreateOrUpdateAsync(eventIndex);
                if (result != null
                    && result.Index != null
                    && result.StatusCode == HttpStatusCode.OK)
                    ConsoleUtils.WriteColor(ConsoleColor.Green, "Index {0} updated successfully.", currentIndexName);
            }
            catch (Exception ex)
            {
                ConsoleUtils.WriteColor(ConsoleColor.Red, "{0}", ex.ToString());
            }
        }
    }
}