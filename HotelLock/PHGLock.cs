using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using HotelLock;

namespace HotelLock
{
    class PHGLock
    {
        //LCRFRW_SDK.dll
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("LCRFRW_SDK.dll", SetLastError = true)]
        private static extern int mif_selecom(int com, int baud);

        [DllImport("LCRFRW_SDK.dll", SetLastError = true)]
        private static extern int tem_readdoorcard_sdk(StringBuilder BH, StringBuilder buff, int p_nLockAP);

        [DllImport("LCRFRW_SDK.dll", SetLastError = true)]
        private static extern int tem_readdatetime13_sdk(StringBuilder p_SDT, StringBuilder p_Unit, StringBuilder p_Times, StringBuilder p_CheckOutHour, StringBuilder p_OpenInner,
            int p_nLockAP);

        [DllImport("LCRFRW_SDK.dll", SetLastError = true)]
        private static extern int tem_writedoorcard13_sdk(StringBuilder BH, StringBuilder fksj, int xh, int sjdw, int sjlength, bool gs, int tfzd, bool fs, int p_nLockAP);

        [DllImport("LCRFRW_SDK.dll", SetLastError = true)]
        private static extern int mif_closecom();

        private static IntPtr phgModule = IntPtr.Zero;

        private const int SysId = 0;

        public static BaseResult __GhiLaiThongTinThe(int com, string ROOMADDRESSnew, string ROOMADDRESSold, DateTime fromDate, DateTime toDate)
        {
            try
            {
                phgModule = LoadLibrary("LCRFRW_SDK.dll");
                if (phgModule == IntPtr.Zero)
                {
                    return new ErrorResult("Không tìm thấy thư viện, LCRFRW_SDK.dll");
                }

                mif_selecom(com, 9600);
                int ret = 0;
                //Nếu là khóa thẻ thì kiểm tra xem có trùng ROOMADDRESS không?
                if (true)
                {
                    //đọc mã phòng từ thẻ
                    StringBuilder bh = new StringBuilder(255);
                    StringBuilder buff = new StringBuilder(255);
                    ret = tem_readdoorcard_sdk(bh, buff, SysId);
                    if (ret != 0) return new ErrorResult(PHGMessage.GetMessage(ret));
                    string strCheck = bh.ToString();
                    if (strCheck.Trim() != ROOMADDRESSold)
                    {
                        return new ErrorResult("Lỗi ghi thẻ: Thẻ trên đầu đọc không đúng số yêu cầu!");
                    }
                }
                StringBuilder RoomCode = new StringBuilder(ROOMADDRESSnew);
                StringBuilder dateStart = new StringBuilder(fromDate.ToString("yyMMddHHmm"));
                int timeLength = (int)((TimeSpan)(toDate - fromDate)).TotalDays;
                int timeType = 1;

                //Tính toán lại cho phù hợp
                if (timeLength == 0)
                {
                    timeType = 0;
                    timeLength = (int)((TimeSpan)(toDate - fromDate)).TotalHours;
                }

                if (timeLength == 0)
                {
                    return new ErrorResult("Thời gian ngắn nhất để ghi thẻ là 60'");
                }

                ret = tem_writedoorcard13_sdk(RoomCode, dateStart, 5, timeType, timeLength, false, SysId, true, 12);

                if (ret != 0)
                {
                    return new ErrorResult(PHGMessage.GetMessage(ret));
                }
                return new OkResult();
            }
            catch (Exception ex)
            {
                return new ErrorResult(ex.Message);
            }
            finally
            {
                try
                {
                    mif_closecom();
                }
                catch
                {
                }
            }
        }

        public static BaseResult __GhiThe(int com, string ROOMADDRESS, DateTime fromDate, DateTime toDate, bool clear = false)
        {
            try
            {
                phgModule = LoadLibrary("LCRFRW_SDK.dll");
                if (phgModule == IntPtr.Zero)
                {
                    return new ErrorResult("Không tìm thấy thư viện, LCRFRW_SDK.dll");
                }

                mif_selecom(com, 9600);
                int ret = 0;
                //Nếu là khóa thẻ thì kiểm tra xem có trùng ROOMADDRESS không?
                if (clear)
                {
                    //đọc mã phòng từ thẻ
                    StringBuilder bh = new StringBuilder(255);
                    StringBuilder buff = new StringBuilder(255);
                    ret = tem_readdoorcard_sdk(bh, buff, SysId);
                    if (ret != 0) return new ErrorResult(PHGMessage.GetMessage(ret));
                    string strCheck = bh.ToString();
                    if (strCheck.Trim() != ROOMADDRESS)
                    {
                        return new ErrorResult("Lỗi ghi thẻ: Thẻ trên đầu đọc không đúng số yêu cầu!");
                    }
                }
                StringBuilder RoomCode = new StringBuilder(ROOMADDRESS);
                StringBuilder dateStart = new StringBuilder(fromDate.ToString("yyMMddHHmm"));
                int timeLength = (int)((TimeSpan)(toDate - fromDate)).TotalDays;
                int timeType = 1;

                //Tính toán lại cho phù hợp
                if (timeLength == 0)
                {
                    timeType = 0;
                    timeLength = (int)((TimeSpan)(toDate - fromDate)).TotalHours;
                }

                if (timeLength == 0)
                {
                    return new ErrorResult("Thời gian ngắn nhất để ghi thẻ là 60'");
                }

                ret = tem_writedoorcard13_sdk(RoomCode, dateStart, 5, timeType, timeLength, false, SysId, true, 12);

                if (ret != 0)
                {
                    return new ErrorResult(PHGMessage.GetMessage(ret));
                }
                return new OkResult();
            }
            catch (Exception ex)
            {
                return new ErrorResult(ex.Message);
            }
            finally
            {
                try
                {
                    mif_closecom();
                }
                catch
                {
                }
            }
        }
    }

    class PHGMessage
    {
        public static string GetMessage(int code)
        {
            string msg = "";
            switch (code)
            {
                case 1: msg = "Communication error"; break;
                case 2: msg = "Time-out error"; break;
                case 3: msg = "List range error"; break;
                case 4: msg = "Data error"; break;
                case 5: msg = "Communication error"; break;
                case 16: msg = "No card"; break;
                case 17: msg = "Power on error"; break;
                case 18: msg = "Password error"; break;
                case 19: msg = "Bad card"; break;
                case 20: msg = "Function error"; break;
                case 128: msg = "Auth error"; break;
                case 129: msg = "Reader type error"; break;
                case 130: msg = "Room Code Error"; break;
                case 131: msg = "System ID error"; break;
                case 132: msg = "Auth card error"; break;
                case 133: msg = "System code error"; break;
                case 134: msg = "Reader auth error"; break;
                case 256: msg = "System error"; break;
            }
            return msg;
        }
    }
}
