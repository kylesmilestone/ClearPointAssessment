using Microsoft.EntityFrameworkCore;
using TodoList.Api.Models;

namespace TodoList.Api.DbContexts
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TodoItem> TodoItems { get; set; }

        // added this for indirection used in unit tests
        public virtual void SetModified(object entity)
        {
            Entry(entity).State = EntityState.Modified;
        }
    }
}
