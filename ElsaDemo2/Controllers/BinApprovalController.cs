using Elsa;
using Elsa.Extensions;
using Elsa.Models;
using Elsa.Persistence.EntityFrameworkCore.DbContexts;
using Elsa.Services;
using Elsa.Services.Workflows;
using Elsa.Workflows.Contracts;
using Elsa.Workflows.Management.Filters;
using Elsa.Workflows.Management.Mappers;
using Elsa.Workflows.Management.Services;

using ElsaDemo2.WorkFlows;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;

using IWorkflowRunner = Elsa.Services.IWorkflowRunner;

namespace ElsaDemo2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BinApprovalController : ControllerBase
    {
        private readonly IWorkflowRegistry _workflowRegistry;
        private readonly IWorkflowLaunchpad _workflowLaunchpad;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IDbContextFactory<ElsaContext> _dbContextFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWorkflowRunner _workflowRunner;
        private readonly IStartsWorkflow _workflowStarter;


        public BinApprovalController(IWorkflowRegistry workflowRegistry, 
            IWorkflowLaunchpad workflowLaunchpad, 
            IServiceScopeFactory scopeFactory, IDbContextFactory<ElsaContext> dbContextFactory,
            IServiceProvider serviceProvider, IWorkflowRunner workflowRunner, IStartsWorkflow workflowStarter)
        {
            _workflowRegistry = workflowRegistry;
            _workflowLaunchpad = workflowLaunchpad;
            _scopeFactory = scopeFactory;
            _serviceProvider = serviceProvider;
            _dbContextFactory = dbContextFactory;
            _workflowRunner = workflowRunner;
            _workflowStarter = workflowStarter;
        }

        [HttpGet("run")]
        public async Task<IActionResult> Run(CancellationToken cancellationToken)
        {
            var myWorkflowBlueprint = (await _workflowRegistry.GetWorkflowAsync<TestingWorkflow>(cancellationToken))!;

            await _workflowStarter.StartWorkflowAsync(myWorkflowBlueprint, cancellationToken: cancellationToken);

            return Ok();
        }

        [HttpPost("v1/kaja")]
        public async Task<IActionResult> Post([FromBody] BinRequestModel binRequest)
        {
            var versionOptions = VersionOptions.Latest;
            var workflowBlueprint = await _workflowRegistry.FindAsync("TestingWorkflow", versionOptions);
            if (workflowBlueprint == null)
            {
                return NotFound("Workflow not found.");
            }

            // var input = new Variables(new { Body = binRequest });
            var xx = new WorkflowInstance { Id= "", DefinitionId = "" };
            var runWorkflowResult = await _workflowRunner.RunWorkflowAsync(workflowBlueprint,xx, "33");

            if (runWorkflowResult.WorkflowInstance.WorkflowStatus == WorkflowStatus.Faulted)
            {
                return StatusCode(500, "An error occurred while processing the workflow.");
            }

            return Ok(new
            {
                Message = "Request for Approval Sent. Your bin has been received and will be reviewed shortly."
            });
        }


        [HttpPost("v1/bins")]
        public async Task<IActionResult> Post([FromBody] BinRequestModel request, CancellationToken cancellationToken)
        {

            using var scope = _serviceProvider.CreateScope();
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ElsaContext>>();

            using var dbContext = dbContextFactory.CreateDbContext();

            var data =  dbContext.WorkflowInstances.FirstOrDefault();


            return Ok("<h1>Request for Approval Sent</h1><p>Your bin has been received and will be reviewed shortly.</p>");
        }

        public class BinRequestModel
        {
            public string BinId { get; set; }
            // Add additional properties as needed
        }
    }
}