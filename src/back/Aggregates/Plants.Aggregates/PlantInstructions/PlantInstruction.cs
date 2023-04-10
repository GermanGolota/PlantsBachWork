using System;

namespace Plants.Aggregates;

[Allow(Consumer, Read)]
[Allow(Producer, Read)]
[Allow(Producer, Write)]
[Allow(Manager, Read)]
[Allow(Manager, Write)]
public class PlantInstruction : AggregateBase,
    IEventHandler<InstructionCreatedEvent>, IEventHandler<InstructionEditedEvent>,
    IDomainCommandHandler<CreateInstructionCommand>, IDomainCommandHandler<EditInstructionCommand>
{
    public PlantInstruction(Guid id) : base(id)
    {
    }

    public InstructionModel Information { get; private set; }
    public Picture Cover { get; private set; }

    public void Handle(InstructionCreatedEvent @event)
    {
        Information = @event.Instruction;
        Cover = @event.CoverImage;
    }

    public void Handle(InstructionEditedEvent @event)
    {
        Information = @event.Instruction;
        if (@event.CoverImage is not null)
        {
            Cover = @event.CoverImage;
        }
    }

    public CommandForbidden? ShouldForbid(CreateInstructionCommand command, IUserIdentity user) =>
         user.HasAnyRoles(Producer, Manager);

    public IEnumerable<Event> Handle(CreateInstructionCommand command)
    {
        return new[]
        {
            new InstructionCreatedEvent(EventFactory.Shared.Create<InstructionCreatedEvent>(command), command.Instruction, command.CoverImage, command.Metadata.UserName, command.Metadata.Aggregate.Id)
        };
    }

    public CommandForbidden? ShouldForbid(EditInstructionCommand command, IUserIdentity user) =>
        user.HasAnyRoles(Producer, Manager);

    public IEnumerable<Event> Handle(EditInstructionCommand command)
    {
        return new[]
        {
            new InstructionEditedEvent(EventFactory.Shared.Create<InstructionEditedEvent>(command), command.Instruction, command.CoverImage)
        };
    }
}
