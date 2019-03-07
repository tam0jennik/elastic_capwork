using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace website.DataLayer
{
    public class SelectSettings
    {
        public SortOrder Order { get; set; }
        public string SortBy { get; set; }
        public ShardSource Source { get; set; }

        public int Size { get; set; }

        public static SelectSettings Default
        {
            get
            {
                return new SelectSettings
                {
                    Order = SortOrder.Descending,
                    SortBy = "_score",
                    Source = ShardSource.random,
                    Size = 10
                };
            }
        }
    }
}
