public interface IPoint
{
    public PointType Type { get; set; }
    public L L { get; set; }
    public PointSettings Settings { get; set; }
    //public string Serialize();
    //public IPoint Deserialize();
}

