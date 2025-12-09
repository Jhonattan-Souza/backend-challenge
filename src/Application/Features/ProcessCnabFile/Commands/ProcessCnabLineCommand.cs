using FastEndpoints;

namespace Application.Features.ProcessCnabFile.Commands;

public sealed record ProcessCnabLineCommand(string Line, int LineNumber) : ICommand;
