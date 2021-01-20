using System.Net;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using CloudCopy.Server.DbContexts;
using CloudCopy.Server.Entities;

namespace CloudCopy.Server.Bll
{
    public class OptionsBl
    {
        public AdminOptionsEntity AdminOptionsRecord { get; set; } = new AdminOptionsEntity();
    
        async public Task CreateRecordAsync()
        {
            using CloudCopyDbContext jbDbContext = new CloudCopyDbContext();

            await jbDbContext.AdminOptions.AddAsync(new AdminOptionsEntity
            {
                AdminOptionsEntityId = 1,
                SiteUrl = $"https://{Dns.GetHostName()}"
            });

            await jbDbContext.SaveChangesAsync();
        }

        async public Task<bool> HasRecord()
        {
            using CloudCopyDbContext jbDbContext = new CloudCopyDbContext();
            return await jbDbContext.AdminOptions.CountAsync() > 0;
        }

        async public Task LoadRecord()
        {
            using CloudCopyDbContext jbDbContext = new CloudCopyDbContext();
            AdminOptionsRecord = await jbDbContext.AdminOptions.SingleAsync();
        }
    }
}

