using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwoPiCon.Models.Text.UserProfile;

public class Session
{
    public string Address { get; set; }
    public int Port { get; set; }
    public string Password { get; set; }
    public DateTime LastVisit { get; set; }
}

public class PrivateProfile
{
    public string UserID { get; set; }
    public PublicProfile PublicProfile { get; set; }
    public List<Session> Sessions { get; set; }
    public DateTime CreatedAt { get; set; }
}
