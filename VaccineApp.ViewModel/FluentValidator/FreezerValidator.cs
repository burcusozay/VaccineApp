using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.ViewModel.FluentValidator
{
    public class FreezerValidator : AbstractValidator<FreezerDto>
    {
        public FreezerValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Dolap adı boş olamaz.");

            RuleFor(x => x.OrderNo)
                .GreaterThan(x => 0)
                .WithMessage("Sıra numarası 0'dan büyük olmalıdır.");

            RuleFor(x => x.CreatedDate)
                .GreaterThan(x => DateTime.Now.AddDays(-1))
                .WithMessage("Oluşturulma tarihi, en eski dün olmalıdır.");
        }
    }
}
