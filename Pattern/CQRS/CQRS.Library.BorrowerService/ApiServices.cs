﻿using CQRS.Library.BorrowerService.Infrastructure.Data;

namespace CQRS.Library.BorrowerService
{
    public class ApiServices(BorrowerDbContext context)
    {
        public BorrowerDbContext Context => context;
    }
}
