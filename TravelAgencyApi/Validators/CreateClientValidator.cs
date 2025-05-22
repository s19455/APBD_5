using FluentValidation;
using TravelAgencyApi.DTOs;

namespace TravelAgencyApi.Validators;

public class CreateClientValidator : AbstractValidator<CreateClientDto>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Telephone).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Pesel)
            .Matches(@"^\d{11}$")
            .WithMessage("PESEL powinien mieć 11 cyfr");
    }
}