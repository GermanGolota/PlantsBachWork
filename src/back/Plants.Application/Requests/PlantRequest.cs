using Humanizer;
using MediatR;

namespace Plants.Application.Requests;

public record PlantRequest(int Id) : IRequest<PlantResult>;

public record PlantResult(bool Exists, PlantResultDto? Item)
{
    public PlantResult(PlantResultDto item) : this(true, item)
    {

    }

    public PlantResult() : this(false, null)
    {

    }
}

public record PlantResultDto(string PlantName, string Description, int GroupId,
    int SoilId, int[] Images, int[] Regions)
{
    //for decoder
    public PlantResultDto() : this("", "",
        -1, -1, Array.Empty<int>(), Array.Empty<int>())
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
