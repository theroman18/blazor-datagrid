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
    public partial class Contacts
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

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.Contact> contacts;

        protected RadzenDataGrid<CRMBlazorServerRBS.Models.RadzenCRM.Contact> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            contacts = await RadzenCRMService.GetContacts(new Query { Filter = $@"i => i.Email.Contains(@0) || i.Company.Contains(@0) || i.LastName.Contains(@0) || i.FirstName.Contains(@0) || i.Phone.Contains(@0)", FilterParameters = new object[] { search } });
        }
        protected override async Task OnInitializedAsync()
        {
            contacts = await RadzenCRMService.GetContacts(new Query { Filter = $@"i => i.Email.Contains(@0) || i.Company.Contains(@0) || i.LastName.Contains(@0) || i.FirstName.Contains(@0) || i.Phone.Contains(@0)", FilterParameters = new object[] { search } });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddContact>("Add Contact", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<CRMBlazorServerRBS.Models.RadzenCRM.Contact> args)
        {
            await DialogService.OpenAsync<EditContact>("Edit Contact", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.Contact contact)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await RadzenCRMService.DeleteContact(contact.Id);

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
                    Detail = $"Unable to delete Contact"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await RadzenCRMService.ExportContactsToCSV(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Contacts");
            }

            if (args == null || args.Value == "xlsx")
            {
                await RadzenCRMService.ExportContactsToExcel(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Contacts");
            }
        }

        protected CRMBlazorServerRBS.Models.RadzenCRM.Contact contactChild;
        protected async Task GetChildData(CRMBlazorServerRBS.Models.RadzenCRM.Contact args)
        {
            contactChild = args;
            var OpportunitiesResult = await RadzenCRMService.GetOpportunities(new Query { Filter = $@"i => i.ContactId == {args.Id}", Expand = "Contact,OpportunityStatus" });
            if (OpportunitiesResult != null)
            {
                args.Opportunities = OpportunitiesResult.ToList();
            }
        }
        protected CRMBlazorServerRBS.Models.RadzenCRM.Opportunity opportunityOpportunities;

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.Contact> contactsForContactIdOpportunities;

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.OpportunityStatus> opportunityStatusesForStatusIdOpportunities;

        protected RadzenDataGrid<CRMBlazorServerRBS.Models.RadzenCRM.Opportunity> OpportunitiesDataGrid;

        protected async Task OpportunitiesAddButtonClick(MouseEventArgs args, CRMBlazorServerRBS.Models.RadzenCRM.Contact data)
        {

            var dialogResult = await DialogService.OpenAsync<AddOpportunity>("Add Opportunities", new Dictionary<string, object> { {"ContactId" , data.Id} });
            await GetChildData(data);
            await OpportunitiesDataGrid.Reload();

        }

        protected async Task OpportunitiesRowSelect(CRMBlazorServerRBS.Models.RadzenCRM.Opportunity args, CRMBlazorServerRBS.Models.RadzenCRM.Contact data)
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

                    await GetChildData(contactChild);

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

        string lastFilter;
        protected async void Grid0Render(DataGridRenderEventArgs<CRMBlazorServerRBS.Models.RadzenCRM.Contact> args)
        {
            if (grid0.Query.Filter != lastFilter)
            {
                contactChild = grid0.View.FirstOrDefault();
            }

            if (grid0.Query.Filter != lastFilter && contactChild != null)
            {
                await grid0.SelectRow(contactChild);
            }

            lastFilter = grid0.Query.Filter;
        }
    }
}