namespace CDRSandbox.Repositories.Interfaces;

public interface ICdrItemEntity
{
    public string CallerId { get; set; }
    public string Recipient { get; set; }
    public DateTime CallDate { get; set; }
    public string EndTime { get; set; }
    public uint Duration { get; set; }
    public float Cost { get; set; }
    public string Reference { get; set; }
    public string Currency { get; set; }
    public int? Type { get; set; } // TODO: check if int or string
}