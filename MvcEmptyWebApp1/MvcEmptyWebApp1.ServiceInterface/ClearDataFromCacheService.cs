using MvcEmptyWebApp1.ServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcEmptyWebApp1.ServiceInterface
{
    class ClearDataFromCacheService : Service
    {
        public ClearDataFromCacheResponse Any(ClearDataFromCache model)
        {
            var cacheKey = "HeyYou";
            base.Request.RemoveFromCache(base.Cache, cacheKey);
            return new ClearDataFromCacheResponse();
        }
    }
}
