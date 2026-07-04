using System.ComponentModel.DataAnnotations;

namespace Example.Common.Enums
{
    public enum DataType
    {
        [Display(Name = "String", ShortName = "string")]
        String = 1,
        [Display(Name = "Json", ShortName = "json")]
        Json = 2
    }
}
