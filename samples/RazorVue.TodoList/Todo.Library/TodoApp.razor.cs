using System.Collections.Generic;
using ECMAScript;
using static ECMAScript.Vue3;
using Jazor.RazorVue;
using Microsoft.AspNetCore.Components;
using Todo.Library.Models;

namespace Todo.Library;

[ECMAScriptModule("./components/todo-app")]
public partial class TodoApp : ComponentBase, IVueComponent
{
    [Parameter]
    public string? DraftTitle { get; set; }

    [Parameter]
    public EventCallback<string?> DraftTitleChanged { get; set; }

    [Parameter]
    public string? DraftCategory { get; set; }

    [Parameter]
    public EventCallback<string?> DraftCategoryChanged { get; set; }

    [Parameter]
    public bool DraftPinned { get; set; }

    [Parameter]
    public EventCallback<bool> DraftPinnedChanged { get; set; }

    [Parameter]
    public bool ShowCompleted { get; set; }

    [Parameter]
    public EventCallback<bool> ShowCompletedChanged { get; set; }

    [Parameter]
    public string? StatusMessage { get; set; }

    [Parameter]
    public int TotalCount { get; set; }

    [Parameter]
    public int CompletedCount { get; set; }

    [Parameter]
    public int OpenCount { get; set; }

    [Parameter]
    public int PinnedCount { get; set; }

    [Parameter]
    public int VisibleCount { get; set; }

    [Parameter]
    public IReadOnlyList<TodoItem> Tasks { get; set; } = Array.Empty<TodoItem>();

    [Parameter]
    public EventCallback AddRequested { get; set; }
}
