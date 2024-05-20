using Elsa;
using Elsa.Activities.Http.Models;
using Elsa.Activities.Signaling.Services;
using Elsa.Extensions;
using Elsa.Models;
using Elsa.Persistence.EntityFrameworkCore.DbContexts;
using Elsa.Services;
using Elsa.Services.Models;
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
using System.Net;

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
        private readonly IWorkflowTriggerInterruptor _workflowTriggerInterruptor;
        private readonly Elsa.Services.IWorkflowRunner _workflowRunner;
        private readonly IStartsWorkflow _workflowStarter;

        public BinApprovalController(IWorkflowRegistry workflowRegistry,
            IWorkflowLaunchpad workflowLaunchpad,
            IServiceScopeFactory scopeFactory,
            IDbContextFactory<ElsaContext> dbContextFactory,
            IServiceProvider serviceProvider,
            IWorkflowTriggerInterruptor workflowTriggerInterruptor,
            Elsa.Services.IWorkflowRunner workflowRunner,
            IStartsWorkflow workflowStarter)
        {
            _workflowRegistry = workflowRegistry;
            _workflowLaunchpad = workflowLaunchpad;
            _scopeFactory = scopeFactory;
            _serviceProvider = serviceProvider;
            _dbContextFactory = dbContextFactory;
            _workflowTriggerInterruptor = workflowTriggerInterruptor;
            _workflowRunner = workflowRunner;
            _workflowStarter = workflowStarter;
        }

        [HttpPost("v1/bins")]
        public async Task<IActionResult> Post([FromBody] BinRequestModel request, CancellationToken cancellationToken)
        {

            using var scope = _serviceProvider.CreateScope();
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ElsaContext>>();

            using var dbContext = dbContextFactory.CreateDbContext();

            var data = await dbContext.WorkflowInstances.ToListAsync();


            //using (var scope = _scopeFactory.CreateScope())
            //{
            //   // var elsaContext = scope.ServiceProvider.GetRequiredService<ElsaContext>();
            //    //var allVersions = await elsaContext.WorkflowDefinitions.ToListAsync(cancellationToken);
            //    //var latestVersion = allVersions.OrderByDescending(x => x.Version).FirstOrDefault();
            //}

            return Ok("<h1>Request for Approval Sent</h1><p>Your bin has been received and will be reviewed shortly.</p>");
        }

        [HttpPost("initiate-workflow")]
        public async Task<IActionResult> InitiateWorkflow([FromBody] InitiateRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var myWorkflowBlueprint = await _workflowRegistry.GetWorkflowAsync<TestingWorkflow>(cancellationToken);
                if (myWorkflowBlueprint == null)
                {
                    return BadRequest("Workflow blueprint not found");
                }

                var workflowInput = new WorkflowInput(request);
                var result = await _workflowStarter.StartWorkflowAsync(myWorkflowBlueprint, cancellationToken: cancellationToken); // input: workflowInput,

                if (result == null || result.WorkflowInstance == null || string.IsNullOrEmpty(result.WorkflowInstance.Id))
                {
                    return StatusCode(500, "Failed to start workflow");
                }

                return Ok(new { InstanceId = result.WorkflowInstance.Id });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here as needed
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("approve/{instanceId}")]
        public async Task<IActionResult> Approve(string instanceId, ApproveRequest request, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var signaler = scope.ServiceProvider.GetRequiredService<ISignaler>();

                if (string.IsNullOrEmpty(instanceId))
                {
                    return BadRequest("InstanceId cannot be null or empty");
                }

                var workflowInput = new WorkflowInput(request);

                var result = await signaler.TriggerSignalAsync("Approve/Reject", input: workflowInput, instanceId, cancellationToken: cancellationToken);

                if (result == null)
                {
                    return StatusCode(500, "Failed to trigger signal");
                }

                return Ok(new { InstanceId = instanceId, Status = request.ApproveStatus ? "Approved" : "Rejected" });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here as needed
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class BinRequestModel
        {
            public string BinId { get; set; }
            // Add additional properties as needed
        }

        public class InitiateRequest
        {
            public string BinId { get; set; }
            public string User { get; set; }
        }

        public class ApproveRequest
        {
            public bool ApproveStatus { get; set; }
        }
    }
}