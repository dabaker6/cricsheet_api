using FluentValidation;

namespace Cricsheet.Api.Validation;

internal sealed class MatchIdRequestValidator : AbstractValidator<MatchIdRequest>
{
    public MatchIdRequestValidator()
    {
        RuleFor(request => request.MatchId)
            .NotEmpty()
            .MaximumLength(128)
            .Matches("^[A-Za-z0-9_-]+$")
            .WithMessage("matchId must contain only letters, numbers, underscore, or hyphen.");
    }
}