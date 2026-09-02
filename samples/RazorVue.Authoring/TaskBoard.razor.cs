using System.Threading.Tasks;
using ECMAScript;
using ECMAScript.TDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using static ECMAScript.Vue;

namespace RazorVue.Authoring;

[ECMAScriptModule("./components/task-board")]
public partial class TaskBoard : ComponentBase, IVueComponent, IDisposable
{
    [Inject]
    public NavigationManager Navigation { get; set; } = null!;

    private readonly List<TaskRow> tasks =
    [
        new(1, "Review the generated module", "Compiler", "Open"),
        new(2, "Check the package closure", "Release", "Open"),
        new(3, "Document the authoring boundary", "Docs", "Done")
    ];

    // Object initializers are intentional: erased record construction preserves these
    // writable fields in the generated Vue state, including the blank-form path.
    private TaskDraft Draft { get; set; } = NewDraft();

    private bool DialogVisible { get; set; }

    private bool Saving { get; set; }

    private string CurrentPath { get; set; } = "/";

    private string WorkspaceName { get; } = "RazorVue workspace";

    private string StatusMessage { get; set; } = "Ready for the next task.";

    private string ValidationMessage { get; set; } = "Title and owner are required.";

    private string LastLocationEvent { get; set; } = "none";

    private string LastHistoryEntryState { get; set; } = "none";

    private int LocationEventCount { get; set; }

    private TFormRules<TaskDraft> Rules { get; } = new()
    {
        ["title"] =
        [
            new TFormRule { Required = true, Message = "Add a title before saving." }
        ],
        ["owner"] =
        [
            new TFormRule { Required = true, Message = "Add an owner before saving." }
        ]
    };

    private TaskRow[] Tasks => tasks.ToArray();

    private int OpenCount => tasks.Count(static task => task.Status == "Open");

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += OnLocationChanged;
        CurrentPath = Navigation.ToBaseRelativePath(Navigation.Uri);
        if (CurrentPath.Length == 0)
            CurrentPath = "/";
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        LastLocationEvent = args.Location;
        LastHistoryEntryState = args.HistoryEntryState ?? "none";
        LocationEventCount++;
    }

    private void OpenDialog()
    {
        Draft = NewDraft();
        DialogVisible = true;
        StatusMessage = "Drafting a new task.";
    }

    private void CancelDialog(TDialogCancelEventContext context)
    {
        DialogVisible = false;
        StatusMessage = "Draft cancelled.";
    }

    private async Task ConfirmDialog(TDialogConfirmEventContext context)
        => await SaveDraftAsync();

    private async Task SubmitForm(TSubmitContext<TaskDraft> context)
        => await SaveDraftAsync();

    private void ValidateForm(TValidateResultContext<TaskDraft> context)
    {
        ValidationMessage = context.FirstError ?? "Form is valid.";
        if (context.FirstError is not null)
            StatusMessage = "Fix the highlighted fields before saving.";
    }

    private void ResetForm(TFormResetEventContext<TaskDraft> context)
    {
        Draft = NewDraft();
        ValidationMessage = "Form reset. Title and owner are required.";
        StatusMessage = "Draft reset.";
    }

    private async Task SaveDraftAsync()
    {
        if (Saving)
            return;

        var title = Draft.Title.Trim();
        if (title.Length == 0)
        {
            StatusMessage = "Add a title before saving.";
            return;
        }

        Saving = true;
        await Task.Yield();
        tasks.Add(new TaskRow(tasks.Count + 1, title, Draft.Owner.Trim(), "Open"));
        Saving = false;
        DialogVisible = false;
        StatusMessage = "Task created from the typed form.";
    }

    private void GoToTasks()
    {
        Navigation.NavigateTo("/tasks");
        CurrentPath = "/tasks";
    }

    private void ReplaceTaskRoute()
    {
        var uri = Navigation.BaseUri + "tasks?view=compact";
        Navigation.NavigateTo(
            uri,
            new Microsoft.AspNetCore.Components.NavigationOptions
            {
                ReplaceHistoryEntry = true,
                HistoryEntryState = "authoring-replace"
            });
        CurrentPath = "/" + Navigation.ToBaseRelativePath(uri).TrimStart('/');
        StatusMessage = "Current route replaced.";
    }

    private void OpenTaskDetails()
        => Navigation.NavigateTo("/tasks/2?highlight=true");

    private static TaskDraft NewDraft() => new()
    {
        Title = string.Empty,
        Owner = string.Empty
    };

    public void Dispose()
        => Navigation.LocationChanged -= OnLocationChanged;
}
