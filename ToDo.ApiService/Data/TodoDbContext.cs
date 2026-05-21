using Microsoft.EntityFrameworkCore;

namespace ToDo.ApiService.Data;

public class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();
    public DbSet<TodoList> Lists => Set<TodoList>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoList>().HasMany(l => l.Items).WithOne(t => t.List).HasForeignKey(t => t.ListId);

        modelBuilder.Entity<TodoList>().HasData(
            new TodoList { Id = 1, Name = "Default", CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<TodoItem>().HasData(
            new TodoItem { Id = 1, Title = "Team Meeting", Description = "Discuss Q3 roadmap and sprint priorities with the team.", IsCompleted = false, CreatedAt = DateTime.UtcNow, ListId = 1 },
            new TodoItem { Id = 2, Title = "Work on Branding", Description = "Update brand guidelines and review logo variations.", IsCompleted = false, CreatedAt = DateTime.UtcNow, ListId = 1 },
            new TodoItem { Id = 3, Title = "Make a Report for client", Description = "Compile analytics data and prepare the monthly report.", IsCompleted = false, CreatedAt = DateTime.UtcNow, ListId = 1 },
            new TodoItem { Id = 4, Title = "Create a planer", Description = "Design the weekly planning template for the team.", IsCompleted = false, CreatedAt = DateTime.UtcNow, ListId = 1 },
            new TodoItem { Id = 5, Title = "Create Treatment Plan", Description = "Draft the patient treatment plan based on latest assessment.", IsCompleted = false, CreatedAt = DateTime.UtcNow, ListId = 1 }
        );
    }
}
