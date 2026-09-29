using StudyTime.Domain.Entities;

namespace StudyTime.Domain.Abstractions.Repositories;

public interface IStudyAreaWeekAssessmentRepository
{
    public Task<StudyAreaWeekAssessment?> GetByStudyAreaWeekIdAsync(Guid studyAreaWeekId, CancellationToken token);
    public Task AddAsync(StudyAreaWeekAssessment entity, CancellationToken token);
    public void Update(StudyAreaWeekAssessment entity);
}