using System.Linq.Expressions;
using HotelBooking.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly HotelDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(HotelDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking().ToListAsync(ct);

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await DbSet.AsNoTracking().Where(predicate).ToListAsync(ct);

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        DbSet.Remove(entity);
        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await DbSet.AnyAsync(predicate, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await Context.SaveChangesAsync(ct);
}
