﻿using Microsoft.AspNetCore.Mvc;
using MidnightLizard.Schemes.Commander.Infrastructure.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MidnightLizard.Schemes.Commander.Requests.PublishScheme
{
    [SchemaVersion("*")]
    public class UnpublishSchemeRequestDeserializer_Latest : AggregateIdRequestDeserializer<UnpublishSchemeRequest>
    {
    }
}
