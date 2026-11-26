using System.Net;
using System.Security.Cryptography;
using System.Text;
using MedicalAppointment.API.Data;
using MedicalAppointment.API.DTOs.Payment;
using MedicalAppointment.API.Entities;
using MedicalAppointment.API.Exceptions;
using MedicalAppointment.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedicalAppointment.API.Services;

public class PaymentService(
    ApplicationDbContext dbContext,
    IConfiguration configuration,
    IHostEnvironment hostEnvironment) : IPaymentService
{
    private string VnpTmnCode =>
        string.IsNullOrWhiteSpace(configuration["PaymentSettings:VnpTmnCode"])
            ? "YTEHP001"
            : configuration["PaymentSettings:VnpTmnCode"]!;

    private string VnpHashSecret
    {
        get
        {
            var configured = configuration["PaymentSettings:VnpHashSecret"];
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured;
            }
            if (hostEnvironment.IsDevelopment())
            {
                return "VNPAY_SANDBOX_SECRET_KEY_YTE_HIEP_PHUOC_2026";
            }
            throw new InvalidOperationException("Chưa cấu hình PaymentSettings:VnpHashSecret.");
        }
    }

    private string VnpBaseUrl =>
        string.IsNullOrWhiteSpace(configuration["PaymentSettings:VnpBaseUrl"])
            ? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
            : configuration["PaymentSettings:VnpBaseUrl"]!;

    private string BankId =>
        string.IsNullOrWhiteSpace(configuration["PaymentSettings:VietQrBankId"])
            ? "MB"
            : configuration["PaymentSettings:VietQrBankId"]!;

    private string BankName =>
        string.IsNullOrWhiteSpace(configuration["PaymentSettings:VietQrBankName"])
            ? "Ngân hàng TMCP Quân Đội (MB Bank)"
            : configuration["PaymentSettings:VietQrBankName"]!;

    private string AccountNo =>
        string.IsNullOrWhiteSpace(configuration["PaymentSettings:VietQrAccountNo"])
            ? "0987654321"
            : configuration["PaymentSettings:VietQrAccountNo"]!;

    private string AccountName =>
        string.IsNullOrWhiteSpace(configuration["PaymentSettings:VietQrAccountName"])
            ? "TRUNG TAM Y TE HIEP PHUOC"
            : configuration["PaymentSettings:VietQrAccountName"]!;

    private async Task<Order> GetPayableOrderAsync(int userId, bool isAdmin, int orderId)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(
            o => o.Id == orderId && (isAdmin || o.UserId == userId));

        if (order == null)
        {
            throw new NotFoundException(ErrorCode.ORDER_NOT_FOUND);
        }

        if (order.Status == "Cancelled")
        {
            throw new BadRequestException(ErrorCode.ORDER_CANCELLED_CANNOT_PAY);
        }

        if (string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(ErrorCode.ORDER_ALREADY_PAID);
        }

        return order;
    }

    public async Task<VnPayPaymentResponse> CreateVnPayUrlAsync(
        int userId,
        bool isAdmin,
        int orderId,
        string clientIp,
        string baseCallbackUrl)
    {
        var order = await GetPayableOrderAsync(userId, isAdmin, orderId);

        var nowVn = DateTime.UtcNow.AddHours(7);
        var txnRef = $"VNP{order.Id}_{nowVn:yyyyMMddHHmmss}";
        var amountMinor = ((long)Math.Round(order.TotalAmount * 100m)).ToString();

        order.PaymentMethod = "VNPay";
        order.TransactionId = txnRef;
        await dbContext.SaveChangesAsync();

        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Amount"] = amountMinor,
            ["vnp_Command"] = "pay",
            ["vnp_CreateDate"] = nowVn.ToString("yyyyMMddHHmmss"),
            ["vnp_CurrCode"] = "VND",
            ["vnp_IpAddr"] = string.IsNullOrWhiteSpace(clientIp) ? "127.0.0.1" : clientIp,
            ["vnp_Locale"] = "vn",
            ["vnp_OrderInfo"] = $"Thanh toan don dat lich kham #{order.Id}",
            ["vnp_OrderType"] = "health",
            ["vnp_ReturnUrl"] = baseCallbackUrl,
            ["vnp_TmnCode"] = VnpTmnCode,
            ["vnp_TxnRef"] = txnRef,
            ["vnp_Version"] = "2.1.0"
        };

        var queryString = BuildSortedQueryString(vnpParams);
        var secureHash = ComputeHmacSha512(VnpHashSecret, queryString);
        var paymentUrl = $"{VnpBaseUrl}?{queryString}&vnp_SecureHash={secureHash}";

        var isDev = hostEnvironment.IsDevelopment();
        string callbackSuccessUrl = string.Empty;
        string callbackCancelUrl = string.Empty;

        if (isDev)
        {
            var successCallbackParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Amount"] = amountMinor,
                ["vnp_BankCode"] = "NCB",
                ["vnp_OrderInfo"] = $"Thanh toan don dat lich kham #{order.Id}",
                ["vnp_PayDate"] = nowVn.ToString("yyyyMMddHHmmss"),
                ["vnp_ResponseCode"] = "00",
                ["vnp_TmnCode"] = VnpTmnCode,
                ["vnp_TransactionNo"] = $"14{nowVn:ddHHmmss}",
                ["vnp_TransactionStatus"] = "00",
                ["vnp_TxnRef"] = txnRef
            };
            var successQuery = BuildSortedQueryString(successCallbackParams);
            var successHash = ComputeHmacSha512(VnpHashSecret, successQuery);

            var cancelCallbackParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Amount"] = amountMinor,
                ["vnp_OrderInfo"] = $"Thanh toan don dat lich kham #{order.Id}",
                ["vnp_ResponseCode"] = "24",
                ["vnp_TmnCode"] = VnpTmnCode,
                ["vnp_TransactionStatus"] = "02",
                ["vnp_TxnRef"] = txnRef
            };
            var cancelQuery = BuildSortedQueryString(cancelCallbackParams);
            var cancelHash = ComputeHmacSha512(VnpHashSecret, cancelQuery);

            callbackSuccessUrl = $"{baseCallbackUrl}?{successQuery}&vnp_SecureHash={successHash}";
            callbackCancelUrl = $"{baseCallbackUrl}?{cancelQuery}&vnp_SecureHash={cancelHash}";
        }

        return new VnPayPaymentResponse
        {
            OrderId = order.Id,
            Amount = order.TotalAmount,
            TxnRef = txnRef,
            PaymentUrl = paymentUrl,
            CallbackSuccessUrl = callbackSuccessUrl,
            CallbackCancelUrl = callbackCancelUrl
        };
    }

    public async Task<VietQrPaymentResponse> CreateVietQrAsync(int userId, bool isAdmin, int orderId)
    {
        var order = await GetPayableOrderAsync(userId, isAdmin, orderId);

        var nowVn = DateTime.UtcNow.AddHours(7);
        var txnRef = $"VQR{order.Id}_{nowVn:yyyyMMddHHmmss}";
        var transferContent = $"YTEHP DH{order.Id}";
        var roundedAmount = (long)Math.Round(order.TotalAmount);

        order.PaymentMethod = "VietQR";
        order.TransactionId = txnRef;
        await dbContext.SaveChangesAsync();

        var vietQrImageUrl =
            $"https://img.vietqr.io/image/{Uri.EscapeDataString(BankId)}-{Uri.EscapeDataString(AccountNo)}-compact2.png" +
            $"?amount={roundedAmount}" +
            $"&addInfo={Uri.EscapeDataString(transferContent)}" +
            $"&accountName={Uri.EscapeDataString(AccountName)}";

        var qrPayload =
            $"VIETQR|BANK={BankId}|ACC={AccountNo}|NAME={AccountName}|AMOUNT={roundedAmount}|CONTENT={transferContent}|REF={txnRef}";

        return new VietQrPaymentResponse
        {
            OrderId = order.Id,
            Amount = order.TotalAmount,
            TxnRef = txnRef,
            BankId = BankId,
            BankName = BankName,
            AccountNo = AccountNo,
            AccountName = AccountName,
            TransferContent = transferContent,
            QrData = qrPayload,
            VietQrImageUrl = vietQrImageUrl
        };
    }

    public async Task<PaymentCallbackResponse> HandleVnPayCallbackAsync(IQueryCollection query)
    {
        var receivedHash = query["vnp_SecureHash"].ToString().Trim();
        var txnRef = query["vnp_TxnRef"].ToString().Trim();
        var responseCode = query["vnp_ResponseCode"].ToString().Trim();
        var transactionNo = query["vnp_TransactionNo"].ToString().Trim();

        if (string.IsNullOrEmpty(receivedHash) || string.IsNullOrEmpty(txnRef))
        {
            throw new BadRequestException(ErrorCode.INVALID_PAYMENT_SIGNATURE);
        }

        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var kvp in query)
        {
            if (kvp.Key.StartsWith("vnp_", StringComparison.Ordinal) &&
                !string.Equals(kvp.Key, "vnp_SecureHash", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(kvp.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase))
            {
                var val = kvp.Value.ToString();
                if (!string.IsNullOrEmpty(val))
                {
                    vnpParams[kvp.Key] = val;
                }
            }
        }

        var rawData = BuildSortedQueryString(vnpParams);
        var expectedHash = ComputeHmacSha512(VnpHashSecret, rawData);

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedHash.ToLowerInvariant()),
                Encoding.UTF8.GetBytes(receivedHash.ToLowerInvariant())))
        {
            throw new BadRequestException(ErrorCode.INVALID_PAYMENT_SIGNATURE);
        }

        var orderId = ParseOrderIdFromTxnRef(txnRef);
        var order = await dbContext.Orders.FirstOrDefaultAsync(
            o => (orderId.HasValue && o.Id == orderId.Value) || o.TransactionId == txnRef);

        if (order == null)
        {
            throw new NotFoundException(ErrorCode.ORDER_NOT_FOUND);
        }

        if (order.Status == "Cancelled")
        {
            throw new BadRequestException(ErrorCode.ORDER_CANCELLED_CANNOT_PAY);
        }

        var expectedAmountMinor = (long)Math.Round(order.TotalAmount * 100m, MidpointRounding.AwayFromZero);
        var amountRaw = query["vnp_Amount"].ToString().Trim();
        if (!long.TryParse(amountRaw, out var receivedAmountMinor) || receivedAmountMinor != expectedAmountMinor)
        {
            throw new BadRequestException(ErrorCode.PAYMENT_AMOUNT_MISMATCH);
        }

        if (responseCode == "00")
        {
            if (!string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                order.PaymentMethod = "VNPay";
                order.PaymentStatus = "Paid";
                order.TransactionId = string.IsNullOrEmpty(transactionNo) ? txnRef : transactionNo;
                await dbContext.SaveChangesAsync();
            }

            return new PaymentCallbackResponse
            {
                Success = true,
                OrderId = order.Id,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                TransactionId = order.TransactionId,
                ResponseCode = "00",
                Message = "Thanh toán qua VNPay thành công!"
            };
        }

        return new PaymentCallbackResponse
        {
            Success = false,
            OrderId = order.Id,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            TransactionId = order.TransactionId,
            ResponseCode = string.IsNullOrEmpty(responseCode) ? "99" : responseCode,
            Message = "Giao dịch thanh toán VNPay không thành công hoặc đã bị hủy."
        };
    }

    public async Task<PaymentCallbackResponse> ConfirmPaymentAsync(
        int userId,
        bool isAdmin,
        ConfirmPaymentRequest request)
    {
        var order = await GetPayableOrderAsync(userId, isAdmin, request.OrderId);

        if (request.SimulateFailure)
        {
            return new PaymentCallbackResponse
            {
                Success = false,
                OrderId = order.Id,
                PaymentMethod = request.Provider,
                PaymentStatus = order.PaymentStatus,
                TransactionId = order.TransactionId,
                ResponseCode = "24",
                Message = $"Giao dịch {request.Provider} đã bị hủy hoặc chưa nhận được chuyển khoản."
            };
        }

        var nowVn = DateTime.UtcNow.AddHours(7);
        var prefix = string.Equals(request.Provider, "VNPay", StringComparison.OrdinalIgnoreCase)
            ? "VNP"
            : "VQR";

        order.PaymentMethod = string.Equals(request.Provider, "VNPay", StringComparison.OrdinalIgnoreCase)
            ? "VNPay"
            : "VietQR";
        order.PaymentStatus = "Paid";
        order.TransactionId = $"{prefix}{order.Id}_{nowVn:yyyyMMddHHmmss}";
        await dbContext.SaveChangesAsync();

        return new PaymentCallbackResponse
        {
            Success = true,
            OrderId = order.Id,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            TransactionId = order.TransactionId,
            ResponseCode = "00",
            Message = $"Thanh toán {order.PaymentMethod} thành công!"
        };
    }

    private static int? ParseOrderIdFromTxnRef(string txnRef)
    {
        // Định dạng: VNP{orderId}_{timestamp} hoặc VQR{orderId}_{timestamp}
        if (txnRef.Length <= 3)
        {
            return null;
        }

        var withoutPrefix = txnRef[3..];
        var underscoreIdx = withoutPrefix.IndexOf('_');
        var idPart = underscoreIdx > 0 ? withoutPrefix[..underscoreIdx] : withoutPrefix;
        return int.TryParse(idPart, out var parsed) ? parsed : null;
    }

    private static string BuildSortedQueryString(SortedDictionary<string, string> parameters)
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in parameters)
        {
            if (sb.Length > 0)
            {
                sb.Append('&');
            }
            sb.Append(WebUtility.UrlEncode(key));
            sb.Append('=');
            sb.Append(WebUtility.UrlEncode(value));
        }
        return sb.ToString();
    }

    private static string ComputeHmacSha512(string key, string input)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = HMACSHA512.HashData(keyBytes, inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
