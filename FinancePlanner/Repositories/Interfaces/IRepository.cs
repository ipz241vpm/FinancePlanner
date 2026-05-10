namespace FinancePlanner.Repositories.Interfaces;

public interface IRepository<T>
{
    void Add(T entity);
    List<T> GetAll();
    void Update(T entity);
    void Delete(int id);
}
