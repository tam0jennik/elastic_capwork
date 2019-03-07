using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace website
{
    public enum ShardSource
    {
        _primary,
        _replica,
        _primary_first,
        random
    }

}
