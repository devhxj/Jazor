namespace Jero.Core;

public class OnlineUserList
{
    public string RealName { get; set; } = string.Empty;
    public bool Online { get; set; }

    public List<SysOnlineUser> UserList { get; set; } = [];
}