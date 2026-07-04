namespace Example.UserService.API.Models
{
    /// <summary>Model cập nhật nhanh trạng thái (Status) theo Id.</summary>
    public class UpdateStatusModel
    {
        public long Id { get; set; }
        public int Status { get; set; }
    }
}

