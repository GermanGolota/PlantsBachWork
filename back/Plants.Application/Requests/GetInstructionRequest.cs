using MediatR;

namespace Plants.Application.Requests
{
    public record GetInstructionRequest(int Id) : IRequest<GetInstructionResult>;
    public record GetInstructionResult(bool Exists, GetInstructionResultItem Item);
    public record GetInstructionResultItem(int Id, string Title, string Description,
        string InstructionText, bool HasCover)
    {
        //decoder
        public GetInstructionResultItem() : this (-1, "", "", "", false)
        {

        }
    }
}
