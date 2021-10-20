using Xunit;
using Moq;
using TodoList.Api.Repositories;
using TodoList.Api.Models;
using TodoList.Api.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace TodoList.Api.UnitTests.Controllers
{
    public class TodoItemsControllerTests
    {
        private readonly Mock<ITodoRepository> _mockToDoRepository;
        private readonly TodoItemsController _controller;
        private readonly IEnumerable<TodoItem> _todoItems;
        private readonly static Guid _id1 = Guid.NewGuid();
        private readonly static Guid _id2 = Guid.NewGuid();
        private readonly static Guid _id3 = Guid.NewGuid();
        delegate void AddItemCallback(TodoItem todoItem);

        public TodoItemsControllerTests()
        {
            _mockToDoRepository = new Mock<ITodoRepository>();
            _controller = new TodoItemsController(_mockToDoRepository.Object);

            _todoItems = new List<TodoItem>() {
                new TodoItem() { Id = _id1, Description = "Description1" },
                new TodoItem() { Id = _id2, Description = "Description2" },
                new TodoItem() { Id = _id3, Description = "Description3" }
            };
        }

        #region GetTodoItems
        [Fact]
        public async Task GetTodoItems_ReturnsOKObjectResultWithData()
        {
            // Arrange
            _mockToDoRepository.Setup(r => r.GetItems()).ReturnsAsync(_todoItems);

            // Act
            var actionResult = await _controller.GetTodoItems();

            //Assert
            var okObjectResult = actionResult as OkObjectResult;
            Assert.NotNull(okObjectResult);

            var items = okObjectResult.Value as IEnumerable<TodoItem>;
            Assert.NotNull(items);
            Assert.Equal(_todoItems.Count(), items.Count());
        }
        #endregion

        #region GetTodoItem
        [Fact]
        public async Task GetTodoItem_ResultIsNotNull_ReturnsOKObjectResultWithCorrectToDoItem()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockToDoRepository.Setup(r => r.GetItem(It.IsAny<Guid>())).ReturnsAsync(new TodoItem() { Id = id });

            // Act
            var actionResult = await _controller.GetTodoItem(Guid.NewGuid());

            //Assert
            var okObjectResult = actionResult as OkObjectResult;
            Assert.NotNull(okObjectResult);

            var item = okObjectResult.Value as TodoItem;
            Assert.NotNull(item);
            Assert.Equal(id, item.Id);
        }

        [Fact]
        public async Task GetTodoItem_ResultIsNull_ReturnsNotFoundResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockToDoRepository.Setup(r => r.GetItem(It.IsAny<Guid>())).Returns(Task.FromResult<TodoItem>(null));

            // Act
            var actionResult = await _controller.GetTodoItem(Guid.NewGuid());

            //Assert
            var notFoundResult = actionResult as NotFoundResult;
            Assert.NotNull(notFoundResult);
        }
        #endregion

        #region PutTodoItem
        [Fact]
        public async Task PutTodoItem_IdsDontMatch_ReturnsBadRequestResult()
        {
            // Act
            var actionResult = await _controller.PutTodoItem(Guid.NewGuid(), new TodoItem() { Id = Guid.NewGuid() });

            //Assert
            var badRequestResult = actionResult as BadRequestResult;
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task PutTodoItem_ItemDoesNotExisit_ThrowsCorrectException()
        {
            // Arrange 
            var exceptionMessage = "Test Exception";
            var exception = new IOException(exceptionMessage);
            var id = Guid.NewGuid();
            _mockToDoRepository.Setup(r => r.UpdateItem(It.IsAny<Guid>(), It.IsAny<TodoItem>())).Throws(exception);
            _mockToDoRepository.Setup(r => r.ItemIdExists(It.IsAny<Guid>())).ReturnsAsync(false);

            // Act & Asset
            try
            {
                var actionResult = await _controller.PutTodoItem(id, new TodoItem() { Id = id });
            }
            catch (IOException e)
            {
                if (e.Message == exceptionMessage)
                {
                    Assert.True(true);
                }
                else
                {
                    // same type of exception but wrong message
                    Assert.True(false, "Wrong Esception Message");
                }
            }
            catch (Exception e)
            {
                // wrong type of exception
                Assert.True(false, "Wrong Type of Exception");
            }
        }

        [Fact]
        public async Task PutTodoItem_DbUpdateConcurrencyExceptionAndItemExists_ThrowsCorrectException()
        {
            // Arrange 
            var exceptionMessage = "Test Exception";
            var exception = new DbUpdateConcurrencyException(exceptionMessage);
            var id = Guid.NewGuid();
            _mockToDoRepository.Setup(r => r.UpdateItem(It.IsAny<Guid>(), It.IsAny<TodoItem>())).Throws(exception);
            _mockToDoRepository.Setup(r => r.ItemIdExists(It.IsAny<Guid>())).ReturnsAsync(true);

            // Act & Asset
            try
            {
                var actionResult = await _controller.PutTodoItem(id, new TodoItem() { Id = id });
            }
            catch (DbUpdateConcurrencyException e)
            {
                if (e.Message == exceptionMessage)
                {
                    Assert.True(true);
                }
                else
                {
                    // same type of exception but wrong message
                    Assert.True(false, "Wrong Esception Message");
                }
            }
            catch (Exception e)
            {
                // wrong type of exception
                Assert.True(false, "Wrong Type of Exception");
            }
        }

        [Fact]
        public async Task PutTodoItem_DbUpdateConcurrencyExceptionAndItemDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange 
            var exceptionMessage = "Test Exception";
            var exception = new DbUpdateConcurrencyException(exceptionMessage);
            var id = Guid.NewGuid();
            _mockToDoRepository.Setup(r => r.UpdateItem(It.IsAny<Guid>(), It.IsAny<TodoItem>())).Throws(exception);
            _mockToDoRepository.Setup(r => r.ItemIdExists(It.IsAny<Guid>())).ReturnsAsync(false);

            // Act
            var actionResult = await _controller.PutTodoItem(id, new TodoItem() { Id = id });

            //Assert
            var notFoundResult = actionResult as NotFoundResult;
            Assert.NotNull(notFoundResult);
        }
        #endregion

        #region PostTodoItem
        [Fact]
        public async Task PostTodoItem_TodoItemIsNull_ReturnsBadRequestObjectResultWithCorrectMessage()
        {

            // Act
            var actionResult = await _controller.PostTodoItem(null);

            //Assert
            var badRequestResult = actionResult as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal("Description is required", badRequestResult.Value);
        }

        [Fact]
        public async Task PostTodoItem_DiscriptionIsNullOrEmpty_ReturnsBadRequestObjectResultWithCorrectMessage()
        {

            // Act
            var actionResult = await _controller.PostTodoItem(new TodoItem() { Description = string.Empty });
            var actionResult2 = await _controller.PostTodoItem(new TodoItem() { Description = null });

            //Assert
            var badRequestResult = actionResult as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal("Description is required", badRequestResult.Value);
            var badRequestResult2 = actionResult2 as BadRequestObjectResult;
            Assert.NotNull(badRequestResult2);
            Assert.Equal("Description is required", badRequestResult2.Value);
        }

        [Fact]
        public async Task PostTodoItem_DiscriptionExists_ReturnsBadRequestObjectResultWithCorrectMessage()
        {
            // Arrange
            _mockToDoRepository.Setup(r => r.ItemDescriptionExists(It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var actionResult = await _controller.PostTodoItem(new TodoItem() { Description = "description" });

            //Assert
            var badRequestResult = actionResult as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal("Description already exists", badRequestResult.Value);
        }

        [Fact]
        public async Task PostTodoItem_DiscriptionDoesNotExistsAndNotEmpty_ReturnsCreatedAtActionResultWithNewIdAndSavedItem()
        {
            // Arrange
            var id = Guid.NewGuid();
            var description = "Test SDescription";
            _mockToDoRepository.Setup(r => r.ItemDescriptionExists(It.IsAny<string>())).ReturnsAsync(false);
            _mockToDoRepository.Setup(r => r.AddItem(It.IsAny<TodoItem>())).Callback(new AddItemCallback((TodoItem todoItem) =>
            {
                todoItem.Id = id;
            })).ReturnsAsync((1));

            // Act
            var actionResult = await _controller.PostTodoItem(new TodoItem() { Description = description });

            //Assert
            var createdAtActionResult = actionResult as CreatedAtActionResult;
            Assert.NotNull(createdAtActionResult);
            Assert.Equal(id, createdAtActionResult.RouteValues.GetValueOrDefault("id"));
            Assert.Equal(id, (createdAtActionResult.Value as TodoItem).Id);
            Assert.Equal(description, (createdAtActionResult.Value as TodoItem).Description);
            Assert.False((createdAtActionResult.Value as TodoItem).IsCompleted);
        }
        #endregion
    }
}
