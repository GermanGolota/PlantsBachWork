using Plants.Aggregates.PlantStocks;

namespace Plants.Aggregates.PlantInstructions;

internal class CreateEditInstructionCommandHandler : ICommandHandler<CreateInstructionCommand>, ICommandHandler<EditInstructionCommand>
{
    private readonly FileUploader _uploader;

    public CreateEditInstructionCommandHandler(FileUploader uploader)
    {
        _uploader = uploader;
    }

    public Task<CommandForbidden?> ShouldForbidAsync(CreateInstructionCommand command, IUserIdentity user, CancellationToken token = default) =>
        user.HasAnyRoles(Producer, Manager).ToResultTask();

    public async Task<IEnumerable<Event>> HandleAsync(CreateInstructionCommand command, CancellationToken token = default)
    {
        var url = await Upload(command.Metadata.Aggregate.Id, command.CoverImage, token);
        return new[]
        {
            new InstructionCreatedEvent(EventFactory.Shared.Create<InstructionCreatedEvent>(command), command.Instruction, url, command.Metadata.UserName, command.Metadata.Aggregate.Id)
        };
    }

    public Task<CommandForbidden?> ShouldForbidAsync(EditInstructionCommand command, IUserIdentity user, CancellationToken token = default) =>
         user.HasAnyRoles(Producer, Manager).ToResultTask();

    public async Task<IEnumerable<Event>> HandleAsync(EditInstructionCommand command, CancellationToken token = default)
    {
        var url = await Upload(command.Metadata.Aggregate.Id, command.CoverImage, token);
        return new[]
        {
            new InstructionEditedEvent(EventFactory.Shared.Create<InstructionCreatedEvent>(command), command.Instruction, url)
        };
    }

    private async Task<string> Upload(Guid aggregateId, byte[] cover, CancellationToken token = default)
    {
        string url;
        if (cover is not null)
        {
            url = await _uploader.UploadIntructionCover(aggregateId, cover, token);
        }
        else
        {
            url = null;
        }

        return url;
    }

}
