using Microsoft.EntityFrameworkCore;
using Sql2Json.Core.Data;
using Sql2Json.Core.Models;

namespace Sql2Json.Core.Services;

/// <summary>
/// 查询历史服务实现
/// 管理常用 SQL 的保存、检索和使用统计
/// </summary>
public class QueryHistoryService : IQueryHistoryService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public QueryHistoryService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc/>
    public async Task<List<QueryHistory>> GetAllAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.QueryHistories
            .OrderByDescending(x => x.LastUsedAt ?? x.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<QueryHistory>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return await GetAllAsync();

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.QueryHistories
            .Where(x => x.Name.Contains(keyword) || x.Sql.Contains(keyword))
            .OrderByDescending(x => x.LastUsedAt ?? x.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<QueryHistory> SaveAsync(QueryHistory history)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.QueryHistories.Add(history);
        await db.SaveChangesAsync();
        return history;
    }

    /// <inheritdoc/>
    public async Task MarkUsedAsync(Guid id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var history = await db.QueryHistories.FindAsync(id);
        if (history != null)
        {
            history.UsageCount++;
            history.LastUsedAt = DateTime.Now;
            await db.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var history = await db.QueryHistories.FindAsync(id);
        if (history != null)
        {
            db.QueryHistories.Remove(history);
            await db.SaveChangesAsync();
        }
    }
}
