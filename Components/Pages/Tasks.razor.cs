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
    public partial class Tasks
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

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.Task> tasks;

        protected RadzenDataGrid<CRMBlazorServerRBS.Models.RadzenCRM.Task> grid0;
        protected bool isEdit = true;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            tasks = await RadzenCRMService.GetTasks(new Query { Filter = $@"i => i.Title.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Opportunity,TaskType,TaskStatus" });
        }
        protected override async Task OnInitializedAsync()
        {
            tasks = await RadzenCRMService.GetTasks(new Query { Filter = $@"i => i.Title.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Opportunity,TaskType,TaskStatus" });

            opportunitiesForOpportunityId = await RadzenCRMService.GetOpportunities();

            taskTypesForTypeId = await RadzenCRMService.GetTaskTypes();

            taskStatusesForStatusId = await RadzenCRMService.GetTaskStatuses();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await grid0.SelectRow(null);
            isEdit = false;
            task = new CRMBlazorServerRBS.Models.RadzenCRM.Task();
        }

        protected async Task EditRow(CRMBlazorServerRBS.Models.RadzenCRM.Task args)
        {
            isEdit = true;
            task = args;
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.Task task)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await RadzenCRMService.DeleteTask(task.Id);

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
                    Detail = $"Unable to delete Task"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await RadzenCRMService.ExportTasksToCSV(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Opportunity,TaskType,TaskStatus",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Tasks");
            }

            if (args == null || args.Value == "xlsx")
            {
                await RadzenCRMService.ExportTasksToExcel(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Opportunity,TaskType,TaskStatus",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Tasks");
            }
        }
        protected bool errorVisible;
        protected CRMBlazorServerRBS.Models.RadzenCRM.Task task;

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.Opportunity> opportunitiesForOpportunityId;

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.TaskType> taskTypesForTypeId;

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.TaskStatus> taskStatusesForStatusId;

        protected async Task FormSubmit()
        {
            try
            {
                var result = isEdit ? await RadzenCRMService.UpdateTask(task.Id, task) : await RadzenCRMService.CreateTask(task);

            }
            catch (Exception ex)
            {
                errorVisible = true;
            }
        }

        protected async Task CancelButtonClick(MouseEventArgs args)
        {

        }
    }
}