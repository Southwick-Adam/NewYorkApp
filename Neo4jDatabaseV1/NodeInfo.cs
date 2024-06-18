class NodeInfo(double distance, byte direction, int quietScore)
{
    public double Distance { get; set; } = distance;
    public byte Direction { get; set; } = direction;
    public int QuietScore { get; set; } = quietScore;

    public override string ToString()
    {
        return $"distance: {this.Distance},\nDirection: {this.Direction}, \n QuietScore: {this.QuietScore}";
    }
}

