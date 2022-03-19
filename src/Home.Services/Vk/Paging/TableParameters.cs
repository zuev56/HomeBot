using System;

namespace Home.Services.Vk.Paging;

/// <summary>Universal table parameters for tables with paging</summary>
public class TableParameters
{
    // TODO: It is necessary to make a list of universal filters of different types and with different operators
    public string FilterText { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    //public SortOrder SortOrder { get; set; }

    public Paging Paging { get; set; }
}
