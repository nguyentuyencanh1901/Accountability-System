using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class ExamPeriod : EntityAuditBase<long>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public int Status { get; set; }               // 1=Nháp, 2=Đã công bố, 3=Đã đóng
    }
}
