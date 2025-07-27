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
    public partial class OpportunityStatuses
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

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.OpportunityStatus> opportunityStatuses;

        protected RadzenDataGrid<CRMBlazorServerRBS.Models.RadzenCRM.OpportunityStatus> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            opportunityStatuses = await RadzenCRMService.GetOpportunityStatuses(new Query { Filter = $@"i => i.Name.Contains(@0)", FilterParameters = new object[] { search } });
        }
        protected override async Task OnInitializedAsync()
        {
            opportunityStatuses = await RadzenCRMService.GetOpportunityStatuses(new Query { Filter = $@"i => i.Name.Contains(@0)", FilterParameters = new object[] { search } });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddOpportunityStatus>("Add OpportunityStatus", null);
            await grid0.Reload();
        }

        protected async Task EditRow(CRMBlazorServerRBS.Models.RadzenCRM.OpportunityStatus args)
        {
            await DialogService.OpenAsync<EditOpportunityStatus>("Edit OpportunityStatus", new Dictionary<string, object> { {"Id", args.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.OpportunityStatus opportunityStatus)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await RadzenCRMService.DeleteOpportunityStatus(opportunityStatus.Id);

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
                    Detail = $"Unable to delete OpportunityStatus"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await RadzenCRMService.ExportOpportunityStatusesToCSV(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "OpportunityStatuses");
            }

            if (args == null || args.Value == "xlsx")
            {
                await RadzenCRMService.ExportOpportunityStatusesToExcel(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "OpportunityStatuses");
            }
        }

        protected CRMBlazorServerRBS.Models.RadzenCRM.OpportunityStatus opportunityStatusChild;
        protected async Task GetChildData(CRMBlazorServerRBS.Models.RadzenCRM.OpportunityStatus args)
        {
            opportunityStatusChild = args;
            var OpportunitiesResult = await RadzenCRMService.GetOpportunities(new Query { Filter = $@"i => i.StatusId == {args.Id}", Expand = "Contact,OpportunityStatus" });
            if (OpportunitiesResult != null)
            {
                args.Opportunities = OpportunitiesResult.ToList();
            }
        }
        protected CRMBlazorServerRBS.Models.RadzenCRM.Opportunity opportunityOpportunities;

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.Contact> contactsForContactIdOpportunities;

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.OpportunityStatus> opportunityStatusesForStatusIdOpportunities;

        protected RadzenDataGrid<CRMBlazorServerRBS.Models.RadzenCRM.Opportunity> OpportunitiesDataGrid;

        protected async Task OpportunitiesAddButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.OpportunityStatus data)
        {

            var dialogResult = await DialogService.OpenAsync<AddOpportunity>("Add Opportunities", new Dictionary<string, object> { {"StatusId" , data.Id} });
            await GetChildData(data);
            await OpportunitiesDataGrid.Reload();

        }

        protected async Task OpportunitiesRowSelect(CRMBlazorServerRBS.Models.RadzenCRM.Opportunity args, CRMBlazorServerRBS.Models.RadzenCRM.OpportunityStatus data)
        {
            var dialogResult = await DialogService.OpenAsync<EditOpportunity>("Edit Opportunities", new Dictionary<string, object> { {"Id", args.Id} });
            await GetChildData(data);
            await OpportunitiesDataGrid.Reload();
        }

        protected async Task OpportunitiesDeleteButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.Opportunity opportunity)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await RadzenCRMService.DeleteOpportunity(opportunity.Id);

                    await GetChildData(opportunityStatusChild);

                    if (deleteResult != null)
                    {
                        await OpportunitiesDataGrid.Reload();
                    }
                }
            }
            catch (System.Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = $"Error",
                    Detail = $"Unable to delete Opportunity"
                });
            }
        }
    }
}