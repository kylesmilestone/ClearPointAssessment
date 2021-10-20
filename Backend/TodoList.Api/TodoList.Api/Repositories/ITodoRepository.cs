using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoList.Api.Models;

namespace TodoList.Api.Repositories
{
    public interface ITodoRepository
    {
        Task<IEnumerable<TodoItem>> GetItems();
        Task<TodoItem> GetItem(Guid id);
        Task<int> AddItem(TodoItem item);
        Task<int> UpdateItem(Guid id, TodoItem item);
        Task<bool> ItemIdExists(Guid id);
        Task<bool> ItemDescriptionExists(string description);
    }
}
