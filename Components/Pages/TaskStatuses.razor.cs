using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace CRMBlazorServerRBS.Components.Pages
{
    public partial class TaskStatuses
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        public RadzenCRMService RadzenCRMService { get; set; }

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.TaskStatus> taskStatuses;

        protected RadzenDataGrid<CRMBlazorServerRBS.Models.RadzenCRM.TaskStatus> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            taskStatuses = await RadzenCRMService.GetTaskStatuses(new Query { Filter = $@"i => i.Name.Contains(@0)", FilterParameters = new object[] { search } });
        }
        protected override async Task OnInitializedAsync()
        {
            taskStatuses = await RadzenCRMService.GetTaskStatuses(new Query { Filter = $@"i => i.Name.Contains(@0)", FilterParameters = new object[] { search } });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddTaskStatus>("Add TaskStatus", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<CRMBlazorServerRBS.Models.RadzenCRM.TaskStatus> args)
        {
            await DialogService.OpenAsync<EditTaskStatus>("Edit TaskStatus", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.TaskStatus taskStatus)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await RadzenCRMService.DeleteTaskStatus(taskStatus.Id);

                    if (deleteResult != null)
                    {
                        await grid0.Reload();
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = $"Error",
                    Detail = $"Unable to delete TaskStatus"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await RadzenCRMService.ExportTaskStatusesToCSV(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "TaskStatuses");
            }

            if (args == null || args.Value == "xlsx")
            {
                await RadzenCRMService.ExportTaskStatusesToExcel(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "TaskStatuses");
            }
        }

        protected CRMBlazorServerRBS.Models.RadzenCRM.TaskStatus taskStatusChild;
        protected async Task GetChildData(CRMBlazorServerRBS.Models.RadzenCRM.TaskStatus args)
        {
            taskStatusChild = args;
            var TasksResult = await RadzenCRMService.GetTasks(new Query { Filter = $@"i => i.StatusId == {args.Id}", Expand = "Opportunity,TaskType,TaskStatus" });
            if (TasksResult != null)
            {
                args.Tasks = TasksResult.ToList();
            }
        }
        protected CRMBlazorServerRBS.Models.RadzenCRM.Task taskTasks;

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.Opportunity> opportunitiesForOpportunityIdTasks;

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.TaskType> taskTypesForTypeIdTasks;

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.TaskStatus> taskStatusesForStatusIdTasks;

        protected RadzenDataGrid<CRMBlazorServerRBS.Models.RadzenCRM.Task> TasksDataGrid;

        protected async Task TasksAddButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.TaskStatus data)
        {

            var dialogResult = await DialogService.OpenAsync<AddTask>("Add Tasks", new Dictionary<string, object> { {"StatusId" , data.Id} });
            await GetChildData(data);
            await TasksDataGrid.Reload();

        }

        protected async Task TasksRowSelect(CRMBlazorServerRBS.Models.RadzenCRM.Task args, CRMBlazorServerRBS.Models.RadzenCRM.TaskStatus data)
        {
            var dialogResult = await DialogService.OpenAsync<EditTask>("Edit Tasks", new Dictionary<string, object> { {"Id", args.Id} });
            await GetChildData(data);
            await TasksDataGrid.Reload();
        }

        protected async Task TasksDeleteButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.Task task)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await RadzenCRMService.DeleteTask(task.Id);

                    await GetChildData(taskStatusChild);

                    if (deleteResult != null)
                    {
                        await TasksDataGrid.Reload();
                    }
                }
            }
            catch (System.Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = $"Error",
                    Detail = $"Unable to delete Task"
                });
            }
        }

        string lastFilter;
        protected async void Grid0Render(DataGridRenderEventArgs<CRMBlazorServerRBS.Models.RadzenCRM.TaskStatus> args)
        {
            if (grid0.Query.Filter != lastFilter)
            {
                taskStatusChild = grid0.View.FirstOrDefault();
            }

            if (grid0.Query.Filter != lastFilter && taskStatusChild != null)
            {
                await grid0.SelectRow(taskStatusChild);
            }

            lastFilter = grid0.Query.Filter;
        }
    }
}