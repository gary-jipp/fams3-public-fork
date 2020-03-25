﻿using BcGov.Fams3.SearchApi.Contracts.Person;
using BcGov.Fams3.SearchApi.Contracts.PersonSearch;
using System;
using System.Collections.Generic;
using System.Text;

namespace BcGov.Fams3.Redis.Model
{
    public class SearchRequest
    {
        public Guid SearchRequestId { get; set; }
        public Person Person { get; set; }
    }
}