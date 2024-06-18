using System.Collections;
using Microsoft.VisualBasic;
using OsmSharp;
using OsmSharp.Streams;

class CreateDatabase
{
    private string uri = "bolt://localhost:7687";
    private string user = "neo4j";
    private string password = "mnNa5Gs7aYuBW7T";
    public CreateDatabase(){}
    
    /// <summary>
    /// Used to get a dictionary of all roads that are pedestrian accessible in the osm.pbf file
    /// </summary>
    /// <param name="filePath">Path to the file in osm.pbf format </param>
    /// <returns>Dictionary long:MapNode</returns>
    public async Task ReturnNodes(string filePath)
    {
        Neo4jImplementation neo4JImplementation = new Neo4jImplementation(uri, user, password);
        // dictionary of the MapNodes for faster lookup on loops
        Dictionary<long, MapNode> nodeTable = [];

        // list of the ways
        List<Way> ways = new List<Way>();
        List<Node> nodesToProcess = new List<Node>();

        int zeroLatLongCounter = 0;
        int populatedLatLongCounter =0;


        var allTags = new List<string>();
        var allTagsAndValues = new Dictionary<string, List<string>>();
        var allHighwayTags = new List<string>();


        // Open the OSM PBF file for reading
        using (var fileStream = File.OpenRead(filePath))
        {
            // Initialize the PBF source
            var source = new PBFOsmStreamSource(fileStream);

            // TODO: Bring this into the function call, allowing to chose the area the function works on
            var filtered = source.FilterBox(-73.9861f, 40.7722f, -73.9679f, 40.7603f); // left, top, right, bottom

            var ignoredTags = new List<string> {"freeway", "service"};
            // Enumerate through the OSM data and display it
            foreach (var osmGeo in filtered)
            {
                if (osmGeo.Type == OsmGeoType.Way)
                {
                    var tempWay = osmGeo as Way;

                    // used when generating the txt file of tags
                    // foreach (var tag in tempWay.Tags)
                    // {
                    //     if (tag.Key == "highway" && !allHighwayTags.Contains(tag.Value))
                    //     {
                    //         allHighwayTags.Add(tag.Value);
                    //     }

                    //     if (allTagsAndValues.ContainsKey(tag.Key))
                    //     {
                    //         if (!allTagsAndValues[tag.Key].Contains(tag.Value))
                    //         {
                    //             allTagsAndValues[tag.Key].Add(tag.Value);
                    //         }
                    //     }
                    //     else
                    //     {
                    //         allTagsAndValues.Add(tag.Key, new List<string> {tag.Value});
                    //     }
                    // }

                    foreach (var tag in tempWay.Tags)
                    {
                    if (tag.Key == "highway" && !ignoredTags.Contains(tag.Value)) // need to revisit these filters to add more road types
                        {
                            ways.Add(tempWay);
                            break;
                        }
                    }
                }
                else if (osmGeo.Type == OsmGeoType.Node)
                {
                    nodesToProcess.Add(osmGeo as Node);
                }
            }
        }
            
        Console.WriteLine("In loop 1");
        // loop 1 - through all the ways - adding nodes to use dictionary 
        foreach (var way in ways)
        {
            foreach (var node in way.Nodes)
            {
                if (!nodeTable.ContainsKey(node))
                {
                    nodeTable.Add(node, new MapNode(node));
                }
            } 
        }

        Console.WriteLine("In loop 2");
        // second loop: Loops through the Nodes and adds any used Nodes Coordinates - rewriting them in the dictionary
        foreach (var node in nodesToProcess)
        {
            if (nodeTable.ContainsKey(node.Id.Value))
            {
                if (node.Longitude.HasValue && node.Latitude.HasValue)
                {
                    MapNode tempNode = new MapNode(node.Id.Value, node.Longitude.Value, node.Latitude.Value);
                    nodeTable[node.Id.Value] = tempNode;
                    await neo4JImplementation.AddNodeToDB(tempNode);
                }
            }
        }

        Console.WriteLine("In loop 3");
        // final loop: again looping through the Ways - adding links between Nodes
        foreach (var way in ways)
        {
            if (way.Tags != null)
            {
                foreach (var tag in way.Tags)
                {
                    if (tag.Key == "highway" && tag.Value != "freeway")
                    {
                        bool edge = true;
                        MapNode currMapNode;
                        MapNode pastMapNode = null;

                        foreach (var point in way.Nodes)
                        {
                            try
                            {
                                currMapNode = (MapNode)nodeTable[point];

                                // used to check that the amount of nodes with no latitude or longitude assigned
                                if (currMapNode.Latitude == 0)
                                {
                                    zeroLatLongCounter += 1;
                                }
                                else
                                {
                                    populatedLatLongCounter += 1;
                                }
                                
                                if (edge==false){
                                    currMapNode.AddInfo(pastMapNode);
                                    await neo4JImplementation.AddNodeRelationships(currMapNode, pastMapNode);
                                    pastMapNode = currMapNode;

                                } else if (edge == true){
                                    pastMapNode = (MapNode)nodeTable[point];
                                    edge = false;
                                }
                            }
                            catch (KeyNotFoundException)
                            {
                                Console.WriteLine($"Node: {point} not found in the nodeTable.");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                            
                        }
                        break;
                    }
                }
            }
        }
        
        Console.WriteLine($"There are {zeroLatLongCounter} nodes with a latitude of 0.");
        Console.WriteLine($"There are {populatedLatLongCounter} nodes with a latitude that is populated.");
        Console.WriteLine(nodesToProcess.Count);
        Console.WriteLine(ways.Count);
        Console.WriteLine(nodeTable.Keys.Count);
        Console.WriteLine("Nodes created successfully: "+neo4JImplementation.Success);
        Console.WriteLine("Nodes not created successfully: "+neo4JImplementation.Failed);
        Console.WriteLine("Rels created successfully: "+neo4JImplementation.RelSuccess);
        Console.WriteLine("Rels not created successfully: "+neo4JImplementation.RelFailed);

        Console.WriteLine("Highway tags: " );
        foreach (string tag in allHighwayTags)
        {
            Console.WriteLine(tag);
        }

        // Used to create a txt file of all the tags in the area 
        // string currentDirectory = Directory.GetCurrentDirectory();
        // string relativeFilePath = Path.Combine(currentDirectory, "Tags.txt");

        // using (StreamWriter writer = new StreamWriter(relativeFilePath))
        // {
        //     foreach (string tag in allTagsAndValues.Keys)
        //     {
        //         var tagList = allTagsAndValues[tag];
        //         foreach (var item in tagList)
        //         {
        //             writer.WriteLine($"Tag: {tag}, Item: {item}"); 
        //         }
        //     }
        // }

        // foreach (var key in nodeTable.Keys)
        // {
        //     await neo4JImplementation.AddNodeToDB(nodeTable[key]);
        // }
        
    }
}


