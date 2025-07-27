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
    public partial class Opportunities
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

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.Opportunity> opportunities;

        protected RadzenDataGrid<CRMBlazorServerRBS.Models.RadzenCRM.Opportunity> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            opportunities = await RadzenCRMService.GetOpportunities(new Query { Filter = $@"i => i.UserId.Contains(@0) || i.Name.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Contact,OpportunityStatus" });
        }

            protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.Contact> contactsForContactId;

            protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.OpportunityStatus> opportunityStatusesForStatusId;
        protected override async Task OnInitializedAsync()
        {
            opportunities = await RadzenCRMService.GetOpportunities(new Query { Filter = $@"i => i.UserId.Contains(@0) || i.Name.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Contact,OpportunityStatus" });

            contactsForContactId = await RadzenCRMService.GetContacts();

            opportunityStatusesForStatusId = await RadzenCRMService.GetOpportunityStatuses();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await grid0.InsertRow(new CRMBlazorServerRBS.Models.RadzenCRM.Opportunity());
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.Opportunity opportunity)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await RadzenCRMService.DeleteOpportunity(opportunity.Id);

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
                    Detail = $"Unable to delete Opportunity"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await RadzenCRMService.ExportOpportunitiesToCSV(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Contact,OpportunityStatus",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Opportunities");
            }

            if (args == null || args.Value == "xlsx")
            {
                await RadzenCRMService.ExportOpportunitiesToExcel(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Contact,OpportunityStatus",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Opportunities");
            }
        }

        protected async Task GridRowUpdate(CRMBlazorServerRBS.Models.RadzenCRM.Opportunity args)
        {
            try
            {
                await RadzenCRMService.UpdateOpportunity(args.Id, args);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                      Severity = NotificationSeverity.Error,
                      Summary = $"Error",
                      Detail = $"Unable to update Opportunity"
                });
            }
        }

        protected async Task GridRowCreate(CRMBlazorServerRBS.Models.RadzenCRM.Opportunity args)
        {
            try
            {
                await RadzenCRMService.CreateOpportunity(args);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                      Severity = NotificationSeverity.Error,
                      Summary = $"Error",
                      Detail = $"Unable to create Opportunity"
                });
            }
            await grid0.Reload();
        }

        protected async Task EditButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.Opportunity data)
        {
            await grid0.EditRow(data);
        }

        protected async Task SaveButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.Opportunity data)
        {
            await grid0.UpdateRow(data);
        }

        protected async Task CancelButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.Opportunity data)
        {
            grid0.CancelEditRow(data);
            await RadzenCRMService.CancelOpportunityChanges(data);
        }
    }
}