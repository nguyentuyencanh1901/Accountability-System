using System.Net.Mail;
using System.Net;
using System.Reflection;
using Example.Common.Models.AppSetting;
using Example.Common.Const;

namespace Example.Common.Utilities.Helper
{
    public class EmailHelper
    {
        private readonly SendEmailSettingModel _sendEmailSetting = StaticVariable.SendEmailSetting;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body">Nội dung email</param>
        /// <param name="subject">Tiêu đề</param>
        /// <param name="listEmailTo">Danh sách email nhận. Ví dụ: "abc@gmail.com" hoặc "abc@gmail.com, demo@gmail.com"</param>
        /// <param name="listEmailCc">Danh sách email cc. Ví dụ: "abc@gmail.com" hoặc "abc@gmail.com, demo@gmail.com"</param>
        /// <param name="listEmailBcc">Danh sách email bcc. Ví dụ: "abc@gmail.com" hoặc "abc@gmail.com, demo@gmail.com"</param>
        /// <param name="attachments">Tệp đính kèm</param>
        /// <returns></returns>
        public bool SendEmailWithBody(string body, string subject, string listEmailTo, string listEmailCc = "", string listEmailBcc = "", List<Attachment> attachments = null)
        {
            if (!_sendEmailSetting.IsSend)
            {
                return true;
            }
            var mail = new MailMessage()
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                From = new MailAddress(_sendEmailSetting.Email)
            };

            mail.To.Add(listEmailTo);

            if (!string.IsNullOrEmpty(listEmailCc))
            {
                mail.CC.Add(listEmailCc);
            }

            if (!string.IsNullOrEmpty(listEmailBcc))
            {
                mail.Bcc.Add(listEmailBcc);
            }

            if (attachments != null && attachments.Any())
            {
                foreach (var item in attachments)
                {
                    mail.Attachments.Add(item);
                }
            }

            this.SendEmail(mail);
            return true;
        }

        public string GenerateBodyEmailWithTemplate<T>(string pathTemplate, T values)
        {
            string body = this.GetTemplate(pathTemplate);

            return this.GenerateBodyEmailByObj<T>(body, values);
        }

        #region private
        private void SendEmail(MailMessage message)
        {
            SmtpClient smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(_sendEmailSetting.Email, _sendEmailSetting.Password),
                EnableSsl = true
            };
            smtpClient.Send(message);
        }

        private string GetTemplate(string templateFullPath)
        {
            string template;

            if (!File.Exists(templateFullPath))
                throw new ArgumentException("Template file does not exist: " + templateFullPath);

            using (StreamReader reader = new StreamReader(templateFullPath))
            {
                template = reader.ReadToEnd();
                reader.Close();
            }

            return template;
        }

        /// <summary>
        /// Create email body content corresponding to the values ​​of object 
        /// (Body must be formed as a field [$NAME_FIELD$])
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        private string GenerateBodyEmailByObj<T>(string body, T values)
        {
            if (values == null) return body;

            Type myType = values.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
            foreach (PropertyInfo prop in props)
            {
                string propName = prop.Name;
                object propValue = prop.GetValue(values, null) ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(propName))
                    body = body.Replace($"[${propName.ToUpper()}$]", propValue.ToString());
            }

            return body;
        }
        #endregion
    }
}
