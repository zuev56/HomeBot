using System;

namespace Home.Web.Areas.ApiVk.Models
{
    public class ActivityLogItemVM
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool? IsOnline { get; set; }
        public DateTime InsertDate { get; set; }
        public int? OnlineApp { get; set; }
        public bool IsOnlineMobile { get; set; }
        public int LastSeen { get; set; }
        //public string UserName { get; set; }

    }
}
