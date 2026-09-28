using AutoMapper;

using StudyTime.Application.Dtos;
using StudyTime.Domain.Entities;

namespace StudyTime.Application.Mappings;

/// <summary>
/// Fonte composta para projeção: agrega as três entidades que compõem
/// uma configuração semanal (StudyAreaWeek + StudyAreaWeekAssessment +
/// WeeklyAssessment) para produzir um StudyAreaWeekDto.
/// </summary>
public sealed record StudyAreaWeekMappingSource(
    StudyAreaWeek Week,
    StudyAreaWeekAssessment Assessment,
    WeeklyAssessment Weekly);

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<StudyAreaWeekMappingSource, StudyAreaWeekDto>()
            .ForCtorParam(
                nameof(StudyAreaWeekDto.Id),
                o => o.MapFrom(s => s.Week.Id))
            .ForCtorParam(
                nameof(StudyAreaWeekDto.WeekStartDate),
                o => o.MapFrom(s => s.Week.WeekStartDate))
            .ForCtorParam(
                nameof(StudyAreaWeekDto.StudyAreaId),
                o => o.MapFrom(s => s.Week.StudyAreaId))
            .ForCtorParam(
                nameof(StudyAreaWeekDto.StudyPlanId),
                o => o.MapFrom(s => s.Week.StudyPlanId))
            .ForCtorParam(
                nameof(StudyAreaWeekDto.WeeklyAssessmentId),
                o => o.MapFrom(s => s.Week.WeeklyAssessmentId))
            .ForCtorParam(
                nameof(StudyAreaWeekDto.WeekIndividualGoal),
                o => o.MapFrom(s => s.Assessment.WeekIndividualGoal))
            .ForCtorParam(
                nameof(StudyAreaWeekDto.MinutesStudied),
                o => o.MapFrom(s => s.Assessment.MinutesStudied))
            .ForCtorParam(
                nameof(StudyAreaWeekDto.GoalAchieved),
                o => o.MapFrom(s =>
                    s.Assessment.MinutesStudied >= s.Assessment.WeekIndividualGoal));
    }
}