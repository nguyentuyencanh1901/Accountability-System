using Example.Common.Base;

namespace Example.UserService.API.Entities
{
    public class ExamSetField : EntityAuditBase<long>
    {
        public long ExamSetId { get; set; }
        public long FieldId { get; set; }
    }
}
