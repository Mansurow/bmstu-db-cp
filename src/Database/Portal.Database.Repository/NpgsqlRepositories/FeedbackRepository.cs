using Microsoft.EntityFrameworkCore;
using Portal.Common.Converter;
using Portal.Common.Models;
using Portal.Common.Models.Enums;
using Portal.Database.Context;
using Portal.Database.Core.Repositories;

namespace Portal.Database.Repositories.NpgsqlRepositories;

public class FeedbackRepository: BaseRepository, IFeedbackRepository
{
    private readonly IDbContextFactory _contextFactory;

    public FeedbackRepository(IDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    public Task<List<Feedback>> GetAllFeedbackByZoneAsync(Guid zoneId, Role role)
    {
        return _contextFactory.GetDbContext(role)
            .Feedbacks
            .Where(f => f.ZoneId == zoneId)
            .OrderBy(f => f.Date)
            .Select(f => FeedbackConverter.ConvertDbModelToAppModel(f))
            .ToListAsync();
    }

    public Task<List<Feedback>> GetAllFeedbackUserAsync(Guid userId, Role role)
    {
        return _contextFactory.GetDbContext(role)
            .Feedbacks
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.Date)
            .Select(f => FeedbackConverter.ConvertDbModelToAppModel(f))
            .ToListAsync();
    }

    public Task<List<Feedback>> GetAllFeedbackAsync(Role role)
    {
        return _contextFactory.GetDbContext(role)
            .Feedbacks
            .OrderBy(f => f.Date)
            .Select(f => FeedbackConverter.ConvertDbModelToAppModel(f))
            .ToListAsync();
    }

    public async Task<Feedback> GetFeedbackAsync(Guid feedbackId, Role role)
    {
        var feedback = await _contextFactory.GetDbContext(role)
            .Feedbacks.FirstAsync(f => f.Id == feedbackId);

        return FeedbackConverter.ConvertDbModelToAppModel(feedback);
    }

    public async Task InsertFeedbackAsync(Feedback feedback, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var feedbackDb = FeedbackConverter.ConvertAppModelToDbModel(feedback);
        
        await context.Feedbacks.AddAsync(feedbackDb);
        await context.SaveChangesAsync();
    }

    public async Task UpdateFeedbackAsync(Feedback feedback, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var feedbackDb = await context.Feedbacks.FirstAsync(f => f.Id == feedback.Id);

        feedbackDb.Mark = feedback.Mark;
        feedbackDb.Message = feedback.Message;
        // feedbackDb.ChangeTime = feedback.ChangeTime; // TODO: добавить время изменения
        // Возможно флаг изменения IsChanged    
        
        await context.SaveChangesAsync();
    }

    public async Task DeleteFeedbackAsync(Guid feedbackId, Role role)
    {
        var context = _contextFactory.GetDbContext(role);
        var feedbackDb = await context.Feedbacks.FirstAsync(f => f.Id == feedbackId);

        context.Feedbacks.Remove(feedbackDb);
        await context.SaveChangesAsync();
    }
}