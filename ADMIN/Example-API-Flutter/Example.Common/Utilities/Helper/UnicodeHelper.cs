using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Example.Common.Utilities.Helper
{
    public class UnicodeHelper
    {
        /// <summary>
        /// Hàm chuyển một chuỗi tiếng việt có dấu thành tiếng việt không dấu
        /// </summary>
        /// <param name="Unicode">xâu tiếng việt có dấu</param>        
        public static string UnicodeToAscii(string Unicode)
        {
            Unicode = Regex.Replace(Unicode, "[á|à|ả|ã|ạ|â|ă|ấ|ầ|ẩ|ẫ|ậ|ắ|ằ|ẳ|ẵ|ặ]", "a", RegexOptions.IgnoreCase);
            Unicode = Regex.Replace(Unicode, "[é|è|ẻ|ẽ|ẹ|ê|ế|ề|ể|ễ|ệ]", "e", RegexOptions.IgnoreCase);
            Unicode = Regex.Replace(Unicode, "[ú|ù|ủ|ũ|ụ|ư|ứ|ừ|ử|ữ|ự]", "u", RegexOptions.IgnoreCase);
            Unicode = Regex.Replace(Unicode, "[í|ì|ỉ|ĩ|ị]", "i", RegexOptions.IgnoreCase);
            Unicode = Regex.Replace(Unicode, "[ó|ò|ỏ|õ|ọ|ô|ơ|ố|ồ|ổ|ỗ|ộ|ớ|ờ|ở|ỡ|ợ]", "o", RegexOptions.IgnoreCase);
            Unicode = Regex.Replace(Unicode, "[đ|Đ]", "d", RegexOptions.IgnoreCase);
            Unicode = Regex.Replace(Unicode, "[ý|ỳ|ỷ|ỹ|ỵ|Ý|Ỳ|Ỷ|Ỹ|Ỵ]", "y", RegexOptions.IgnoreCase);
            Unicode = Regex.Replace(Unicode, "[^A-Za-z0-9-\\s]", "");
            return Unicode;
        }

        public static string UnicodeToKoDauAndGach(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return string.Empty;
            }

            const string strChar = "abcdefghijklmnopqrstxyzuvxw0123456789 -";
            //string retVal = UnicodeToKoDau(s);
            s = RemoveUnicode(s.ToLower().Trim());
            string sReturn = "";
            for (int i = 0; i < s.Length; i++)
            {
                if (strChar.IndexOf(s[i]) > -1)
                {
                    if (s[i] != ' ')
                        sReturn += s[i];
                    else if (i > 0 && s[i - 1] != ' ' && s[i - 1] != '-')
                        sReturn += "-";
                }
            }
            while (sReturn.IndexOf("--") != -1)
            {
                sReturn = sReturn.Replace("--", "-");
            }
            return sReturn;
        }
        public static string ChuanHoaChuoi(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    return string.Empty;
                }
                input = input.Trim();
                while (input.IndexOf("  ") != -1)
                {
                    input = input.Replace("  ", " ");
                }
            }
            catch (Exception ex)
            {
                //LogMan.Instance.WriteErrorToLog(ex);
            }
            return input;
        }

        private static string charLower = "aAeEoOuUiIdDyY";
        private static string aLower = "áàạảãâấầậẩẫăắằặẳẵ";
        private static string eLower = "éèẹẻẽêếềệểễeeeeee";
        private static string oLower = "óòọỏõôốồộổỗơớờợởỡ";
        private static string uLower = "úùụủũưứừựửữuuuuuu";
        private static string iLower = "íìịỉĩiiiiiiiiiiii";
        private static string dLower = "đdddddddddddddddd";
        private static string yLower = "ýỳỵỷỹyyyyyyyyyyyy";
        private static string aUpper = "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ";
        private static string eUpper = "ÉÈẸẺẼÊẾỀỆỂỄEEEEEE";
        private static string oUpper = "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ";
        private static string uUpper = "ÚÙỤỦŨƯỨỪỰỬỮUUUUUU";
        private static string iUpper = "ÍÌỊỈĨIIIIIIIIIIII";
        private static string dUpper = "ĐDDDDDDDDDDDDDDDD";
        private static string yUpper = "ÝỲỴỶỸYYYYYYYYYYYY";

        public static string RemoveUnicode(string resource)
        {
            string[,] array = new string[14, 18];
            array = initArray();
            string result, temp;
            result = resource;

            for (int i = 1; i < 18; i++)
            {
                array[0, i] = aLower.Substring(i - 1, 1);
                array[1, i] = aUpper.Substring(i - 1, 1);
                array[2, i] = eLower.Substring(i - 1, 1);
                array[3, i] = eUpper.Substring(i - 1, 1);
                array[4, i] = oLower.Substring(i - 1, 1);
                array[5, i] = oUpper.Substring(i - 1, 1);
                array[6, i] = uLower.Substring(i - 1, 1);
                array[7, i] = uUpper.Substring(i - 1, 1);
                array[8, i] = iLower.Substring(i - 1, 1);
                array[9, i] = iUpper.Substring(i - 1, 1);
                array[10, i] = dLower.Substring(i - 1, 1);
                array[11, i] = dUpper.Substring(i - 1, 1);
                array[12, i] = yLower.Substring(i - 1, 1);
                array[13, i] = yUpper.Substring(i - 1, 1);
            }

            for (int j = 0; j < 14; j++)
            {
                for (int k = 0; k < 18; k++)
                {
                    temp = result.Replace(array[j, k], array[j, 0]);
                    result = temp;
                }
            }

            return result;
        }

        private static string[,] initArray()
        {
            string[,] array = new string[14, 18];
            for (int i = 0; i < 14; i++)
            {
                array[i, 0] = charLower.Substring(i, 1);
            }
            return array;
        }
        
        /// <summary>
        /// Nối chuỗi + xóa bỏ tiếng việt => Phục vụ cho gen trường TextSearch
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string RemoveUnicode(params string?[]? values)
        {
            values = values?.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray(); // Loại bỏ các phần tử null hoặc rỗng
            if (values == null || values.Length == 0)
            {
                return string.Empty;
            }
            return RemoveUnicode(string.Join(" | ", values));
        }
    }
}
