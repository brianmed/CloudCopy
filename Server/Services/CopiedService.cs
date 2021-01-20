using System;

using Microsoft.AspNetCore.Http;

using CloudCopy.Server.DbContexts;
using CloudCopy.Server.Entities;

namespace CloudCopy.Server.Repositories
{
    public interface ICopiedService
    {
        CopiedEntity Entity(string body);
    }

    public class CopiedService : ICopiedService
    {
        private CloudCopyDbContext DbContext { get; init; }

        private IHttpContextAccessor HttpContextAccessor { get; init; }

        public CopiedService(
            IHttpContextAccessor httpContextAccessor,
            CloudCopyDbContext dbContext
        )
        {
            HttpContextAccessor = httpContextAccessor;
            DbContext = dbContext;
        }

        public CopiedEntity Entity(string body)
        {
            CopiedEntity copiedEntity = new CopiedEntity
            {
                Body = body,
                DayCreated = (int)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds,
            };

            copiedEntity.IpAddress = HttpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            copiedEntity.MimeType = "text/plain";

            return copiedEntity;
        }
    }
}