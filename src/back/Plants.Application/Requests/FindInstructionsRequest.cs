using MediatR;

namespace Plants.Application.Requests;

public record FindInstructionsRequest(long GroupId, string? Title, string? Description) : IRequest<FindInstructionsResult>;
public record FindInstructionsResult(List<FindInstructionsResultItem> Items);
public record FindInstructionsResultItem(long Id, string Title, string Description, bool HasCover)
{
    public FindInstructionsResultItem() : this(-1, "", "", false)
    {

    }
}
