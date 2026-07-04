using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace Example.Common.Utilities
{
    public static class StringUtils
    {
        public static string NewLineToBr(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            else
            {
                var builder = new StringBuilder();
                var lines = text.Split('\n');
                for (var i = 0; i < lines.Length; i++)
                {
                    if (i > 0)
                    {
                        builder.Append("<br/>\n");
                    }

                    builder.Append(lines[i]);
                }
                return builder.ToString();
            }
        }

        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            var md5 = MD5.Create();
            var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string GetHash(string text)
        {
            // SHA512 is disposable by inheritance.  
            using (var sha256 = SHA256.Create())
            {
                // Send a sample text to hash.  
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                // Get the hashed string.  
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public static string QuoteString(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return "";
            }

            var str = inputString.Trim();
            if (str != "")
            {
                str = str.Replace("'", "''");
            }
            return str;
        }

        public static string AddSlash(string input)
        {
            var str = !string.IsNullOrEmpty(input) ? input.Trim() : "";
            if (str != "")
            {
                str = str.Replace("'", "'").Replace("\"", "\\\"");
            }
            return str;
        }

        public static string RefreshText(string text)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text.Trim()))
            {
                return text;
            }

            text = HttpUtility.HtmlDecode(text);

            text = HttpUtility.UrlDecode(text);

            return text;
        }

        public static string RemoveStrHtmlTags(object inputObject)
        {
            if (inputObject == null)
            {
                return string.Empty;
            }
            var input = (Convert.ToString(inputObject) + "").Trim();
            if (input != "")
            {
                input = Regex.Replace(input, @"<(.|\n)*?>", string.Empty);
            }
            return input;
        }

        public static string ReplaceSpaceToPlus(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return Regex.Replace(input, @"\s+", "+", RegexOptions.IgnoreCase);
            }
            return input;
        }

        public static string ReplaceSpecialCharater(object inputObject)
        {
            if (inputObject == null)
            {
                return string.Empty;
            }
            return Convert.ToString(inputObject)
                .Trim()
                .Trim()
                .Replace(@"\", @"\\")
                .Replace("\"", "&quot;")
                .Replace("“", "&ldquo;")
                .Replace("”", "&rdquo;")
                .Replace("‘", "&lsquo;")
                .Replace("’", "&rsquo;")
                .Replace("'", "&#39;");
        }

        public static string JavaScriptSring(string input)
        {
            input = input.Replace("'", @"\u0027");
            input = input.Replace("\"", @"\u0022");
            return input;
        }

        public static int CountWords(string stringInput)
        {
            if (string.IsNullOrEmpty(stringInput))
            {
                return 0;
            }
            stringInput = RemoveStrHtmlTags(stringInput);
            return Regex.Matches(stringInput, @"[\S]+").Count;
        }

        public static string GetEnumDescription(Enum value)
        {
            try
            {
                var fi = value.GetType().GetField(value.ToString());
                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return attributes.Length > 0 ? attributes[0].Description : value.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static DisplayAttribute[] GetEnumDisplay(Enum value)
        {
            try
            {
                var fi = value.GetType().GetField(value.ToString());
                var attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);
                return attributes;
            }
            catch (Exception ex)
            {
                return new DisplayAttribute[0];
            }
        }

        public static List<(int number, string name, string description)> GetEnumItems(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Tham số phải là một loại Enum");

            var items = new List<(int number, string name, string description)>();

            foreach (var value in Enum.GetValues(enumType))
            {
                var number = (int)value;
                var name = value.ToString();

                // Lấy mô tả từ thuộc tính Description
                var fieldInfo = enumType.GetField(name);
                var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>();
                var description = displayAttribute != null ? displayAttribute.Description : name;

                items.Add((number, name, description));
            }

            return items;
        }

        public static string GetEnumDisplayShortName(Enum value)
        {
            try
            {
                DisplayAttribute[] display = StringUtils.GetEnumDisplay(value);
                string? shortName = display?.FirstOrDefault()?.ShortName;
                return shortName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetPropertyDisplayName<T>(Expression<Func<T, object>> propertyExpression)
        {
            var memberInfo = GetPropertyInformation(propertyExpression.Body);
            if (memberInfo == null)
            {
                throw new ArgumentException(
                    "No property reference expression was found.",
                    "propertyExpression");
            }

            var attr = memberInfo.GetAttribute<DisplayNameAttribute>(false);
            if (attr == null)
            {
                return memberInfo.Name;
            }

            return attr.DisplayName;
        }

        public static MemberInfo GetPropertyInformation(Expression propertyExpression)
        {
            //Debug.Assert(propertyExpression != null, "propertyExpression != null");
            var memberExpr = propertyExpression as MemberExpression;
            if (memberExpr == null && propertyExpression is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert)
            {
                memberExpr = unaryExpr.Operand as MemberExpression;
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
            {
                return memberExpr.Member;
            }

            return null;
        }

        public static string SubWordInString(object obj, int maxWord, bool removeHTML = false)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            if (removeHTML) obj = RemoveStrHtmlTags(obj);

            var input = Regex.Replace(Convert.ToString(obj), @"\s+", " ");

            var strArray = Regex.Split(input, " ");
            if (strArray.Length <= maxWord)
            {
                return input;
            }
            input = string.Empty;
            for (var i = 0; i < maxWord; i++)
            {
                input = input + strArray[i] + " ";
            }
            return string.Concat(input.Trim(), "...");
        }

        public static string SubWordInDotString(object obj, int maxWord, string extensionEnd = " ...")
        {
            if (obj == null)
            {
                return string.Empty;
            }
            var input = Regex.Replace(Convert.ToString(obj), @"\s+", " ");
            var strArray = Regex.Split(input, " ");
            if (strArray.Length <= maxWord)
            {
                return input;
            }
            input = string.Empty;
            for (var i = 0; i < maxWord; i++)
            {
                input = input + strArray[i] + " ";
            }
            return (input.Trim() + extensionEnd);
        }

        public static string StripHtml(string html)
        {
            return (string.IsNullOrEmpty(html) ? string.Empty : Regex.Replace(html, "<.*?>", string.Empty));
        }

        public static string TrimText(object strIn, int intLength)
        {
            try
            {
                var str = StripHtml(Convert.ToString(strIn));
                if (str.Length > intLength)
                {
                    str = str.Substring(0, intLength - 4);
                    return (str.Substring(0, str.LastIndexOfAny(new char[] { ' ', '.', '?', ',', '!' })) + " ...");
                }
                return str;
            }
            catch (Exception)
            {
                return Convert.ToString(strIn);
            }
        }

        public static string FormatNumber(string sNumber, string sperator = ".")
        {
            var num = 3;
            var num2 = 0;
            for (var i = 1; i <= (sNumber.Length / 3); i++)
            {
                if ((num + num2) < sNumber.Length)
                {
                    sNumber = sNumber.Insert((sNumber.Length - num) - num2, sperator);
                }
                num += 3;
                num2++;
            }
            return sNumber;
        }

        public static string FormatNumberWithComma(string sNumber)
        {
            var num = 3;
            var num2 = 0;
            for (var i = 1; i <= (sNumber.Length / 3); i++)
            {
                if ((num + num2) < sNumber.Length)
                {
                    sNumber = sNumber.Insert((sNumber.Length - num) - num2, ",");
                }
                num += 3;
                num2++;
            }
            return sNumber;
        }

        public static bool IsValidWord(string input, char character)
        {
            if (string.IsNullOrEmpty(input))
            {
                return true;
            }
            var arr = input.Split(character);
            for (var i = 0; i < arr.Length; i++)
            {
                if (arr[i].Length > 30)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                var m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static string GetMetaDescription(string format, params object[] args)
        {
            if (String.IsNullOrEmpty(format)) return String.Empty;

            var strDes = format;

            if (args != null && args.Length > 0)
            {
                strDes = String.Format(strDes, args);
            }

            return strDes;
        }

        public static string ConvertNumberToCurrency(double number, string sperator = ".", string currentcy = "")
        {
            if (number <= 0)
            {
                return "0";
            }

            number = Math.Round(number, 0);

            var output = StringUtils.FormatNumber(number.ToString(CultureInfo.CurrentCulture), sperator) + currentcy;

            return output;
        }

        public static string ReplaceCaseInsensitive(string input, string[] search, string[] replacement)
        {
            int lenSearch = search.Length, lenRepalace = replacement.Length;
            var result = string.Empty;
            for (var i = 0; i < lenSearch; i++)
            {
                for (var j = 0; j < lenRepalace; j++)
                {
                    result = Regex.Replace(
                        input,
                        Regex.Escape(search[i]),
                        replacement[j].Replace("$", "$$"),
                        RegexOptions.IgnoreCase
                    ).Trim();
                    input = result;
                }
            }

            return result;
        }

        public static string GetStringTreeview(int level)
        {
            if (level == 0)
            {
                return string.Empty;
            }

            var strLevel = "";
            for (var i = 0; i < level; i++)
            {
                strLevel = strLevel + "__ ";
            }
            return strLevel;
        }

        public static List<long> ConvertStringToListLong(string ids)
        {
            var lstId = new List<long>();
            if (string.IsNullOrEmpty(ids))
            {
                return lstId;
            }

            if (ids.Contains(","))
            {
                lstId = ids.Split(',').Select(i => i.ToLong()).ToList();
            }
            else
            {
                lstId.Add(ids.ToLong());
            }
            return lstId;
        }

        #region Content Process

        public static string UploadImageIncontent(string content, out string firstImage, Func<string, string> uploadImage)
        {
            firstImage = string.Empty;

            var newContent = content;

            if (string.IsNullOrEmpty(newContent))
            {
                return newContent;
            }

            try
            {
                const string strRegex = @"<img.+?src=[\""'](?<SRC>.+?)[\""'].*?>";
                var myRegex = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                var match = myRegex.Match(newContent);
                if (match.Success)
                {
                    newContent = myRegex.Replace(newContent, m => string.Format("<p style=\"text-align:center\"><img src=\"{0}\" /></p>", uploadImage.Invoke(m.Groups["SRC"].Value)));
                }

                foreach (Match matchAvatar in myRegex.Matches(newContent))
                {
                    if (matchAvatar.Success)
                    {
                        firstImage = matchAvatar.Groups["SRC"].Value;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

                // Todo
            }
            return newContent;
        }

        #endregion Content Process

        public static string AddAttributeForAnchors(string htmlContent, string domainTarget = "http://banxehoi.com/diendan/seolink/?refer=", bool isEncrypt = false)
        {
            if (string.IsNullOrEmpty(htmlContent))
            {
                return htmlContent;
            }

            try
            {
                htmlContent = Regex.Replace(htmlContent, @"rel=[""']nofollow[""']", string.Empty);
                htmlContent = htmlContent.Replace(@"target=[""']_blank[""']", string.Empty);

                const string strRegex = @"(?<LINK><a[^>]href=[""'](?<url>[^""']+)[""'](?<attrs>[^>]*)>(?<Content>((?!<\/a>).)*)<\/a>)";
                var myRegex = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var match = myRegex.Match(htmlContent);
                //string strReplace = @"<a href=""" + domainTarget + @"${url}"" ${attrs} rel=""nofollow"" target=""_blank"">${Content}</a>";

                if (match.Success)
                {
                    htmlContent = myRegex.Replace(htmlContent, delegate (Match m)
                    {
                        var url = domainTarget + m.Groups["url"].Value;
                        var attrs = m.Groups["attrs"].Value;
                        var content = m.Groups["Content"].Value;
                        var link = string.Format(@"<a href=""{0}"" {1} rel=""nofollow"" target=""_blank"">{2}</a>", url, attrs,
                            content);
                        link = Regex.Replace(link, @"\s+", " ");
                        return link;
                    });
                    //htmlContent = myRegex.Replace(htmlContent, strReplace);
                }
            }
            catch
            {
                // Todo something
            }
            return htmlContent;
        }

        public static string TrimTextByLimitCharacters(string strIn, int intLength)
        {
            return strIn.Length > intLength ? strIn.Substring(0, intLength) : strIn;
        }

        public static void AddPaging(StringBuilder query, int pageIndex, int pageSize)
        {
            if (pageSize > 0)
            {
                query.AppendFormat($" LIMIT {pageSize} ");
                if (pageIndex > 1)
                {
                    int nextOffset = (pageIndex - 1) * pageSize;
                    query.AppendFormat($" OFFSET {nextOffset} ");
                }
            }
        }

        public static bool IsJson(this string source)
        {
            if (source == null)
                return false;
            try
            {
                JsonDocument.Parse(source);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        public static List<int> RangeIncrement(int start, int end, int increment)
        {
            return Enumerable
                .Repeat(start, ((end - start) / increment) + 1)
                .Select((tr, ti) => tr + (increment * ti))
                .ToList();
        }
    }

    public static class ValidateSQL
    {
        public static string Sanitize(this string stringValue)
        {
            if (null == stringValue)
                return null;

            const string regExp = @"[^\w\d\s{1}\-]";

            stringValue = Regex.Replace(stringValue, regExp, "");
            stringValue = Regex.Replace(stringValue, @"\s+", " ").Replace(" ", "-").Trim('-');
            return stringValue
                        .RegexReplace("-{2,}", "-")                 // transforms multiple --- in - use to comment in sql scripts
                        .RegexReplace(@"[*/]+", string.Empty)      // removes / and * used also to comment in sql scripts
                        .RegexReplace(@"(;|\s)(exec|execute|select|insert|update|delete|create|alter|drop|rename|truncate|backup|restore)\s", string.Empty, RegexOptions.IgnoreCase);
        }

        private static string RegexReplace(this string stringValue, string matchPattern, string toReplaceWith)
        {
            return Regex.Replace(stringValue, matchPattern, toReplaceWith);
        }

        private static string RegexReplace(this string stringValue, string matchPattern, string toReplaceWith, RegexOptions regexOptions)
        {
            return Regex.Replace(stringValue, matchPattern, toReplaceWith, regexOptions);
        }
    }
}
