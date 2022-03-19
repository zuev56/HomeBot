using System;

namespace Home.Services.Vk.Paging;

public class Paging
{
    public int CurrentPage { get; set; }
    public int RecordsOnPage { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalRecords / (float)RecordsOnPage);
}
