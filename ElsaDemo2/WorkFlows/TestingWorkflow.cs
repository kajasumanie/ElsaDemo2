using System.Net;
using System.Net.Http;

using Elsa.Activities.ControlFlow;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Activities.Email;
using Elsa.Activities.Http;
using Elsa.Activities.Http.Extensions;
using Elsa.Activities.Http.Models;
using Elsa.Activities.Primitives;
using Elsa.Activities.Temporal;
using Elsa.Builders;
using Elsa.Models;

using NodaTime;

using static ElsaDemo2.Controllers.BinApprovalController;



namespace ElsaDemo2.WorkFlows
{
    public class TestingWorkflow : IWorkflow
    {
        public void Build(IWorkflowBuilder builder)
        {
            builder
                .WithDisplayName("Bin Approval Workflow")
                .HttpEndpoint(activity => activity
                    .WithPath("/v1/bins")
                    .WithMethod(HttpMethod.Post.Method)
                    .WithReadContent())
                //.SetVariable("BinId", context => context.GetInput<HttpRequestModel>().GetBody<InitiateRequest>().BinId)
                //.SetVariable("User", context => context.GetInput<HttpRequestModel>().GetBody<InitiateRequest>().User)
                .Then(context => context.SetVariable("InstanceId", context.WorkflowInstance.Id))
                .WriteHttpResponse(
                    HttpStatusCode.OK,
                    "<h1>Request for Approval Sent</h1><p>Your bin has been received and will be reviewed shortly.</p>",
                    "text/html")
                .SignalReceived("Approve/Reject")
                .SetVariable("IsApproved", context => ((ApproveRequest)context.GetInput<WorkflowInput>().Input).ApproveStatus)
                .Then<If>(
                    activity => activity
                        .WithCondition(context => context.GetVariable<bool>("IsApproved")),
                    ifElse =>
                    {
                        ifElse
                            .When(Elsa.OutcomeNames.True)
                            .SetVariable("TestStatus", "Approved")
                            .ThenNamed("Join");

                        ifElse
                            .When(Elsa.OutcomeNames.False)
                            .SetVariable("TestStatus", "Rejected")
                            .ThenNamed("Join");
                    })
                .Add<Elsa.Activities.ControlFlow.Join>(join => join.WithMode(Elsa.Activities.ControlFlow.Join.JoinMode.WaitAny)).WithName("Join")
                .WriteHttpResponse(HttpStatusCode.OK, "Thanks for your work!", "text/html");
        }
    }
}