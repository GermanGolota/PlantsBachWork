﻿using Plants.Application.Commands;
using Plants.Application.Requests;
using System.Threading.Tasks;

namespace Plants.Application.Contracts
{
    public interface IPostService
    {
        Task<PostResultItem?> GetBy(int postId);
        Task<PlaceOrderResult> Order(int postId, string city, int mailNumber);
        Task<DeletePostResult> Delete(int postId);
    }
}