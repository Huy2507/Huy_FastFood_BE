using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Huy_FastFood_BE.Services
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return string.Empty;

            // Loại bỏ dấu và chuyển thành chữ thường
            string str = RemoveDiacritics(phrase).ToLowerInvariant();

            // Loại bỏ ký tự không hợp lệ
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // Thay thế nhiều khoảng trắng bằng một dấu cách
            str = Regex.Replace(str, @"\s+", " ").Trim();

            // Cắt chuỗi nếu dài hơn 45 ký tự
            if (str.Length > 45)
                str = str.Substring(0, 45).Trim();

            // Thay thế dấu cách bằng dấu gạch ngang
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Sử dụng Normalization Form D để tách dấu ra khỏi ký tự
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Trả về chuỗi sau khi loại bỏ dấu
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
