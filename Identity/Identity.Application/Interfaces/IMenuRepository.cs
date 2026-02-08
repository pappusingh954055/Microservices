using Identity.Domain.Menus;

namespace Identity.Application.Interfaces;

public interface IMenuRepository
{
    Task<IEnumerable<Menu>> GetMenuByUserIdAsync(Guid userId);
    Task<IEnumerable<Menu>> GetAllMenusAsync();
    Task<Menu?> GetByIdAsync(int id);
    Task AddAsync(Menu menu);
    Task UpdateAsync(Menu menu);
    Task DeleteAsync(int id);
}
