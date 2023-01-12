using MediatR;

namespace Plants.Application.Requests;

public record GetInstructionRequest(long Id) : IRequest<GetInstructionResult>;
public record GetInstructionResult(bool Exists, GetInstructionResultItem Item);
public record GetInstructionResultItem(long Id, string Title, string Description,
    string InstructionText, bool HasCover, long PlantGroupId)
{
    //decoder
    public GetInstructionResultItem() : this (-1, "", "", "", false, -1)
    {

    }
}

public record GetInstructionResult2(bool Exists, GetInstructionResultItem2 Item);
public record GetInstructionResultItem2(Guid Id, string Title, string Description,
    string InstructionText, bool HasCover, string PlantGroupId)
{
    //decoder
    public GetInstructionResultItem2() : this(Guid.NewGuid(), "", "", "", false, "-1")
    {

    }
}