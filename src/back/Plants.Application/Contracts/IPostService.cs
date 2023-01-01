using Plants.Application.Commands;
using Plants.Application.Requests;

namespace Plants.Application.Contracts;

public interface IPostService
{
    Task<PostResultItem?> GetBy(long postId);
    Task<PlaceOrderResult> Order(long postId, string city, int mailNumber);
    Task<DeletePostResult> Delete(long postId);
}