namespace BE.Dtos.Admin;

public class AdminRoomUpdateDto
{
    public string RoomName { get; set; } = "";
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
}
