using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly AppDbContext _db;

    public TodosController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var todos = await _db.Todos.ToListAsync();
        return Ok(todos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var todo = await _db.Todos.FindAsync(id);
        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TodoItem item)
    {
        item.Id = 0; // let EF assign the id
        _db.Todos.Add(item);
        await _db.SaveChangesAsync();
        Console.WriteLine($"Created Todo with name: {item.Title}");
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TodoItem item)
    {
        var existing = await _db.Todos.FindAsync(id);
        if (existing is null) return NotFound();

        existing.Title = item.Title;
        existing.IsCompleted = item.IsCompleted;
        await _db.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Todos.FindAsync(id);
        if (existing is null) return NotFound();
        Console.WriteLine($"Deleting Todo with name: {existing.Title}");

        _db.Todos.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
