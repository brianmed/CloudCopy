using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using Mapster;

using CloudCopy.Server.DbContexts;
using CloudCopy.Server.Entities;
using CloudCopy.Server.Repositories;
using CloudCopy.Shared.Forms;

namespace CloudCopy.Server.Controllers
{
    [ApiController]
    [Route("v1/[controller]/[action]")]
    [Authorize]
    public class CopiedController : ControllerBase
    {
        private CloudCopyDbContext CloudCopyDbContext { get; init; }

        private ICopiedRepository CopiedRepository { get; init; }

        private ICopiedService CopiedService { get; init; }

        public CopiedController(
            ICopiedRepository copiedRepository,
            ICopiedService copiedService,
            CloudCopyDbContext cloudCopyDbContext)
        {
            CloudCopyDbContext = cloudCopyDbContext;

            CopiedRepository = copiedRepository;

            CopiedService = copiedService;
        }

        [HttpGet("{*body}")]
        async public Task<CopiedForm> Create([FromRoute]string body)
        {
            CopiedEntity copiedEntity = CopiedService.Entity(body);

            return (await CopiedRepository
                .CreateAsync(copiedEntity))
                .Adapt<CopiedForm>();
        }

        [HttpGet]
        async public Task<IEnumerable<CopiedForm>> Read()
        {
            return (await CopiedRepository
                .ReadAsync())
                .Adapt<IEnumerable<CopiedForm>>();
        }

        [HttpGet]
        async public Task<JsonResult> Latest()
        {
            CopiedForm form = (await CloudCopyDbContext.Copies
                .OrderByDescending(v => v.CopiedEntityId)
                .Take(1)
                .SingleOrDefaultAsync())
                .Adapt<CopiedForm>();

            return new JsonResult(new { Success = true, Copy = form });
        }
    }
}
