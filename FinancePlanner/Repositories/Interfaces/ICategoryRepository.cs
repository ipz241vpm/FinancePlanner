namespace FinancePlanner.Repositories.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    List<Category> GetByType(string type);
}