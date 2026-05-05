using System.Linq;
using ECMAScript;
using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;
using Todo.Library.Models;

namespace Todo.Library;

[ECMAScriptModule("./components/todo-app")]
public partial class TodoApp : ComponentBase, IVueComponent
{
    private int _nextId = 4;
    private string? _draftTitle = "Document RazorVue SFC contract";
    private string? _draftCategory = "Architecture";
    private bool _draftPinned;
    private bool _showCompleted = true;
    private string? _statusMessage = "Library mode emits Vue SFC artifacts during design time.";

    private TodoItem[] _tasks =
    [
        new(1, "Define per-component SFC topology", "Compiler", false, true),
        new(2, "Wire host requirements into consumer bootstrap", "Host", true, false),
        new(3, "Verify generated .vue imports stay stable", "Emit", false, false)
    ];

    private TodoItem[] Tasks => _tasks;

    private string? DraftTitle => _draftTitle;

    private string? DraftCategory => _draftCategory;

    private bool DraftPinned => _draftPinned;

    private bool ShowCompleted => _showCompleted;

    private string? StatusMessage => _statusMessage;

    private TodoItem[] VisibleTasks
        => _showCompleted
            ? _tasks
            : _tasks.Where(static task => !task.IsDone).ToArray();

    private int CompletedCount => _tasks.Count(static task => task.IsDone);

    private int OpenCount => _tasks.Count(static task => !task.IsDone);

    private int PinnedCount => _tasks.Count(static task => task.IsPinned);

    private void DraftTitleChanged(string? value)
    {
        _draftTitle = value;
        _statusMessage = "Draft title updated in component state.";
    }

    private void DraftCategoryChanged(string? value)
    {
        _draftCategory = value;
        _statusMessage = "Category focus updated.";
    }

    private void DraftPinnedChanged(bool value)
    {
        _draftPinned = value;
        _statusMessage = value
            ? "New tasks will be pinned for focus."
            : "New tasks will be created without a pin.";
    }

    private void ShowCompletedChanged(bool value)
    {
        _showCompleted = value;
        _statusMessage = value
            ? "Showing the full backlog."
            : "Filtering to active work only.";
    }

    private void AddTask()
    {
        var title = string.IsNullOrWhiteSpace(_draftTitle)
            ? "Untitled task"
            : _draftTitle!.Trim();
        var category = string.IsNullOrWhiteSpace(_draftCategory)
            ? "General"
            : _draftCategory!.Trim();

        var item = new TodoItem(_nextId++, title, category, false, _draftPinned);
        _tasks = [item, .. _tasks];
        _draftTitle = string.Empty;
        _statusMessage = "Added \"" + item.Title + "\" to the top of the workspace.";
    }

    private EventCallback<bool> CreateDoneChanged(int id)
        => EventCallback.Factory.Create<bool>(this, value => UpdateDone(id, value));

    private EventCallback CreatePinToggle(int id)
        => EventCallback.Factory.Create(this, () => TogglePin(id));

    private void UpdateDone(int id, bool value)
    {
        _tasks = _tasks
            .Select(task => task.Id == id ? task with { IsDone = value } : task)
            .ToArray();

        var updated = _tasks.First(task => task.Id == id);
        _statusMessage = updated.IsDone
            ? "\"" + updated.Title + "\" marked complete."
            : "\"" + updated.Title + "\" reopened.";
    }

    private void TogglePin(int id)
    {
        _tasks = _tasks
            .Select(task => task.Id == id ? task with { IsPinned = !task.IsPinned } : task)
            .ToArray();

        var updated = _tasks.First(task => task.Id == id);
        _statusMessage = updated.IsPinned
            ? "\"" + updated.Title + "\" pinned for follow-up."
            : "\"" + updated.Title + "\" unpinned.";
    }

    private static string BuildSubtitle(TodoItem item)
        => item.Category + " | " + (item.IsDone ? "Completed" : "Active");

    private static string BuildPinButtonText(TodoItem item)
        => item.IsPinned ? "Unpin" : "Pin";
}
