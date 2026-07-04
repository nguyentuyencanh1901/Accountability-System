using Example.Common.Enums;
using System.Collections.Generic;

namespace Example.Common.Utilities.Helper
{
    public static class ArticlesStatusHelper
    {
        #region TurnOn TurnOff Status

        public static int TurnOn(this int s, ArticleStatusEnum.States b) { return s | (int)b; }
        public static int TurnOff(this int s, ArticleStatusEnum.States b) { return s & ~(int)b; }

        public static bool HasState(this int s, ArticleStatusEnum.States b)
        {
            return (s & (int)b) == (int)b;
        }
        public static bool HasState(this int s, int b)
        {
            return (s & b) == b;
        }

        #endregion

        public static Dictionary<int, string> ArticlesStatusName = new Dictionary<int, string>()
        {
             {(int)ArticleStatusEnum.States.New, "Not approved"},
             {(int)ArticleStatusEnum.States.Approved, "Approved"},
             {(int)ArticleStatusEnum.States.NotApproved, "Disapproved"},
             {(int)ArticleStatusEnum.States.Disapprove, "Unpublish"},
             {(int)ArticleStatusEnum.States.Deleted, "Deleted"},
        };

        public static List<int> GetStatus(int status)
        {
            var result = new List<int>();

            if (HasState(status, ArticleStatusEnum.States.New))
            {
                result.Add((int)ArticleStatusEnum.States.New);
            }

            if (HasState(status, ArticleStatusEnum.States.Approved))
            {
                result.Add((int)ArticleStatusEnum.States.Approved);
            }

            if (HasState(status, ArticleStatusEnum.States.NotApproved))
            {
                result.Add((int)ArticleStatusEnum.States.NotApproved);
            }

            if (HasState(status, ArticleStatusEnum.States.Deleted))
            {
                result.Add((int)ArticleStatusEnum.States.Deleted);
            }

            if (HasState(status, ArticleStatusEnum.States.Disapprove))
            {
                result.Add((int)ArticleStatusEnum.States.Disapprove);
            }

            return result;
        }
    }
}
