using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Huy_FastFood_BE.DTOs;

namespace Huy_FastFood_BE.Services
{
    public class VNPayService
    {
        private readonly VNPayConfig _config;

        public VNPayService(IOptions<VNPayConfig> config)
        {
            _config = config.Value;
        }

        public string CreatePaymentUrl(decimal amount, string orderInfo, string ipAddress)
        {
            // Base parameters for VNPay
            var vnp_Params = new SortedDictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", _config.TmnCode },
                { "vnp_Amount", ((int)(amount * 100)).ToString() }, // Amount in VND x 100
                { "vnp_CurrCode", "VND" },
                { "vnp_TxnRef", DateTime.Now.Ticks.ToString() }, // Unique order ID
                { "vnp_OrderInfo", orderInfo },
                { "vnp_OrderType", "other" }, // Adjust based on business type
                { "vnp_Locale", "vn" },
                { "vnp_ReturnUrl", _config.ReturnUrl },
                { "vnp_IpAddr", ipAddress },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") }
            };

            // Build data string
            var queryString = new StringBuilder();
            foreach (var param in vnp_Params)
            {
                queryString.Append($"{param.Key}={Uri.EscapeDataString(param.Value)}&");
            }

            // Remove last '&' and create signature
            queryString.Length--;
            var rawHash = queryString.ToString();
            var vnpSecureHash = CalculateHash(rawHash, _config.HashSecret);

            // Build final URL
            return $"{_config.Url}?{queryString}&vnp_SecureHash={vnpSecureHash}";
        }

        public bool ValidateReturn(Dictionary<string, string> vnpData)
        {
            // Remove vnp_SecureHash from return data
            var secureHash = vnpData["vnp_SecureHash"];
            vnpData.Remove("vnp_SecureHash");

            // Sort and rebuild query string
            var sortedData = new SortedDictionary<string, string>(vnpData);
            var rawHash = string.Join("&", sortedData.Select(x => $"{x.Key}={x.Value}"));

            // Verify HMAC hash
            var computedHash = CalculateHash(rawHash, _config.HashSecret);
            return secureHash.Equals(computedHash, StringComparison.OrdinalIgnoreCase);
        }

        public static string CalculateHash(string data, string key)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
        }
       
    }
}
