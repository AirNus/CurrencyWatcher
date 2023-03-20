using CurrencyWatcher.DAL.Interfaces;
using System;

namespace CurrencyWatcher.DAL.Repositories
{
    public class ValutRepository : IBaseRepository<Valut>
    {
        private readonly AppDbContext _appDbContext;

        public ValutRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task Create(Valut entity)
        {
            await _appDbContext.Valuts.AddAsync(entity);

            await _appDbContext.SaveChangesAsync();
        }
        public void Add(Valut entity)
        {
            _appDbContext.Valuts.Add(entity);

            _appDbContext.SaveChanges();
        }

        public async Task Delete(Valut entity)
        {
            _appDbContext.Valuts.Remove(entity);

            await _appDbContext.SaveChangesAsync();
        }

        public IQueryable<Valut> GetAll()
        {
            return _appDbContext.Valuts;
        }

        public int GetMaxId()
        {
            return _appDbContext.Valuts.Max(x => x.Id);
        }

        public async Task<Valut> Update(Valut entity)
        {
            _appDbContext.Valuts.Update(entity);

            await _appDbContext.SaveChangesAsync();

            return entity;
        }
    }
}
