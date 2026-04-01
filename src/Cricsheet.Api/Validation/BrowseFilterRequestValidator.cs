using FluentValidation;

namespace Cricsheet.Api.Validation;

internal sealed class BrowseFilterRequestValidator : AbstractValidator<BrowseFilterRequest>
{
    public BrowseFilterRequestValidator()
    {
        RuleFor(request => request)
            .Must(HasAtLeastOneFilter)
            .WithMessage("At least one filter field must be provided.");

        RuleFor(request => request)
            .Must(HasValidDateRange)
            .WithMessage("fromDate must be less than or equal to toDate.");

        RuleFor(request => request.Gender)
            .MaximumLength(20)
            .When(request => !string.IsNullOrWhiteSpace(request.Gender));

        RuleFor(request => request.Venue)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.Venue));

        RuleFor(request => request.MatchType)
            .MaximumLength(50)
            .When(request => !string.IsNullOrWhiteSpace(request.MatchType));

        RuleFor(request => request.EventName)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.EventName));

        RuleFor(request => request.Team)
            .MaximumLength(150)
            .When(request => !string.IsNullOrWhiteSpace(request.Team));
    }

    private static bool HasAtLeastOneFilter(BrowseFilterRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Gender)
               || request.FromDate is not null
               || request.ToDate is not null
               || !string.IsNullOrWhiteSpace(request.Venue)
               || !string.IsNullOrWhiteSpace(request.MatchType)
               || !string.IsNullOrWhiteSpace(request.EventName)
               || !string.IsNullOrWhiteSpace(request.Team);
    }

    private static bool HasValidDateRange(BrowseFilterRequest request)
    {
        if (request.FromDate is null || request.ToDate is null)
        {
            return true;
        }

        return request.FromDate <= request.ToDate;
    }
}