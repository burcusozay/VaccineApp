using AutoMapper;

namespace VaccineApp.Business.Base
{
    public class BaseService<TEntity, TDto>
    {
        protected readonly IMapper _mapper;

        protected BaseService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public virtual TEntity MapToEntity(TDto dto)
        {
            return _mapper.Map<TEntity>(dto);
        }

        public virtual TDto MapToDto(TEntity entity)
        {
            return _mapper.Map<TDto>(entity);
        }

        public virtual IEnumerable<TDto> MapToDtoList(IEnumerable<TEntity> entities)
        {
            return _mapper.Map<IEnumerable<TDto>>(entities);
        }
    }
}
