using Application.Models;

namespace Application.Services;

public interface ICnabParser
{
    CnabLineResult ParseLine(ReadOnlySpan<char> line);
}