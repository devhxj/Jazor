namespace Todo.Library.Models;

public sealed record TodoItem(
    int Id,
    string Title,
    string Category,
    bool IsDone,
    bool IsPinned);
