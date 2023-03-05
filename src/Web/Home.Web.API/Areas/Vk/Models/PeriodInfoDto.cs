﻿namespace Home.Web.API.Areas.Vk.Models;

public class PeriodInfoDto
{
    public int UserId { get; init; }
    public string UserName { get; init; }
    public int VisitsCount { get; init; }
    public string TimeInSite { get; init; }
    public string TimeInApp { get; init; }
    public string FullTime { get; init; }
}