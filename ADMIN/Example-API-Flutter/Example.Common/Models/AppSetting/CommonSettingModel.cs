using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Models.AppSetting
{
    public class CommonSettingModel
    {
        public string UserImagePath { get; set; }
        /// <summary>Thư mục gốc lưu ảnh entity. Để trống → wwwroot/uploads.</summary>
        public string MediaUploadPath { get; set; }
    }
}
