using System.ComponentModel.DataAnnotations;

namespace Home.Web.Areas.Admin.Models.ServerInfo;

public class Process
{
    [Display(Name = "Name")]
    public string Name { get; internal set; }

    [Display(Name = "Responding")]
    public bool IsResponding { get; internal set; }

    [Display(Name = "Threads Number")]
    public int DuplicatesInName { get; internal set; }
    public int ThreadsNumber { get; internal set; }
}