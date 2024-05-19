using System.Net;
using System.Net.Http;

using Elsa.Activities.ControlFlow;
using Elsa.Activities.Email;
using Elsa.Activities.Http;
using Elsa.Activities.Http.Extensions;
using Elsa.Activities.Http.Models;
using Elsa.Activities.Primitives;
using Elsa.Activities.Temporal;
using Elsa.Builders;

using NodaTime;



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
                .SetVariable("Bin", context => context.GetInput<HttpRequestModel>()!.Body)
                .WriteHttpResponse(
                    HttpStatusCode.OK,
                    "<h1>Request for Approval Sent</h1><p>Your bin has been received and will be reviewed shortly.</p>",
                    "text/html")
                .Then<Fork>(activity => activity.WithBranches("Approve", "Reject"), fork =>
                {
                    fork
                        .When("Approve")
                        .SignalReceived("Approve")
                        .WriteHttpResponse(
                            HttpStatusCode.OK,
                            "<h1>Bin Approved and send for a person to collection Sample.</p>",
                            "text/html")
                        .ThenNamed("Join");

                    fork
                        .When("Reject")
                        .SignalReceived("Reject")
                        .WriteHttpResponse(
                            HttpStatusCode.OK,
                            "<h1>Bin Rejected for retest.</p>",
                            "text/html")
                        .ThenNamed("Join");
                })
                .Add<Join>(join => join.WithMode(Join.JoinMode.WaitAny)).WithName("Join")
                .WriteHttpResponse(HttpStatusCode.OK, "Thanks for your work!", "text/html");
        }
    }
}