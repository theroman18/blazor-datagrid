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
    public partial class EditOpportunity
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

        [Parameter]
        public int Id { get; set; }

        protected override async Task OnInitializedAsync()
        {
            opportunity = await RadzenCRMService.GetOpportunityById(Id);

            contactsForContactId = await RadzenCRMService.GetContacts();

            opportunityStatusesForStatusId = await RadzenCRMService.GetOpportunityStatuses();
        }
        protected bool errorVisible;
        protected CRMBlazorServerRBS.Models.RadzenCRM.Opportunity opportunity;

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.Contact> contactsForContactId;

        protected IEnumerable<CRMBlazorServerRBS.Models.RadzenCRM.OpportunityStatus> opportunityStatusesForStatusId;

        protected async Task FormSubmit()
        {
            try
            {
                await RadzenCRMService.UpdateOpportunity(Id, opportunity);
                DialogService.Close(opportunity);
            }
            catch (Exception ex)
            {
                errorVisible = true;
            }
        }

        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close(null);
        }





        bool hasContactIdValue;

        [Parameter]
        public int ContactId { get; set; }

        bool hasStatusIdValue;

        [Parameter]
        public int StatusId { get; set; }

        bool hasUserIdValue;

        [Parameter]
        public string UserId { get; set; }
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            opportunity = new CRMBlazorServerRBS.Models.RadzenCRM.Opportunity();

            hasContactIdValue = parameters.TryGetValue<int>("ContactId", out var hasContactIdResult);

            if (hasContactIdValue)
            {
                opportunity.ContactId = hasContactIdResult;
            }

            hasStatusIdValue = parameters.TryGetValue<int>("StatusId", out var hasStatusIdResult);

            if (hasStatusIdValue)
            {
                opportunity.StatusId = hasStatusIdResult;
            }

            hasUserIdValue = parameters.TryGetValue<string>("UserId", out var hasUserIdResult);

            if (hasUserIdValue)
            {
                opportunity.UserId = hasUserIdResult;
            }
            await base.SetParametersAsync(parameters);
        }
    }
}