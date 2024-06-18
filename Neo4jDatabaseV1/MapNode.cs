using System.Collections.Generic;
using System.Collections;
using Neo4j.Driver;

/// <summary>
/// used for testing if the MapNode Concept worked
///</summary>
public class MapNode
{
    public long ID{get;}
    public double Latitude {get;}
    public double Longitude {get;}
    public MapNode(long ID, double latitude, double longitude) {
        this.ID = ID;
        this.Latitude = latitude;
        this.Longitude = longitude;
    }

    public MapNode(long ID){
        this.ID = ID;
    }
    
    public List<long> vertices = [];

    // create hashtable structured as neighborID:nodeInfo(distance, direction, QuietScore) 
    public Hashtable verticesInfo = new Hashtable();

    public void AddInfo(MapNode node){
        if (!verticesInfo.ContainsKey(node.ID))
        {
            double distance = this.NodeDistance(node);
            byte direction = this.BearingByte(node);
            Random rnd = new();
            int quietScore = rnd.Next(0,11);

            NodeInfo nodeInfo = new(distance, direction, quietScore);
            
            verticesInfo.Add(node.ID, nodeInfo);
            vertices.Add(node.ID);
        }
        // TODO: need to add an else, checking that the original addition had the correct values - check that it wasn't long/lat of 0.
    }


    /// <summary>
    /// used to check the distance from the current node to the desired node
    /// </summary>
    public double NodeDistance(MapNode node)
    {
        double lat2 = node.Latitude;
        double lon2 = node.Longitude;
        const double R = 6371e3; // Earth's radius in meters
        var lat1Rad = ToRadians(Latitude);
        var lat2Rad = ToRadians(lat2);
        var deltaLat = ToRadians(lat2 - Latitude);
        var deltaLon = ToRadians(lon2 - Longitude);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
            Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
            Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // Distance in meters
    }


    public double CalculateBearing(MapNode node){
        // Convert degrees to radians
        double lat1Rad = ToRadians(Latitude);
        double lon1Rad = ToRadians(Longitude);
        double lat2Rad = ToRadians(node.Latitude);
        double lon2Rad = ToRadians(node.Longitude);

        // Calculate the difference in longitude
        double deltaLon = lon2Rad - lon1Rad;

        // Calculate y and x
        double y = Math.Sin(deltaLon) * Math.Cos(lat2Rad);
        double x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                   Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(deltaLon);

        // Calculate the bearing in radians
        double bearingRad = Math.Atan2(y, x);

        // Convert bearing from radians to degrees
        double bearingDeg = ToDegrees(bearingRad);

        // Normalize to 0-360 degrees
        bearingDeg = (bearingDeg + 360) % 360;

        return bearingDeg;
    }

    /// <summary>
    /// Used to calculate the direction. 
    /// Returns a byte NSEW/0000 
    /// </summary>
    public byte BearingByte(MapNode node){
        double bearingDeg = this.CalculateBearing(node);

        if (bearingDeg <= 30 || bearingDeg > 330){
            return 8; // 1000
        } else if (bearingDeg <= 60) {
            return 10; // 1010
        } else if (bearingDeg <= 120){
            return 2; // 0010
        } else if (bearingDeg <= 150){
            return 6; // 0110
        } else if (bearingDeg <= 210){
            return 4; // 0100
        } else if(bearingDeg <= 240){
            return 5; // 0101
        } else if (bearingDeg <= 300){
            return 1; // 0001
        } else if (bearingDeg <= 330){
            return 9; // 1001
        } else {
            throw new Exception("Error in bearing point calculation");
        }
    }

    public override string ToString()
    {
        return $"ID: {this.ID},\nLong, Lat: {this.Longitude}, {this.Latitude}";
    }

    // Used for calculating bearing point
    private static double ToDegrees(double radians)
    {
        return radians * 180 / Math.PI;
    }
    public static double ToRadians(double degrees)
    {
    return degrees * (Math.PI / 180);
    }
}
