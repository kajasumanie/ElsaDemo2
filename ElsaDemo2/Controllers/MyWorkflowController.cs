using Elsa;
using Elsa.Services;

using ElsaDemo2.WorkFlows;

using Microsoft.AspNetCore.Mvc;

namespace ElsaDemo2.Controllers
{
    [ApiController]
    [Route("my-workflow")]
    public class MyWorkflowController : Controller
    {
        private readonly IWorkflowRegistry _workflowRegistry;
        private readonly IStartsWorkflow _workflowStarter;

        public MyWorkflowController(IWorkflowRegistry workflowRegistry, IStartsWorkflow workflowStarter)
        {
            _workflowRegistry = workflowRegistry;
            _workflowStarter = workflowStarter;
        }

        [HttpGet("run")]
        public async Task<IActionResult> RunMyWorkflow(CancellationToken cancellationToken)
        {
            var myWorkflowBlueprint = (await _workflowRegistry.GetWorkflowAsync<HelloWorld>(cancellationToken))!;

            await _workflowStarter.StartWorkflowAsync(myWorkflowBlueprint, cancellationToken: cancellationToken);

            return Ok();
        }
    }
}
