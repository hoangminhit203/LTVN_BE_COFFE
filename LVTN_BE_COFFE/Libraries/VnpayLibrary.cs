using LVTN_BE_COFFE.Domain.Model;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace LVTN_BE_COFFE.Libraries
{
    public class VnpayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        // Bước 1: Nhận dữ liệu trả về từ VNPAY và kiểm tra chữ ký
        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            // Tạo mới đối tượng VnpayLibrary để lưu dữ liệu trả về
            var vnPay = new VnpayLibrary();
            // Lặp qua tất cả các tham số trả về, chỉ lấy các tham số bắt đầu bằng 'vnp_'
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value);
                }
            }
            // Lấy mã đơn hàng từ tham số trả về
            var orderIdStr = vnPay.GetResponseData("vnp_TxnRef");
            // var orderId = long.TryParse(orderIdStr, out var oid) ? oid : 0;
            var orderId = orderIdStr;
            // Lấy mã giao dịch VNPAY
            var vnPayTranIdStr = vnPay.GetResponseData("vnp_TransactionNo");
            var vnPayTranId = long.TryParse(vnPayTranIdStr, out var tid) ? tid : 0;
            // Lấy mã phản hồi từ VNPAY
            var vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
            // Lấy mã hash để kiểm tra chữ ký
            var vnpSecureHash = collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
            // Lấy thông tin đơn hàng
            var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");
            // Kiểm tra chữ ký hợp lệ hay không
            var checkSignature = vnPay.ValidateSignature(vnpSecureHash, hashSecret);
            if (!checkSignature)
                return new PaymentResponseModel()
                {
                    Success = false
                };
            // Trả về kết quả thanh toán
            return new PaymentResponseModel()
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = orderInfo,
                OrderId = orderId.ToString(),
                PaymentId = vnPayTranId.ToString(),
                TransactionId = vnPayTranId.ToString(),
                Token = vnpSecureHash,
                VnPayResponseCode = vnpResponseCode
            };
        }

        // Bước 2: Lấy địa chỉ IP của client thực hiện thanh toán
        public string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;

                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    }

                    if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();

                    return ipAddress;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "127.0.0.1";
        }

        // Bước 3: Thêm dữ liệu vào request gửi lên VNPAY
        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        // Bước 4: Thêm dữ liệu vào response nhận từ VNPAY
        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        // Bước 5: Lấy dữ liệu từ response
        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }

        // Bước 6: Tạo URL thanh toán gửi lên VNPAY
        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            // Ghép các tham số thành query string
            foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            var querystring = data.ToString();

            baseUrl += "?" + querystring;
            var signData = querystring;
            if (signData.Length > 0)
            {
                signData = signData.Remove(data.Length - 1, 1);
            }

            // Tạo mã hash để xác thực giao dịch
            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnpSecureHash;

            return baseUrl;
        }

        // Bước 7: Kiểm tra chữ ký giao dịch trả về từ VNPAY
        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();
            var myChecksum = HmacSha512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        // Bước 8: Tạo mã hash HMAC SHA512
        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new System.Security.Cryptography.HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        // Bước 9: Ghép dữ liệu response thành chuỗi để xác thực chữ ký
        private string GetResponseData()
        {
            var data = new StringBuilder();
            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }

            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }

            foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            // Xóa ký tự '&' cuối cùng
            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            return data.ToString();
        }

        // Bước 10: So sánh key cho SortedList
        public class VnPayCompare : IComparer<string>
        {
            public int Compare(string? x, string? y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                var vnpCompare = CompareInfo.GetCompareInfo("en-US");
                return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
            }
        }
    }

}