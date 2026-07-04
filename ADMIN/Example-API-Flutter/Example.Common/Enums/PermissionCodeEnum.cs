using System.ComponentModel;
using Example.Common.Utilities;

namespace Example.Common.Enums
{
    /// <summary>Mã quyền hệ thống — đồng bộ tự động vào bảng Permissions khi API khởi động.</summary>
    public enum PermissionCodeEnum
    {
        [Description("Quyền Super Admin — bypass mọi kiểm tra quyền")]
        SUPER_ADMIN,

        [Description("Danh sách người dùng")]
        USER_LIST,
        [Description("Thêm người dùng")]
        USER_CREATE,
        [Description("Sửa người dùng")]
        USER_UPDATE,
        [Description("Xóa người dùng")]
        USER_DELETE,
        [Description("Reset mật khẩu người dùng")]
        USER_RESET_PASSWORD,

        [Description("Danh sách vai trò")]
        ROLE_LIST,
        [Description("Thêm vai trò")]
        ROLE_CREATE,
        [Description("Sửa vai trò")]
        ROLE_UPDATE,
        [Description("Xóa vai trò")]
        ROLE_DELETE,

        [Description("Danh sách quyền hạn")]
        PERMISSION_LIST,

        [Description("Danh sách lĩnh vực")]
        FIELD_LIST,
        [Description("Thêm lĩnh vực")]
        FIELD_CREATE,
        [Description("Sửa lĩnh vực")]
        FIELD_UPDATE,
        [Description("Xóa lĩnh vực")]
        FIELD_DELETE,

        [Description("Danh sách câu hỏi")]
        QUESTION_LIST,
        [Description("Thêm câu hỏi")]
        QUESTION_CREATE,
        [Description("Sửa câu hỏi")]
        QUESTION_UPDATE,
        [Description("Xóa câu hỏi")]
        QUESTION_DELETE,

        [Description("Danh sách đáp án")]
        ANSWEROPTION_LIST,
        [Description("Thêm đáp án")]
        ANSWEROPTION_CREATE,
        [Description("Sửa đáp án")]
        ANSWEROPTION_UPDATE,
        [Description("Xóa đáp án")]
        ANSWEROPTION_DELETE,

        [Description("Danh sách bộ đề")]
        EXAMSET_LIST,
        [Description("Thêm bộ đề")]
        EXAMSET_CREATE,
        [Description("Sửa bộ đề")]
        EXAMSET_UPDATE,
        [Description("Xóa bộ đề")]
        EXAMSET_DELETE,

        [Description("Danh sách kỳ thi")]
        EXAMPERIOD_LIST,
        [Description("Thêm kỳ thi")]
        EXAMPERIOD_CREATE,
        [Description("Sửa kỳ thi")]
        EXAMPERIOD_UPDATE,
        [Description("Xóa kỳ thi")]
        EXAMPERIOD_DELETE,

        [Description("Danh sách phân công thi")]
        EXAMPERIODASSIGNMENT_LIST,
        [Description("Thêm phân công thi")]
        EXAMPERIODASSIGNMENT_CREATE,
        [Description("Sửa phân công thi")]
        EXAMPERIODASSIGNMENT_UPDATE,
        [Description("Xóa phân công thi")]
        EXAMPERIODASSIGNMENT_DELETE,

        [Description("Danh sách bài thi")]
        EXAMSESSION_LIST,
        [Description("Thêm bài thi")]
        EXAMSESSION_CREATE,
        [Description("Sửa bài thi")]
        EXAMSESSION_UPDATE,
        [Description("Xóa bài thi")]
        EXAMSESSION_DELETE,

        [Description("Danh sách câu hỏi bài thi")]
        EXAMSESSIONQUESTION_LIST,
        [Description("Thêm câu hỏi bài thi")]
        EXAMSESSIONQUESTION_CREATE,
        [Description("Sửa câu hỏi bài thi")]
        EXAMSESSIONQUESTION_UPDATE,
        [Description("Xóa câu hỏi bài thi")]
        EXAMSESSIONQUESTION_DELETE,

        [Description("Danh sách câu trả lời bài thi")]
        EXAMSESSIONANSWER_LIST,
        [Description("Thêm câu trả lời bài thi")]
        EXAMSESSIONANSWER_CREATE,
        [Description("Sửa câu trả lời bài thi")]
        EXAMSESSIONANSWER_UPDATE,
        [Description("Xóa câu trả lời bài thi")]
        EXAMSESSIONANSWER_DELETE,

        [Description("Danh sách đáp án đã chọn")]
        EXAMSESSIONANSWEROPTION_LIST,
        [Description("Thêm đáp án đã chọn")]
        EXAMSESSIONANSWEROPTION_CREATE,
        [Description("Sửa đáp án đã chọn")]
        EXAMSESSIONANSWEROPTION_UPDATE,
        [Description("Xóa đáp án đã chọn")]
        EXAMSESSIONANSWEROPTION_DELETE
    }

    public static class PermissionCodeExtensions
    {
        public static string GetCode(this PermissionCodeEnum code) => code.ToString();

        public static string GetDescription(this PermissionCodeEnum code) =>
            StringUtils.GetEnumDescription(code);

        public static IReadOnlyList<PermissionCodeEnum> GetAll() =>
            Enum.GetValues<PermissionCodeEnum>().ToList();

        public static IReadOnlyList<(string Code, string Description)> GetAllWithDescriptions() =>
            GetAll().Select(c => (c.GetCode(), c.GetDescription())).ToList();
    }
}
