using Xunit;
using Moq;
using TodoList.Api.Repositories;
using TodoList.Api.Models;
using System.Threading.Tasks;
using System;
using TodoList.Api.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace TodoList.Api.UnitTests.Repositories
{
    public class TodoRepositoryTests
    {
        private readonly static Guid _id1 = Guid.NewGuid();
        private readonly TodoItem _todoItem1;
        private readonly Mock<TodoContext> _mockTodoContext;
        private readonly Mock<DbSet<TodoItem>> _mockDbSet;
        private readonly TodoRepository _todoRepository;

        public TodoRepositoryTests()
        {
            _mockDbSet = new Mock<DbSet<TodoItem>>();

            var optionsBuilder = new DbContextOptionsBuilder<TodoContext>()
               .UseSqlServer("--REDACTED CONNECTION STRING--",
               x => x.MigrationsHistoryTable("__EFMigrationHistory", "catalog"));
            _mockTodoContext = new Mock<TodoContext>(optionsBuilder.Options);
            _mockTodoContext.Setup(c => c.Set<TodoItem>()).Returns(_mockDbSet.Object);

            _mockTodoContext.Setup(m => m.TodoItems).Returns(_mockDbSet.Object);
            _todoRepository = new TodoRepository(_mockTodoContext.Object);

            _todoItem1 = new TodoItem() { Id = _id1, Description = "Description1", IsCompleted = true};
        }

        [Fact]
        public async Task GetItem_ReturnsTodoItem()
        {
            // Arrange
            _mockDbSet.Setup(o => o.FindAsync(It.IsAny<Guid>())).ReturnsAsync(_todoItem1);

            // Act
            var todoItem = await _todoRepository.GetItem(_id1);

            //Assert
            Assert.Equal(_id1, todoItem.Id);
        }

        [Fact]
        public async Task UpdateItem_CallsSaveChangesAsync()
        {
            // Arrange
            _mockTodoContext.Setup(o => o.SetModified(It.IsAny<TodoItem>()));

            // Act
            var todoItems = await _todoRepository.UpdateItem(Guid.NewGuid(), new TodoItem());

            //Assert
            _mockTodoContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddItem_CallsSaveChangesAsync()
        {
            // Arrange
            _mockTodoContext.Setup(o => o.SetModified(It.IsAny<TodoItem>()));

            // Act
            var todoItems = await _todoRepository.AddItem(new TodoItem());

            //Assert
            _mockTodoContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
