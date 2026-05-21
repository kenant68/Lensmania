namespace LensmaniaLibrary.DTOs.Users;

public class PaginatedUsers
{
    public List<UserAdminResponse> Users { get; set; } = [];
    public int Total { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
}
