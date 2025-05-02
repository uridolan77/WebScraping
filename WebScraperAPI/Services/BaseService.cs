using System;
using Microsoft.Extensions.Logging;
using WebScraperApi.Data.Repositories;

namespace WebScraperApi.Services
{
    public abstract class BaseService
    {
        protected readonly IScraperRepository _repository;
        protected readonly ILogger<BaseService> _logger;

        public BaseService(IScraperRepository repository, ILogger<BaseService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
