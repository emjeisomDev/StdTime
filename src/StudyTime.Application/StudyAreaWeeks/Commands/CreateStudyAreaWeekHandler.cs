using StudyTime.Domain.Enums;
using StudyTime.Domain.Services;
using StudyTime.Domain.Entities;
using StudyTime.Application.Dtos;
using StudyTime.Domain.Exceptions;
using StudyTime.Domain.ValueObjects;
using StudyTime.Domain.Abstractions;
using StudyTime.Domain.Abstractions.Repositories;
using StudyTime.Application.Abstractions.Messaging;

namespace StudyTime.Application.StudyAreaWeeks.Commands;

public sealed class CreateStudyAreaWeekHandler
    : ICommandHandler<CreateStudyAreaWeekCommand, StudyAreaWeekDto>
{
    private readonly IStudyAreaWeekRepository _weekRepo;
    private readonly IStudyAreaRepository _studyAreaRepo;
    private readonly IStudyPlanRepository _studyPlanRepo;
    private readonly IStudyAreaWeekAssessmentRepository _assessmentRepo;
    private readonly IWeeklyAssessmentRepository _weeklyRepo;
    private readonly IIsoWeekCalendar _isoCalendar;

    public CreateStudyAreaWeekHandler(
        IStudyAreaWeekRepository weekRepo,
        IStudyAreaRepository studyAreaRepo,
        IStudyPlanRepository studyPlanRepo,
        IStudyAreaWeekAssessmentRepository assessmentRepo,
        IWeeklyAssessmentRepository weeklyRepo,
        IIsoWeekCalendar isoCalendar)
    {
        _weekRepo = weekRepo;
        _studyAreaRepo = studyAreaRepo;
        _studyPlanRepo = studyPlanRepo;
        _assessmentRepo = assessmentRepo;
        _weeklyRepo = weeklyRepo;
        _isoCalendar = isoCalendar;
    }

    public async Task<StudyAreaWeekDto> Handle(CreateStudyAreaWeekCommand request, CancellationToken cancellationToken)
    {
        var studyArea = await _studyAreaRepo.GetByIdAsync(request.StudyAreaId, cancellationToken);

        if (studyArea is null)
        {
            throw new DomainValidationException("StudyArea not found.");
        }

        var studyPlan = await _studyPlanRepo.GetByIdAsync(request.StudyPlanId, cancellationToken);

        if (studyPlan is null)
        {
            throw new DomainValidationException("StudyPlan not found.");
        }

        if (studyPlan.Status != StudyPlanStatus.Active)
        {
            throw new DomainValidationException("StudyPlan must be active (R04).");
        }

        var existingWeeks = await _weekRepo.GetByWeekStartDateAsync(request.WeekStartDate, cancellationToken);

        if (existingWeeks.Any(week => week.StudyAreaId == request.StudyAreaId))
        {
            throw new DuplicateStudyAreaWeekException();
        }

        var newIndividualGoal = GoalCalculator.Calculate(studyArea.StdWeekStudyTime, new Coefficient(studyPlan.Coefficient));

        var iso = _isoCalendar.FromDate(request.WeekStartDate);

        var weekly = await _weeklyRepo.GetByYearAndWeekAsync(iso.Year, iso.WeekNumber, cancellationToken);

        decimal sumExistingGoals = 0;

        foreach (var existingWeek in existingWeeks)
        {
            var existingAssessment = await _assessmentRepo.GetByStudyAreaWeekIdAsync(existingWeek.Id, cancellationToken);

            if (existingAssessment is not null)
            {
                sumExistingGoals += existingAssessment.WeekIndividualGoal;
            }
        }

        var newGlobalGoal = sumExistingGoals + newIndividualGoal;

        WeeklyGoalValidator.Validate(newGlobalGoal);

        if (weekly is null)
        {
            weekly = new WeeklyAssessment(
                Guid.NewGuid(),
                iso.WeekNumber,
                iso.Year,
                newGlobalGoal);

            await _weeklyRepo.AddAsync(weekly, cancellationToken);
        }
        else
        {
            weekly.UpdateGoals(newGlobalGoal);
            _weeklyRepo.Update(weekly);
        }

        var week = new StudyAreaWeek(
            Guid.NewGuid(),
            request.WeekStartDate,
            request.StudyAreaId,
            request.StudyPlanId,
            weekly.Id);

        await _weekRepo.AddAsync(week, cancellationToken);

        var assessment = new StudyAreaWeekAssessment(
            Guid.NewGuid(),
            newIndividualGoal,
            week.Id);

        await _assessmentRepo.AddAsync(assessment, cancellationToken);

        return new StudyAreaWeekDto(
            week.Id,
            week.WeekStartDate,
            week.StudyAreaId,
            week.StudyPlanId,
            week.WeeklyAssessmentId,
            assessment.WeekIndividualGoal,
            assessment.MinutesStudied,
            assessment.MinutesStudied >= assessment.WeekIndividualGoal);
    }
}