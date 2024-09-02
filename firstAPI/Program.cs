using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<adminDB>(opt => opt.UseInMemoryDatabase("AdminList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<adminDB>(opt => opt.UseInMemoryDatabase("AdminList"));
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/admins", async (adminDB db) =>
    await db.Todos.Select(x => new adminDTO(x)).ToListAsync());

app.MapGet("/admins/complete", async (adminDB db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/admins/{id}", async (int id, adminDB db) =>
    await db.Todos.FindAsync(id)
        is Admin todo
            ? Results.Ok(new adminDTO(todo))
            : Results.NotFound());

app.MapPost("/admins", async (adminDTO todoItemDTO, adminDB db) =>
{
    var adminItem = new Admin
    {
        FName = todoItemDTO.FName,
        SName = todoItemDTO.SName,
        Email = todoItemDTO.Email,
        Password = todoItemDTO.Password,
        MobileNumber = todoItemDTO.MobileNumber,
        IsComplete = todoItemDTO.IsComplete
    };

    db.Todos.Add(adminItem);
    await db.SaveChangesAsync();

    return Results.Created($"/admins/{adminItem.Id}", new adminDTO(adminItem));
});

app.MapPut("/admins/{id}", async (int id, adminDTO inputitemTDO, adminDB db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.FName = inputitemTDO.FName;
    todo.SName = inputitemTDO.SName;
    todo.Email = inputitemTDO.Email;
    todo.Password = inputitemTDO.Password;
    todo.MobileNumber = inputitemTDO.MobileNumber;
    todo.IsComplete = inputitemTDO.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/admins/{id}", async (int id, adminDB db) =>
{
    if (await db.Todos.FindAsync(id) is Admin todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(new adminDTO(todo));
    }

    return Results.NotFound();
});

app.Run();

public class Admin
{
    public int Id { get; set; }
    public string? FName { get; set; }
    public string? SName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? MobileNumber { get; set; }
    public bool IsComplete { get; set; }
    public String? Secret { get; set; }
}

public class adminDB : DbContext
{
    public adminDB(DbContextOptions<adminDB> options)
        : base(options) { }

    public DbSet<Admin> Todos => Set<Admin>();
}

public class adminDTO
{
    public int Id { get; set; }
    public string? FName { get; set; }
    public string? SName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? MobileNumber { get; set; }
    public bool IsComplete { get; set; }

    public adminDTO() { }
    public adminDTO(Admin todoItem) =>
    (Id, FName,SName,Email,Password,MobileNumber, IsComplete) = (todoItem.Id, todoItem.FName, todoItem.SName, todoItem.Email, todoItem.Password, todoItem.MobileNumber, todoItem.IsComplete);
}