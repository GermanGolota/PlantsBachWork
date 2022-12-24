using Plants.Application.Requests;

namespace Plants.Application.Contracts;

public interface IInstructionsService
{
    Task<IEnumerable<FindInstructionsResultItem>> GetFor(int GroupId, string? Title, string? Description);
    Task<GetInstructionResultItem?> GetBy(int Id);
    Task<int> Create(int GroupId, string Text, string Title, string Description, byte[]? CoverImage);
    Task Edit(int InstructionId, int GroupId, string Text, string Title, string Description, byte[]? CoverImage);
}
