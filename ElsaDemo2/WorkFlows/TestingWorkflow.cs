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
                .WithDisplayName("Document Approval Workflow")
                .HttpEndpoint(activity => activity
                    .WithPath("/v1/documents")
                    .WithMethod(HttpMethod.Post.Method)
                    .WithReadContent())
                .SetVariable("Document", context => context.GetInput<HttpRequestModel>()!.Body)
                .SendEmail(activity => activity
                    .WithSender("workflow@acme.com")
                    .WithRecipient("josh@acme.com")
                    .WithSubject(context => $"Document received from {context.GetVariable<dynamic>("Document")!.Author.Name}")
                    .WithBody(context =>
                    {
                        var document = context.GetVariable<dynamic>("Document")!;
                        var author = document!.Author;
                        return $"Document from {author.Name} received for review.<br><a href=\"{context.GenerateSignalUrl("Approve")}\">Approve</a> or <a href=\"{context.GenerateSignalUrl("Reject")}\">Reject</a>";
                    }))
                .WriteHttpResponse(
                    HttpStatusCode.OK,
                    "<h1>Request for Approval Sent</h1><p>Your document has been received and will be reviewed shortly.</p>",
                    "text/html")
                .Then<Fork>(activity => activity.WithBranches("Approve", "Reject", "Remind"), fork =>
                {
                    fork
                        .When("Approve")
                        .SignalReceived("Approve")
                        .SendEmail(activity => activity
                            .WithSender("workflow@acme.com")
                            .WithRecipient(context => context.GetVariable<dynamic>("Document")!.Author.Email)
                            .WithSubject(context => $"Document {context.GetVariable<dynamic>("Document")!.Id} Approved!")
                            .WithBody(context => $"Great job {context.GetVariable<dynamic>("Document")!.Author.Name}, that document is perfect."))
                        .ThenNamed("Join");

                    fork
                        .When("Reject")
                        .SignalReceived("Reject")
                        .SendEmail(activity => activity
                            .WithSender("workflow@acme.com")
                            .WithRecipient(context => context.GetVariable<dynamic>("Document")!.Author.Email)
                            .WithSubject(context => $"Document {context.GetVariable<dynamic>("Document")!.Id} Rejected")
                            .WithBody(context => $"Nice try {context.GetVariable<dynamic>("Document")!.Author.Name}, but that document needs work."))
                        .ThenNamed("Join");

                    fork
                        .When("Remind")
                        .Timer(Duration.FromSeconds(10)).WithName("Reminder")
                        .SendEmail(activity => activity
                                .WithSender("workflow@acme.com")
                                .WithRecipient("josh@acme.com")
                                .WithSubject(context => $"{context.GetVariable<dynamic>("Document")!.Author.Name} is waiting for your review!")
                                .WithBody(context =>
                                    $"Don't forget to review document {context.GetVariable<dynamic>("Document")!.Id}.<br><a href=\"{context.GenerateSignalUrl("Approve")}\">Approve</a> or <a href=\"{context.GenerateSignalUrl("Reject")}\">Reject</a>"))
                            .ThenNamed("Reminder");
                })
                .Add<Join>(join => join.WithMode(Join.JoinMode.WaitAny)).WithName("Join")
                .WriteHttpResponse(HttpStatusCode.OK, "Thanks for the hard work!", "text/html");
        }
    }
    //public void Build(IWorkflowBuilder builder)
    //{
    //    builder
    //        .StartWith<SignalReceived>(a =>
    //        {
    //            a.Set(x => x.Signal, "Sample1");
    //            a.Set(x => x.Id, "SignalReceived1");
    //        })
    //        .WriteLine($"Starting workflow")
    //        .Then<UserTaskSignal>(a =>
    //        {
    //            a.Set(x => x.Signal, "usertasksample1"); //should be lower case
    //            a.Set(x => x.TaskName, "Demo Sample1");
    //            a.Set(x => x.TaskTitle, "Press the button to continue");
    //            a.Set(x => x.TaskName, "The task will suspend the execution until the button is pressed");
    //        }
    //        )
    //        .WriteLine($"Workflow is done");
    //}

}



//using System.Dynamic;
//using System.Net;
//using System.Net.Mime;

//using Elsa.Activities.Http.Models;
//using Elsa.Expressions.Models;
//using Elsa.Extensions;
//using Elsa.Http;
//using Elsa.Http.Models;
//using Elsa.Workflows;
//using Elsa.Workflows.Activities;
//using Elsa.Workflows.Contracts;
//using Elsa.Workflows.Runtime.Activities;

//public class TestingWorkflow : IWorkflow
//{
//    public ValueTask BuildAsync(IWorkflowBuilder builder, CancellationToken cancellationToken = default)
//    {
//        throw new NotImplementedException();
//    }

//    protected  void Build(IWorkflowBuilder builder)
//    {
//        var documentVariable = builder.WithVariable<ExpandoObject>();
//        var approvedVariable = builder.WithVariable<bool>();

//        builder.Root = new Sequence
//        {
//            Activities =
//                {
//                    new HttpEndpoint
//                    {
//                        Path = new("/documents"),
//                        SupportedMethods = new(new[] { HttpMethods.Post }),
//                        ParsedContent = new(documentVariable),
//                        CanStartWorkflow = true
//                    },
//                    new WriteLine(context => $"Document received from {documentVariable.Get<dynamic>(context)!.Author.Name}."),
//                    new WriteHttpResponse
//                    {
//                        Content = new("<h1>Request for Approval Sent</h1><p>Your document has been received and will be reviewed shortly.</p>"),
//                        ContentType = new(MediaTypeNames.Text.Html),
//                        StatusCode = new(HttpStatusCode.OK),
//                        ResponseHeaders = new(new HttpHeaders { ["X-Powered-By"] = new[] { "Elsa 3.0" } })
//                    },
//                    new Fork
//                    {
//                        JoinMode = ForkJoinMode.WaitAll,
//                        Branches =
//                        {
//                            // Jack
//                            new Sequence
//                            {
//                                Activities =
//                                {
//                                    new WriteLine(context => $"Jack approve url: \n {GenerateSignalUrl(context, "Approve:Jack")}"),
//                                    new WriteLine(context => $"Jack reject url: \n {GenerateSignalUrl(context, "Reject:Jack")}"),
//                                    new Fork
//                                    {
//                                        JoinMode = ForkJoinMode.WaitAny,
//                                        Branches =
//                                        {
//                                            // Approve
//                                            new Sequence
//                                            {
//                                                Activities =
//                                                {
//                                                    new Event("Approve:Jack"),
//                                                    new SetVariable
//                                                    {
//                                                        Variable = approvedVariable,
//                                                        Value = new(true)
//                                                    },
//                                                    new WriteHttpResponse
//                                                    {
//                                                        Content = new("Thanks for the approval, Jack!"),
//                                                    }
//                                                }
//                                            },

//                                            // Reject
//                                            new Sequence
//                                            {
//                                                Activities =
//                                                {
//                                                    new Event("Reject:Jack"),
//                                                    new SetVariable
//                                                    {
//                                                        Variable = approvedVariable,
//                                                        Value = new(false)
//                                                    },
//                                                    new WriteHttpResponse
//                                                    {
//                                                        Content = new("Sorry to hear that, Jack!"),
//                                                    }
//                                                }
//                                            }
//                                        }
//                                    }
//                                }
//                            },

//                            // Lucy
//                            new Sequence
//                            {
//                                Activities =
//                                {
//                                    new WriteLine(context => $"Lucy approve url: \n {GenerateSignalUrl(context, "Approve:Lucy")}"),
//                                    new WriteLine(context => $"Lucy reject url: \n {GenerateSignalUrl(context, "Reject:Lucy")}"),
//                                    new Fork
//                                    {
//                                        JoinMode = ForkJoinMode.WaitAny,
//                                        Branches =
//                                        {
//                                            // Approve
//                                            new Sequence
//                                            {
//                                                Activities =
//                                                {
//                                                    new Event("Approve:Lucy"),
//                                                    new SetVariable
//                                                    {
//                                                        Variable = approvedVariable,
//                                                        Value = new(true)
//                                                    },
//                                                    new WriteHttpResponse
//                                                    {
//                                                        Content = new("Thanks for the approval, Lucy!"),
//                                                    }
//                                                }
//                                            },

//                                            // Reject
//                                            new Sequence
//                                            {
//                                                Activities =
//                                                {
//                                                    new Event("Reject:Lucy"),
//                                                    new SetVariable
//                                                    {
//                                                        Variable = approvedVariable,
//                                                        Value = new(false)
//                                                    },
//                                                    new WriteHttpResponse
//                                                    {
//                                                        Content = new("Sorry to hear that, Lucy!"),
//                                                    }
//                                                }
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    },
//                    new WriteLine(context => $"Approved: {approvedVariable.Get<bool>(context)}"),
//                    new If(context => approvedVariable.Get<bool>(context))
//                    {
//                        Then = new WriteLine(context => $"Document ${documentVariable.Get<dynamic>(context)!.Id} approved!"),
//                        Else = new WriteLine(context => $"Document ${documentVariable.Get<dynamic>(context)!.Id} rejected!")
//                    }
//                }
//        };
//    }

//    private string GenerateSignalUrl(ExpressionExecutionContext context, string signalName)
//    {
//        return context.GenerateEventTriggerUrl(signalName);
//    }
//}

