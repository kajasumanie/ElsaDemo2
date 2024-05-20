using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Elsa.Services;
using Elsa.Services.Models;
using Elsa.Models;
using ElsaDemo2.WorkFlows;
using Elsa;
using System.Threading;
using Elsa.Extensions;

namespace ElsaDemo2.Controllers
{
    [ApiController]
    [Route("v1/bins")]
    public class BinController : ControllerBase
    {
        private readonly IStartsWorkflow _workflowStarter;
        private readonly IWorkflowRegistry _workflowRegistry;

        public BinController(IStartsWorkflow workflowStarter, IWorkflowRegistry workflowRegistry)
        {
            _workflowStarter = workflowStarter;
            _workflowRegistry = workflowRegistry;
        }

        [HttpPost]
        public async Task<IActionResult> RequestApproval([FromBody] object bin)
        {
            var workflowDefinition = await _workflowRegistry.FindByDefinitionVersionIdAsync("");
            if (workflowDefinition == null)
                return NotFound("Workflow definition not found.");

            var workflowInput = new Variables();
            workflowInput.Set("Bin", bin);

            var workflowResult = await _workflowStarter.StartWorkflowAsync(workflowDefinition);

            if (workflowResult.WorkflowInstance.WorkflowStatus == Elsa.Models.WorkflowStatus.Faulted)
                return StatusCode(500, "There was an error processing your request.");

            return Ok("Request for approval sent.");
        }

        [HttpPost("approve")]
        public async Task<IActionResult> ApproveBin()
        {
            // Assuming _workflowRegistry, _workflowStarter are properly initialized

            //var workflowDefinition = await _workflowRegistry.FindByDefinitionVersionIdAsync("");
            //if (workflowDefinition == null)
            //    return NotFound("Workflow definition not found.");

            // Consider passing proper input data for the workflow
            var workflowInput = new WorkflowInput();
            workflowInput.SetPropertyValue("Input", "Approve");

            // You may want to set properties here if needed
            // workflowInput.SetPropertyValue("Approve");

            // Assuming HelloWorld is the workflow you want to start
            var workflowBlueprint = await _workflowRegistry.GetWorkflowAsync<TestingWorkflow>();
            if (workflowBlueprint == null)
                return NotFound("Workflow blueprint not found.");

            var workflowResult = await _workflowStarter.StartWorkflowAsync(workflowBlueprint,"", workflowInput);

            if (workflowResult.WorkflowInstance.WorkflowStatus == Elsa.Models.WorkflowStatus.Faulted)
                return StatusCode(500, "There was an error processing your approval.");

            // Assuming you're returning the result of the approval process
            return Ok("Bin approved.");


        }

        [HttpPost("reject")]
        public async Task<IActionResult> RejectBin()
        {
            var workflowDefinition = await _workflowRegistry.FindByDefinitionVersionIdAsync("");
            if (workflowDefinition == null)
                return NotFound("Workflow definition not found.");

            var workflowInput = new Variables();
            workflowInput.Set("Signal", "Reject");

            var workflowResult = await _workflowStarter.StartWorkflowAsync(workflowDefinition);

            if (workflowResult.WorkflowInstance.WorkflowStatus == Elsa.Models.WorkflowStatus.Faulted)
                return StatusCode(500, "There was an error processing your rejection.");

            return Ok("Bin rejected.");
        }
    }
}
