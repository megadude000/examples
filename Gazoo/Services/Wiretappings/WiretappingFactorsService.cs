using AutoMapper;
using Company.BammBamm.Database.Entities.Campaigns;
using Company.Gazoo.Repositories.Wiretappings.Interfaces;
using Company.Gazoo.Requests.Wiretapping;
using Company.Gazoo.Services.Wiretappings.Interfaces;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Wiretappings
{
    internal class WiretappingFactorsService : IWiretappingFactorsService
    {
        private readonly IMapper mapper;
        private readonly IWiretappingFactorsRepository wiretappingFactorsRepository;

        public WiretappingFactorsService(IMapper mapper,
            IWiretappingFactorsRepository wiretappingFactorsRepository)
        {
            this.mapper = mapper;
            this.wiretappingFactorsRepository = wiretappingFactorsRepository;
        }

        public async Task AddFactorAsync(AddFactorRequest request)
        {
            var mappedFactor = mapper.Map<WiretappingFactor>(request);

            mappedFactor.Enabled = true;

            await wiretappingFactorsRepository.AddAsync(mappedFactor);
        }

        public async Task UpdateFactorAsync(UpdateFactorRequest request)
        {
            var factor = await wiretappingFactorsRepository.GetAsync(request.Id);

            factor.Name = request.Name;
            factor.AnswerType = (int)request.AnswerType;

            await wiretappingFactorsRepository.UpdateAsync(factor);
        }

        public async Task ToggleFactorAsync(long factorId)
        {
            var factor = await wiretappingFactorsRepository.GetAsync(factorId);

            factor.Enabled = !factor.Enabled;

            await wiretappingFactorsRepository.UpdateAsync(factor);
        }
    }
}
