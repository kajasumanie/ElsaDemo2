using Elsa;
using Elsa.Activities.Console;
using Elsa.Activities.ControlFlow;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Activities.Signaling;
using Elsa.Activities.UserTask;
using Elsa.Activities.UserTask.Activities;
using Elsa.Builders;
using Elsa.Services;
using Elsa.Services.Models;
using Elsa.Workflows.Activities;

namespace ElsaDemo2.WorkFlows
{
    public class RetestRequestWorkflow : IWorkflow
    {
        public void Build(IWorkflowBuilder builder)
        {
            builder
                .StartWith<SignalReceived>(x => x.Set(y => y.Signal, "StartRetestRequest"))

                // Manager Approval
                .Then<UserTask>(x => x
                    .Set(y => y.Name, "ManagerApproval")
                    .Set(y => y.DisplayName, "Approve or Reject Retest Request"));
        }

    }
}
    //public void Build(IWorkflowBuilder builder)
    //{
    //    builder
    //          .StartWith<SignalReceived>(x => x.Set(x => x.Signal, "StartRetestRequest"))

    //          // Manager Approval
    //          .Then<UserTask>(x =>
    //        {
    //            x.Name = "ManagerApproval";
    //            x.DisplayName = "Approve or Reject Retest Request";
    //        })
    //        .Then<IfElse>(
    //            x => x.Condition = context => context.GetVariable<string>("ApprovalDecision") == "Approved",
    //            ifElse =>
    //            {
    //                ifElse
    //                    .When(OutcomeNames.True)
    //                    .Then<UserTask>(x =>
    //                    {
    //                        x.Name = "SampleCollection";
    //                        x.DisplayName = "Collect Sample for Retest";
    //                    })
    //                    .Then<UserTask>(x =>
    //                    {
    //                        x.Name = "SubmitForTesting";
    //                        x.DisplayName = "Submit Samples for Testing";
    //                    })
    //                    .Then<UserTask>(x =>
    //                    {
    //                        x.Name = "FinalManagerApproval";
    //                        x.DisplayName = "Final Approval of Retest Results";
    //                    })
    //                    .Then<IfElse>(
    //                        x => x.ConditionExpression = context => context.GetVariable<string>("FinalApprovalDecision") == "Approved",
    //                        finalIfElse =>
    //                        {
    //                            finalIfElse
    //                                .When(OutcomeNames.True)
    //                                .Then<WriteLine>(x => x.Text = "Retest Request Completed");
    //                            finalIfElse
    //                                .When(OutcomeNames.False)
    //                                .Then<WriteLine>(x => x.Text = "Retest Request Rejected");
    //                        });

    //                ifElse
    //                    .When(OutcomeNames.False)
    //                    .Then<WriteLine>(x => x.Text = "Retest Request Rejected");
    //            });
    //}


