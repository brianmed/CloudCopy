using System.Data;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using CloudCopy.Server.DbContexts;
using CloudCopy.Server.Entities;

namespace CloudCopy.Server.Repositories
{
    public interface IAppRepository
    {
        Task<AppEntity> ReadAsync(long id);
        Task<bool> DeleteAsync(long id);
        Task<bool> DeleteAsync(AppEntity app);
        Task<AppEntity> UpdateAsync(AppEntity app);
    }

    public class AppRepository : IAppRepository
    {
        private CloudCopyDbContext DbContext;
        public AppRepository(
            CloudCopyDbContext dbContext
        )
        {
            DbContext = dbContext;
        }

        async public Task<AppEntity> ReadAsync(long id)
        {
            return await DbContext.App
                .Where(v => v.AppEntityId == id)
                .SingleAsync();
        }

        async public Task<AppEntity> UpdateAsync(AppEntity app)
        {
            DbContext.App.Attach(app);

            await DbContext.SaveChangesAsync();

            return app;
        }

        async public Task<bool> DeleteAsync(long id)
        {
            AppEntity app = await DbContext.App
                .FirstOrDefaultAsync(t => t.AppEntityId == id);

            if (app != null)
            {
                DbContext.Remove(app);
                await DbContext.SaveChangesAsync();

                return true;
            } else {
                return false;
            }
        }

        async public Task<bool> DeleteAsync(AppEntity app)
        {
            return await DeleteAsync(app.AppEntityId);
        }
    }
}
