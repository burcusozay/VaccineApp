using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineApp.ViewModel.Dtos
{
    /// <summary>
    /// Sayfalanmış API yanıtları için standart bir yapı sağlar.
    /// </summary>
    /// <typeparam name="T">Döndürülecek veri listesinin tipi.</typeparam>
    public class ServiceResponseDto<T>
    {
        /// <summary>
        /// Geçerli sayfadaki veri listesi.
        /// React tarafında 'items' veya 'Items' olarak aranacak.
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// İsteğin yapıldığı sayfa numarası.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Toplam sayfa sayısı.
        /// </summary>
        public int TotalPages { get; set; } = 0;

        /// <summary>
        /// Filtrelenmiş toplam kayıt sayısı.
        /// </summary>
        public int TotalCount { get; set; }  = 0;

        public ServiceResponseDto()
        {
            Items = new List<T>();
            CurrentPage = 1;
        }

        public ServiceResponseDto(List<T> items, int count, int? pageNumber, int? pageSize) : this()
        {
            TotalCount = count;
            CurrentPage = pageNumber.HasValue ? pageNumber.Value : 1;
            Items = items;
            // Toplam sayfa sayısını hesapla
            TotalPages = pageSize.HasValue ? (int)Math.Ceiling(count / (double)pageSize) : 0;
        }
    }
}
