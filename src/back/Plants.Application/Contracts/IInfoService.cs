using Plants.Application.Requests;

namespace Plants.Application.Contracts;

public interface IInfoService
{
    Task<DictsResult> GetDicts();
    Task<IEnumerable<PersonAddress>> GetMyAddresses();
}
