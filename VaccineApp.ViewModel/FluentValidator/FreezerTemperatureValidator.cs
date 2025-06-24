using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.ViewModel.FluentValidator
{
    public class FreezerTemperatureValidator : AbstractValidator<FreezerTemperatureDto>
    {
        public FreezerTemperatureValidator()
        {
            RuleFor(x => x.FreezerId)
                .GreaterThan(0).WithMessage("Dolap id si 0'dan büyük olmalıdır.");

            RuleFor(x => x.Temperature)
                .GreaterThan(x => -20)
                .WithMessage("Sıcaklık değeri, -20'den büyük olmalıdır.");

            RuleFor(x => x.CreatedDate)
                .GreaterThan(x => DateTime.Now.AddDays(-1))
                .WithMessage("Oluşturulma tarihi, en eski dün olmalıdır.");
        }
    }
}
