using MediatR;

namespace Plants.Application.Commands;

public record DeletePostCommand(long PostId) : IRequest<DeletePostResult>;
public record DeletePostResult(bool Deleted);
