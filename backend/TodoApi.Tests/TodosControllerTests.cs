using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Controllers;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Tests;

public class TodosControllerTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique db per test
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoTodosExist()
    {
        using var db = CreateInMemoryDb();
        var controller = new TodosController(db);

        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IEnumerable<TodoItem>>(ok.Value);
        Assert.Empty(items);
    }

    [Fact]
    public async Task Create_AddsTodo_AndReturnsCreated()
    {
        using var db = CreateInMemoryDb();
        var controller = new TodosController(db);
        var newItem = new TodoItem { Title = "Buy groceries" };

        var result = await controller.Create(newItem);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var item = Assert.IsType<TodoItem>(created.Value);
        Assert.Equal("Buy groceries", item.Title);
        Assert.False(item.IsCompleted);
        Assert.True(item.Id > 0);
    }

    [Fact]
    public async Task Delete_RemovesTodo_AndReturnsNoContent()
    {
        using var db = CreateInMemoryDb();
        db.Todos.Add(new TodoItem { Title = "Task to delete" });
        await db.SaveChangesAsync();
        var savedId = db.Todos.First().Id;

        var controller = new TodosController(db);
        var result = await controller.Delete(savedId);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await db.Todos.CountAsync());
    }

    [Fact]
    public async Task Update_ModifiesTodo_AndReturnsUpdated()
    {
        using var db = CreateInMemoryDb();
        db.Todos.Add(new TodoItem { Title = "Original" });
        await db.SaveChangesAsync();
        var savedId = db.Todos.First().Id;

        var controller = new TodosController(db);
        var result = await controller.Update(savedId, new TodoItem { Title = "Updated", IsCompleted = true });

        var ok = Assert.IsType<OkObjectResult>(result);
        var item = Assert.IsType<TodoItem>(ok.Value);
        Assert.Equal("Updated", item.Title);
        Assert.True(item.IsCompleted);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenTodoDoesNotExist()
    {
        using var db = CreateInMemoryDb();
        var controller = new TodosController(db);

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
