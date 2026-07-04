namespace Example.UserService.WebAdmin.Models.Shared
{
    public class ExamSetPickOptionViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
        public bool Selected { get; set; }
    }
}
