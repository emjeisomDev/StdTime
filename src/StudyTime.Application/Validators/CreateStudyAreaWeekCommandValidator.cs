using FluentValidation;
using StudyTime.Application.StudyAreaWeeks.Commands;

namespace StudyTime.Application.Validators;

public sealed class CreateStudyAreaWeekCommandValidator : AbstractValidator<CreateStudyAreaWeekCommand>
{
    public CreateStudyAreaWeekCommandValidator()
    {
        RuleFor(x => x.StudyAreaId)
            .NotEmpty()
            .WithMessage("StudyAreaId é obrigatório.");

        RuleFor(x => x.StudyPlanId)
            .NotEmpty()
            .WithMessage("StudyPlanId é obrigatório.");

        RuleFor(x => x.WeekStartDate)
            .Must(d => d.DayOfWeek == DayOfWeek.Monday)
            .WithMessage("A data de início da semana deve ser uma segunda-feira (R07).");
    }
}