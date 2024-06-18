using System.Collections;
using OsmSharp.Streams;
using System.Diagnostics;
using OsmSharp.API;

class DatabaseProgram
{
    public static async Task Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Dictionary<long, MapNode> CurrentNodes = [];

        string filePath = @"new-york-latest.osm.pbf";
        CreateDatabase createDatabase = new CreateDatabase();
        await createDatabase.ReturnNodes(filePath); // used to add all nodes and relationships


        stopwatch.Stop();
        Console.WriteLine("Elapsed Time: {0} milliseconds", stopwatch.ElapsedMilliseconds);
    
    }



}
