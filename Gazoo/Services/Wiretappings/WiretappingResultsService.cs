using AutoMapper;
using Company.BammBamm.Database.Entities.Campaigns;
using Company.Database.Utils.Transaction.Interfaces;
using Company.Gazoo.Repositories.Users.Interfaces;
using Company.Gazoo.Repositories.Wiretappings.Interfaces;
using Company.Gazoo.Requests.Wiretapping;
using Company.Gazoo.Responses.Wiretapping;
using Company.Gazoo.Services.Interfaces;
using Company.Gazoo.Services.Wiretappings.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Gazoo.Services.Wiretappings
{
    internal class WiretappingResultsService : IWiretappingResultsService
    {
        private readonly ILogger<WiretappingResultsService> logger;
        private readonly IMapper mapper;
        private readonly ITransactionService transactionService;
        private readonly IWiretappingResultsRepository wiretappingResultsRepository;
        private readonly ICallResultsService callResultsService;

        public WiretappingResultsService(IMapper mapper,
            ILogger<WiretappingResultsService> logger,
            ITransactionService transactionService,
            IWiretappingResultsRepository wiretappingResultsRepository,
            ICallResultsService callResultsService)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.transactionService = transactionService;
            this.wiretappingResultsRepository = wiretappingResultsRepository;
            this.callResultsService = callResultsService;
        }

        public async Task<WiretapperDetailsResponse> AddResultAsync(AddWiretappingResultRequest request, long userId)
        {
            if(await wiretappingResultsRepository.IsCallWiretappedAsync(request.CallId))
            {
                logger.LogWarning($"Adding wiretapping result failed because wiretapping result already exists for this call result:{request.CallId}");
                return null;
            }

            return await transactionService.CommitAsync(new[] { TransactionContextScope.BammBamm }, async () =>
            {
                await callResultsService.MarkCallAsWiretappedAsync(request.CallId, userId);

                var wiretappingResults = mapper.Map<WiretappingResult[]>(request.WiretappingResults);

                foreach (var result in wiretappingResults)
                    result.CallResultId = request.CallId;

                await wiretappingResultsRepository.AddRangeAsync(wiretappingResults);

                return await callResultsService.GetWiretapperDetailsAsync(userId);
            });
        }

        public async Task<WiretapperDetailsResponse> UpdateResultAsync(UpdateWiretappingResultRequest request, long userId)
        {
            return await transactionService.CommitAsync(new[] { TransactionContextScope.BammBamm }, async () =>
            {
                await callResultsService.MarkCallAsWiretappedAsync(request.CallId, userId);

                var factorIds = request.WiretappingResults.Select(factor => factor.FactorId).ToArray();

                var wiretappingResults = await wiretappingResultsRepository.GetAsync(request.CallId, factorIds);

                foreach (var result in wiretappingResults)
                    result.Value = request.WiretappingResults.Where(factor => factor.FactorId == result.FactorId).Single().Value;

                await wiretappingResultsRepository.UpdateRangeAsync(wiretappingResults);

                return await callResultsService.GetWiretapperDetailsAsync(userId);
            });
        }
    }
}
