using CurrencyWatcher.DAL.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace CurrencyWatcher.DAL.Repositories
{
    public class CurrencyRepository : IBaseRepository<Currency>
    {
        private readonly AppDbContext _appDbContext;

        public CurrencyRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task Create(Currency entity)
        {
            await _appDbContext.Currencies.AddAsync(entity);

            await _appDbContext.SaveChangesAsync();
        }
        public void Add(Currency entity)
        {
            _appDbContext.Currencies.Add(entity);

            _appDbContext.SaveChanges();
        }
        public void AddList(List<Currency> entity)
        {
            _appDbContext.Currencies.AddRange(entity);

            _appDbContext.SaveChanges();
        }

        public async Task Delete(Currency entity)
        {
            _appDbContext.Currencies.Remove(entity);

            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteList(DateTime from, DateTime to, string currency)
        {

            //_appDbContext.Currencies.RemoveRange(entity);
            _appDbContext.Currencies.Where(x => x.Code == currency && x.Dadd > from && x.Dadd < to).ExecuteDelete();

            await _appDbContext.SaveChangesAsync();
        }

        public IQueryable<Currency> GetAll()
        {
            return _appDbContext.Currencies;
        }

        public int GetMaxId()
        {
            return _appDbContext.Currencies.Max(x => x.Id);
        }

        public async Task<Currency> Update(Currency entity)
        {
            _appDbContext.Currencies.Update(entity);

            await _appDbContext.SaveChangesAsync();

            return entity;
        }
    }
}
