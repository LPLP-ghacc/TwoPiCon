using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoPiCon.Models.Text.UserProfile;

public enum StatusState
{
    Online,
    Away,
    DoNotВisturb
}

public class UserProfileMessage
{
    public string Title { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PublicProfile
{
    public string UserName { get; set; }
    public string Description { get; set; }
    public string Email { get; set; }
    public string Status { get; set; }
    public string PhoneNumber { get; set; }
    public UserProfileMessage Message { get; set; }
    public List<string> Links { get; set; }
}
