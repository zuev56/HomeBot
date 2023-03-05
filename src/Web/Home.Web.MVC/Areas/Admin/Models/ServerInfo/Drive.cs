using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Home.Web.Areas.Admin.Models.ServerInfo;

public record Drive
{
    [Display(Name = "Label")]
    public string Name { get; internal set; }
    [Display(Name = "Total Size, MB")]
    public double? TotalSize { get; internal set; }
    [Display(Name = "Free Space, MB")]
    public double? FreeSpace { get; internal set; }
    [Display(Name = "File System")]
    public string FileSystem { get; internal set; }
    [Display(Name = "Type")]
    public DriveType Type { get; internal set; }
}