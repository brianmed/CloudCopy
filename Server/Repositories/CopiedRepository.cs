using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using CloudCopy.Server.DbContexts;
using CloudCopy.Server.Entities;

namespace CloudCopy.Server.Repositories
{
    public interface ICopiedRepository
    {
        Task<CopiedEntity> CreateAsync(CopiedEntity copy);
        Task<List<CopiedEntity>> ReadAsync();
        Task<bool> DeleteAsync(long id);
        Task<bool> DeleteAsync(CopiedEntity copy);
        Task<CopiedEntity> UpdateAsync(CopiedEntity copy);
    }

    public class CopiedRepository : ICopiedRepository
    {
        private CloudCopyDbContext DbContext;
        public CopiedRepository(
            CloudCopyDbContext dbContext
        )
        {
            DbContext = dbContext;
        }

        async public Task<CopiedEntity> CreateAsync(CopiedEntity copy)
        {
            await DbContext.Copies.AddAsync(copy);

            await DbContext.SaveChangesAsync();

            return copy;
        }

        async public Task<List<CopiedEntity>> ReadAsync()
        {
            return await DbContext.Copies
                .OrderByDescending(v => v.CopiedEntityId)
                .ToListAsync();
        }

        async public Task<CopiedEntity> UpdateAsync(CopiedEntity copy)
        {
            DbContext.Copies.Attach(copy);

            await DbContext.SaveChangesAsync();

            return copy;
        }

        async public Task<bool> DeleteAsync(long id)
        {
            CopiedEntity copy = await DbContext.Copies
                .FirstOrDefaultAsync(t => t.CopiedEntityId == id);

            if (copy != null)
            {
                DbContext.Remove(copy);
                await DbContext.SaveChangesAsync();

                return true;
            } else {
                return false;
            }
        }

        async public Task<bool> DeleteAsync(CopiedEntity copy)
        {
            return await DeleteAsync(copy.CopiedEntityId);
        }
    }
}