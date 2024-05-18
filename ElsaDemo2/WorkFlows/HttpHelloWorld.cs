using System.Net;

using Elsa;
using Elsa.Activities.Http;
using Elsa.Builders;
namespace ElsaDemo2.WorkFlows
{
    public class HelloWorld : IWorkflow
    {
        public void Build(IWorkflowBuilder builder)
        {
            builder
                .HttpEndpoint("/hello-world")
                .When(OutcomeNames.Done)
                .WriteHttpResponse(HttpStatusCode.OK, "<h1>Hello World!</h1>", "text/html");
        }
    }
}
