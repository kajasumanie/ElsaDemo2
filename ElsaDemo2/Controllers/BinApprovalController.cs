using Elsa.Extensions;
using Elsa.Persistence.EntityFrameworkCore.DbContexts;
using Elsa.Services;
using Elsa.Workflows.Contracts;
using Elsa.Workflows.Management.Filters;
using Elsa.Workflows.Management.Mappers;
using Elsa.Workflows.Management.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System;

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


        public BinApprovalController(IWorkflowRegistry workflowRegistry, IWorkflowLaunchpad workflowLaunchpad, IServiceScopeFactory scopeFactory, IDbContextFactory<ElsaContext> dbContextFactory, IServiceProvider serviceProvider)
        {
            _workflowRegistry = workflowRegistry;
            _workflowLaunchpad = workflowLaunchpad;
            _scopeFactory = scopeFactory;
            _serviceProvider = serviceProvider;
            _dbContextFactory = dbContextFactory;
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

        public class BinRequestModel
        {
            public string BinId { get; set; }
            // Add additional properties as needed
        }
    }
}