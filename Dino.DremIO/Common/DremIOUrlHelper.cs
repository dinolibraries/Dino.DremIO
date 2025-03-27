using System;
using System.Collections.Generic;
using System.Text;

namespace Dino.DremIO.Common
{
    public class DremIOUrlHelper
    {
        public const string SqlQuery = "/api/v3/sql";
        public const string JobResult = "/api/v3/job/{0}/results?limit={1}&offset={2}";
        public const string JobGet = "/api/v3/job/{0}";
    }
}
