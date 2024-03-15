using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Messenger.Services.DatabaseHelper
{
    public interface IDbServices
    {
        Task<TEntity> GetAllUserAsync<TEntity>() where TEntity : class;
        Task<TEntity> GetUserByIdAsync<TEntity>(Guid Id) where TEntity : class;

        Task<TEntity> GetUserByEmailAsync<TEntity>(string email) where TEntity : class;

        Task Delete<TEntity>(Guid Id) where TEntity : class;

        Task UpdateAsync<TValue, TEntity>(Guid Id, TValue ChangeValue, string Property) where TEntity : class;
    }
    public class DbServices<T> : DbContext where T : DbContext
    {
        private readonly T _ctx;
        public DbServices(T ctx)
        {
            _ctx = ctx;
        }
        public  async Task<List<TEntity>> GetAllUserAsync<TEntity>()where TEntity :class
        {
            return await _ctx.Set<TEntity>().ToListAsync();
        }
        
        public async Task<TEntity> GetUserByIdAsync<TEntity>(Guid Id) where TEntity : class
        {


            return await _ctx.Set<TEntity>().FindAsync(Id);
        }

        public async Task<TEntity> GetUserByEmailAsync<TEntity>(string email ) where TEntity : class
        {


            return await _ctx.Set<TEntity>().FirstOrDefaultAsync(e => EF.Property<string>(e, "Email") == email);
        }
        public async Task CreateAsync<TValue>(TValue infomation)
        {
           await _ctx.AddAsync(infomation);
           await _ctx.SaveChangesAsync();
        }

        public async Task UpdateAsync<TValue,TEntity>(Guid Id, TValue ChangeValue , string Property ) where TEntity : class
        {

            var entry = _ctx.Entry<TEntity>(_ctx.Set<TEntity>().Local.SingleOrDefault(e => EF.Property<Guid>(e, "Id") == Id) ?? await _ctx.Set<TEntity>().FindAsync(Id));

            if (entry != null)
            {
                entry.Property(Property).CurrentValue = ChangeValue;
                await _ctx.SaveChangesAsync();
            }
                
        }
        // k có được tốt bởi vì nếu cái property nó không phải là status thì làm sao rồi nó k trả về cái gì hết thì làm sao mà biết là nó có ghi được vào db hay là không
        public async Task Delete<TEntity>(Guid Id) where TEntity : class
        {
            var entry = _ctx.Entry<TEntity>(_ctx.Set<TEntity>().Local.SingleOrDefault(e => EF.Property<Guid>(e, "Id") == Id) ?? await _ctx.Set<TEntity>().FindAsync(Id));

            if(entry != null)
            {
                entry.Property("Status").CurrentValue = 0;
            }

        }
    }
}
