using Neo4j.Driver;
using System.Collections.Concurrent;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.ComponentModel.Design;

public class Neo4jImplementation : IDisposable
{
    private readonly IDriver _driver;

    public int Success = 0;
    public int Failed = 0;

    public int RelSuccess { get; private set; } = 0;
    public int RelFailed { get; private set; } = 0;

    public Neo4jImplementation(string uri, string user, string password)
    {
        _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
    }

    /// <summary>
    /// Adds a given node to the Neo4j Database.
    /// </summary>
    /// <param name="node">MapNode</param>
    /// <returns>Void</returns>
    public async Task AddNodeToDB(MapNode node)
    {
        var query = @"
        MERGE (n:nodes {nodeid: $ID})
        ON CREATE SET n.longitude = $longitude, n.latitude = $latitude
        ON MATCH
            SET n.longitude = CASE WHEN n.longitude = 0 THEN $longitude ELSE n.longitude END,
                n.latitude = CASE WHEN n.latitude = 0 THEN $latitude ELSE n.latitude END
        RETURN n";

        var parameters = new Dictionary<string, object>
        {
            {"ID", node.ID},
            {"latitude", node.Latitude},
            {"longitude", node.Longitude},
        };

        try
        {
            Console.WriteLine("Before adding");
            await using var session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));

            var result = await session.ExecuteWriteAsync(
                async tx =>
            {
                var cursor = await tx.RunAsync(query, parameters);
                var record = await cursor.SingleAsync();
                return await cursor.ToListAsync();
            });

            Console.WriteLine("Node checked and added if it didn't exist!");
            Success += 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("Failed");
            Failed += 1;
        }
    }


    public async Task AddNodeRelationships(MapNode node, MapNode neighbor)
    {
        var query = @"
        MATCH (a:nodes {nodeid: $ID1}), (b:nodes {nodeid: $ID2})
        MERGE (a)-[r1:PATH]->(b)
        ON CREATE SET r1.distance = $distance, r1.direction = $direction, r1.quietscore = $quietscore
        MERGE (b)-[r2:PATH]->(a)
        ON CREATE SET r2.distance = $distance, r2.direction = $direction, r2.quietscore = $quietscore
        RETURN a, b, r1, r2";

        NodeInfo nodeInfo = (NodeInfo)node.verticesInfo[neighbor.ID];

        var parameters = new Dictionary<string, object>
        {
            {"ID1", node.ID},
            {"ID2", neighbor.ID},
            {"distance", nodeInfo.Distance},
            {"direction", nodeInfo.Direction},
            {"quietscore", nodeInfo.QuietScore}
        };

        try
        {
            Console.WriteLine("Before adding");
            await using var session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));

            var result = await session.ExecuteWriteAsync(
                async tx =>
            {
                var cursor = await tx.RunAsync(query, parameters);
                var record = await cursor.SingleAsync();
                return await cursor.ToListAsync();
            });

            Console.WriteLine("Relationship checked and added/updated!");
            RelSuccess += 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.WriteLine("Failed");
            RelFailed += 1;
        }
    }
    // {
    //     Console.WriteLine("In function");

    //     var checkNodeExistenceQuery = @"
    //     MATCH (n:Node {id: $ID})
    //     RETURN n";

    //     var createNodeQuery = @"
    //     CREATE (n:Node {id: $ID, longitude: $longitude, latitude: $latitude})
    //     RETURN n";

    //     var createRelationshipQuery = @"
    //     MATCH (n:Node {id: $ID}), (neighbor:Node {id: $vertice})
    //     CREATE (n)-[:NEIGHBOR {distance: $distance, direction: $direction}]->(neighbor)";

    //     await using var session = _driver.AsyncSession();

    //     var semaphore = new SemaphoreSlim(10);  // Adjust the number to limit concurrent tasks
    //     var tasks = new ConcurrentBag<Task>();

    //     try
    //     {
    //         // Check if the node already exists
    //         var nodeExists = false;
    //         await session.ExecuteReadAsync(async tx =>
    //         {
    //             var result = await tx.RunAsync(checkNodeExistenceQuery, new { ID = node.ID });
    //             nodeExists = await result.FetchAsync();
    //         });

    //         if (!nodeExists)
    //         {
    //             // Create the node if it does not exist
    //             await session.ExecuteWriteAsync(async tx =>
    //             {
    //                 var parameters = new { ID = node.ID, longitude = node.Longitude, latitude = node.Latitude };
    //                 Console.WriteLine($"Running createNodeQuery with parameters: {parameters}");
    //                 await tx.RunAsync(createNodeQuery, parameters);
    //                 Console.WriteLine("Node Added");
    //             });
    //         }
    //         else
    //         {
    //             Console.WriteLine("Node already exists");
    //         }

    //         // Create relationships for each neighbor
    //         foreach (var key in node.verticesInfo.Keys)
    //         {
    //             await semaphore.WaitAsync();
    //             var task = Task.Run(async () =>
    //             {
    //                 try
    //                 {
    //                     await session.ExecuteWriteAsync(async tx =>
    //                     {
    //                         NodeInfo temp = (NodeInfo)node.verticesInfo[key];
    //                         double distance = temp.distance;
    //                         byte direction = temp.direction;
    //                         var parameters = new { ID = node.ID, vertice = key, distance, direction };
    //                         Console.WriteLine($"Running createRelationshipQuery with parameters: {parameters}");
    //                         await tx.RunAsync(createRelationshipQuery, parameters);
    //                         Console.WriteLine("Relationship added");
    //                     });
    //                 }
    //                 catch (Exception ex)
    //                 {
    //                     Console.WriteLine($"Error creating relationship: {ex}");
    //                 }
    //                 finally
    //                 {
    //                     semaphore.Release();
    //                 }
    //             });
    //             tasks.Add(task);
    //         }

    //         await Task.WhenAll(tasks);
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error adding node to database: {ex}");
    //     }
    //     finally
    //     {
    //         await session.CloseAsync();
    //     }
    // }




    public void Dispose()
    {
        _driver?.Dispose();
    }
}