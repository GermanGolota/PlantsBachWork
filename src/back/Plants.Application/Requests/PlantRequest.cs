using Humanizer;
using MediatR;

namespace Plants.Application.Requests;

public record PlantRequest(long Id) : IRequest<PlantResult>;

public record PlantResult(bool Exists, PlantResultDto? Item)
{
    public PlantResult(PlantResultDto item) : this(true, item)
    {

    }

    public PlantResult() : this(false, null)
    {

    }
}

public record PlantResultDto(string PlantName, string Description, long GroupId,
    long SoilId, long[] Images, long[] Regions)
{
    //for decoder
    public PlantResultDto() : this("", "",
        -1, -1, Array.Empty<long>(), Array.Empty<long>())
    {

    }

    private DateTime created;
    public DateTime Created
    {
        get { return created; }
        set
        {
            created = value;
            CreatedHumanDate = value.Humanize();
            CreatedDate = value.ToShortDateString();
        }
    }
    public string CreatedHumanDate { get; set; }
    public string CreatedDate { get; set; }

}

public record PlantResult2(bool Exists, PlantResultDto2? Item)
{
    public PlantResult2(PlantResultDto2 item) : this(true, item)
    {

    }

    public PlantResult2() : this(false, null)
    {

    }
}

public record PlantResultDto2(string PlantName, string Description, string GroupId,
    string SoilId, string[] Images, string[] Regions)
{
    //for decoder
    public PlantResultDto2() : this("", "",
        "", "", Array.Empty<string>(), Array.Empty<string>())
    {

    }

    private DateTime created;
    public DateTime Created
    {
        get { return created; }
        set
        {
            created = value;
            CreatedHumanDate = value.Humanize();
            CreatedDate = value.ToShortDateString();
        }
    }
    public string CreatedHumanDate { get; set; }
    public string CreatedDate { get; set; }

}
