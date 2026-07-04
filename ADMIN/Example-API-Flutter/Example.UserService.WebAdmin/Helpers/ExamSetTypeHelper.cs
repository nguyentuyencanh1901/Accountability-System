using Example.Common.Enums;

namespace Example.UserService.WebAdmin.Helpers
{
    public static class ExamSetTypeHelper
    {
        /// <summary>ExamType (session): Trial=1, Real=2 → ExamSet.Type: Real=1, Trial=2.</summary>
        public static int MapExamTypeToExamSetType(int examType) => examType switch
        {
            (int)ExamTypeEnum.Real => (int)ExamSetTypeEnum.Real,
            (int)ExamTypeEnum.Trial => (int)ExamSetTypeEnum.Trial,
            _ => (int)ExamSetTypeEnum.Real
        };

        public static string GetExamSetTypeName(int type) => type switch
        {
            (int)ExamSetTypeEnum.Trial => "Thi thử",
            _ => "Thi thật"
        };
    }
}
