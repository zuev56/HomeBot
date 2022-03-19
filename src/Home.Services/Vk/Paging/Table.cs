using System;
using System.Collections.Generic;
using System.Linq;

namespace Home.Services.Vk.Paging;

public class Table<T> : TableParameters
{
    public List<T> Rows { get; set; }

    public Table(IEnumerable<T> rows, TableParameters tableParameters)
    {
        Rows = rows?.ToList() ?? throw new ArgumentNullException(nameof(rows));
        Paging = tableParameters?.Paging ?? throw new ArgumentNullException(nameof(tableParameters));
    }
}
