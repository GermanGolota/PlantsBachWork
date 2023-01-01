using Plants.Application.Requests;

namespace Plants.Application.Contracts;

public interface IInstructionsService
{
    Task<IEnumerable<FindInstructionsResultItem>> GetFor(long GroupId, string? Title, string? Description);
    Task<GetInstructionResultItem?> GetBy(long Id);
    Task<int> Create(long GroupId, string Text, string Title, string Description, byte[]? CoverImage);
    Task Edit(long InstructionId, long GroupId, string Text, string Title, string Description, byte[]? CoverImage);
}
