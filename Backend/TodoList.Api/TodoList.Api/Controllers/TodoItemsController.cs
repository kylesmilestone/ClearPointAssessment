using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TodoList.Api.Models;
using TodoList.Api.Repositories;

namespace TodoList.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly ITodoRepository _repository;

        // assuming all the logic is correct and all controller logic are by designed - no functional refactoring
        // using controller-repository pattern in order to isolate different level logic, good for controller unit testing (some of dbcontext features are difficult to mock but we can always mock the repository)
        public TodoItemsController(ITodoRepository repository)
        {
            _repository = repository;
        }

        // GET: api/TodoItems
        [HttpGet]
        public async Task<IActionResult> GetTodoItems()
        {
            var results = await _repository.GetItems();
            return Ok(results);
        }

        // GET: api/TodoItems/...
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodoItem(Guid id)
        {
            var result = await _repository.GetItem(id);

            if (result == null)
            {
                return NotFound();
            }  

            return Ok(result);
        }

        // PUT: api/TodoItems/... 
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(Guid id, TodoItem todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }

            try
            {
                var result = await _repository.UpdateItem(id, todoItem);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _repository.ItemIdExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        } 

        // POST: api/TodoItems 
        [HttpPost]
        public async Task<IActionResult> PostTodoItem(TodoItem todoItem)
        {
            if (string.IsNullOrEmpty(todoItem?.Description))
            {
                return BadRequest("Description is required");
            }
            else if (await _repository.ItemDescriptionExists(todoItem.Description))
            {
                return BadRequest("Description already exists");
            } 

            await _repository.AddItem(todoItem);

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }
    }
}
