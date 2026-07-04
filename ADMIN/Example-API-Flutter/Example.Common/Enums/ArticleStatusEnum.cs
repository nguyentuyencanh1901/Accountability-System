namespace Example.Common.Enums
{
    public class ArticleStatusEnum
    {
        public enum States
        {
            New = 1 << 0, //1
            Approved = 1 << 1, //2
            NotApproved = 1 << 2, //4
            Deleted = 1 << 3, //8
            Disapprove = 1 << 4, //16
        };
    }

}
