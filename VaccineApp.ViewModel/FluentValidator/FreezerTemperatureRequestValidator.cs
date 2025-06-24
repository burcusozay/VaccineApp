using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.ViewModel.FluentValidator
{
    public class FreezerTemperatureRequestValidator : AbstractValidator<FreezerTemperatureRequestDto>
    {
        public FreezerTemperatureRequestValidator()
        {
            //RuleFor(x => x.Page)
            //    .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

            //RuleFor(x => x.PageSize)
            //    .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            //    .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu 100'den büyük olamaz.");

            RuleFor(x => x.MaxValue)
                .GreaterThan(x => x.MinValue)
                .When(x => x.MinValue.HasValue && x.MaxValue.HasValue) // Sadece her iki değer de varsa bu kuralı çalıştır
                .WithMessage("Maksimum değer, minimum değerden büyük olmalıdır.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");
        }
    }
}
