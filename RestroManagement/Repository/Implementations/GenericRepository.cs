using Microsoft.EntityFrameworkCore;
using RestroManagement.Data;
using RestroManagement.Repository.Interfaces;

namespace RestroManagement.Repository.Implementations
{
    public class GenericRepository<T> : IGeneric<T> where T : class
    {
        protected ApplicationDbContext _context { get; set; }
       

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
            
        }
        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
    
}
