using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Api.DbContexts;
using TodoList.Api.Models;

namespace TodoList.Api.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly TodoContext _context;
        public TodoRepository(TodoContext context)
        {
            _context = context;
        }

        public async Task<TodoItem> GetItem(Guid id)
        {
            return await _context.TodoItems.FindAsync(id);
        }

        public async Task<IEnumerable<TodoItem>> GetItems()
        {
            return await _context.TodoItems.Where(x => !x.IsCompleted).ToListAsync();
        }

        public async Task<int> UpdateItem(Guid id, TodoItem item)
        {
            _context.SetModified(item);
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> ItemIdExists(Guid id)
        {
            return await _context.TodoItems.AnyAsync(x => x.Id == id);
        }

        public async Task<bool> ItemDescriptionExists(string description)
        {
            return await _context.TodoItems
                   .AnyAsync(x => x.Description.ToLowerInvariant() == description.ToLowerInvariant() && !x.IsCompleted);
        }

        public async Task<int> AddItem(TodoItem item)
        {
            _context.TodoItems.Add(item);
            return await _context.SaveChangesAsync();
        }
    }
}
