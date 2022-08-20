using MediatR;
using System.Collections.Generic;

namespace Plants.Application.Requests
{
    public record FindInstructionsRequest(int GroupId, string? Title, string? Description) : IRequest<FindInstructionsResult>;
    public record FindInstructionsResult(List<FindInstructionsResultItem> Items);
    public record FindInstructionsResultItem(int Id, string Title, string Description, bool HasCover)
    {
        public FindInstructionsResultItem() : this(-1, "", "", false)
        {

        }
    }
}
