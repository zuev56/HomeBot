using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Home.Web.Areas.App.Models.Vk;

public class VkVM
{
    //[Display(Name = "Name Filter")]
    //public string UserNameFilter { get; set; }
    [Display(Name = "Дата начала")]
    public DateTime FromDate { get; set; }
    [Display(Name = "Дата окончания")]
    public DateTime ToDate { get; set; }



    public List<UserVM> VkUsers { get; set; }

    public VkVM()
    {
        FromDate = DateTime.UtcNow.Date - TimeSpan.FromDays(1);
        ToDate = DateTime.UtcNow.Date;
    }
}