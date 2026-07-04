using Example.Common.Models;

namespace Example.UserService.API.Models
{
    public class FieldModel : FieldSaveModel
    {
    }

    public class FieldSaveModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
    }

    public class FieldSearchModel : BaseSearch
    {
    }
}
