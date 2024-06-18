using System.Collections;
using OsmSharp.Streams;
using System.Diagnostics;
using OsmSharp.API;

class Program
{
    public static async Task Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Dictionary<long, MapNode> CurrentNodes = [];

        string filePath = @"new-york-latest.osm.pbf";
        CreateDatabase createDatabase = new CreateDatabase();
        await createDatabase.ReturnNodes(filePath); // used to add all nodes and relationships

        // holds the connection to the database

        // foreach (var key in CurrentNodes.Keys)
        // {
        //     driver.AddToDB(CurrentNodes[key]);
        // }

        // WORKED ON DEMO NODE!!!!!!!!!!
        // string uri = "bolt://localhost:7687";
        // string user = "neo4j";
        // string password = "mnNa5Gs7aYuBW7T";
        // Neo4jImplementation driver = new Neo4jImplementation(uri, user, password);
        // MapNode demoNode = new MapNode(123, 21, 22);
        // MapNode demoNode2 = new MapNode(124, 3, 5);
        // demoNode.AddInfo(demoNode2);
        // Console.WriteLine(demoNode.verticesInfo[demoNode2.ID]);
        // await driver.AddNodeToDB(demoNode);
        // await driver.AddNodeToDB(demoNode2);
        // await driver.AddNodeRelationships(demoNode, demoNode2);

        // HelloWorldExample helloWorldExample = new HelloWorldExample(uri, user, password);
        // await helloWorldExample.PrintGreetingAsync("ADD to DB");

        stopwatch.Stop();
        Console.WriteLine("Elapsed Time: {0} milliseconds", stopwatch.ElapsedMilliseconds);
    
    }



}




// // need to move to secret storage
// String password = "mnNa5Gs7aYuBW7T";
// String user = "neo4j";

// private readonly IDriver _driver;

// // connecting to the database
// try {

// }
// catch (Exception e) {
//     Console.WriteLine(e.Message);
//     Console.WriteLine("Error opening Database connection");
// }
// finally {
//     // _driver.dispose();
// }


// var driver = GraphDatabase.Driver("bolt://localhost:7474", AuthTokens.Basic("Username", "Password"));
// var session = driver.AsyncSession();



// await driver.DisposeAsync();





// PedestrianPath.Pedestrian();
// string filePath = @"new-york-latest.osm.pbf";

// // list of the MapNodes
// List<long> nodes = new List<long>();

// // loop 1 - through all the ways 
// // Open the OSM PBF file for reading
// using (var fileStream = File.OpenRead(filePath))
// {
//     // Initialize the PBF source
//     var source = new PBFOsmStreamSource(fileStream);

//     var filtered = source.FilterBox(-73.9861f, 40.7722f,
//     -73.9679f, 40.7603f); // left, top, right, bottom

//     // Enumerate through the OSM data and display it
//     foreach (var osmGeo in filtered)
//     {
//         if (osmGeo.Type == OsmGeoType.Way)
//         {
            
//         }
        
        
        
        
        
        
//         // Display information based on the type of OSM element
//         switch (osmGeo.Type)
//         {
//             case OsmGeoType.Node:
//                 var node = osmGeo as Node;
//                 // Console.WriteLine($"Node: ID={node.Id}, Latitude={node.Latitude}, Longitude={node.Longitude}");
//                 // if (node.Tags != null)
//                 // {
//                 //     foreach (var tag in node.Tags)
//                 //     {
//                 //         Console.WriteLine($"  Tag: {tag.Key}={tag.Value}");
//                 //     }
//                 // } else {
//                 //     Console.WriteLine("No tags");
//                 // }
                
//                 break;

//             case OsmGeoType.Way:
//                 var way = osmGeo as Way;
//                 Console.WriteLine($"Way: ID={way.Id}, NodeCount={way.Nodes?.Length}");
//                 foreach (var point in way.Nodes){
//                     Console.WriteLine(point);
//                 }
//                 if (way.Tags != null)
//                 {
//                     foreach (var tag in way.Tags)
//                     {
//                         Console.WriteLine($"  Tag: {tag.Key}={tag.Value}");
//                     }
//                 } else {
//                     Console.WriteLine("No tags");
//                 }
                    
                
//                 break;

//             case OsmGeoType.Relation:
//                 var relation = osmGeo as Relation;
//                 // Console.WriteLine($"Relation: ID={relation.Id}, MemberCount={relation.Members?.Length}");
//                 break;
//         }
//     }
// }


