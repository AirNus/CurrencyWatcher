namespace CurrencyWatcher.DAL.Interfaces
{
    public interface IBaseRepository<T>
    {
        Task Create(T entity);

        IQueryable<T> GetAll();

        Task Delete(T entity);
        Task DeleteList(DateTime from, DateTime to, string currency);

        Task<T> Update(T entity);

        void Add(T entity);

        void AddList(List<T> entity);

        int GetMaxId();
    }
}
