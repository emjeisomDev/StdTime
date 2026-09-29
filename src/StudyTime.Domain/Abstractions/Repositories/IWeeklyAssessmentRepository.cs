using StudyTime.Domain.Entities;

namespace StudyTime.Domain.Abstractions.Repositories;

public interface IWeeklyAssessmentRepository
{
    public Task<WeeklyAssessment?> GetByYearAndWeekAsync(int year, int weekNumber, CancellationToken ct);
    public Task AddAsync(WeeklyAssessment entity, CancellationToken ct);
    public void Update(WeeklyAssessment entity);
    public void Remove(WeeklyAssessment entity);
}