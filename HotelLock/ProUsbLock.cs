using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using HotelLock;

namespace HotelLock
{
    public class ProUsbLock
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);
        //Lấy version dll
        [DllImport("proRFL.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GetDLLVersion([MarshalAs(UnmanagedType.LPStr)] StringBuilder sDllVer);

        //Kết nối thiết bị proUSB
        [DllImport("proRFL.dll", CallingConvention = CallingConvention.StdCall)]
        private extern static int initializeUSB(byte fUSB);

        //Đọc thông tin thẻ
        [DllImport("proRFL.dll", CallingConvention = CallingConvention.StdCall)]
        private extern static int ReadCard(byte fUSB, [MarshalAs(UnmanagedType.LPStr)] StringBuilder Buffer);

        //Tạo thẻ
        [DllImport("proRFL.dll", CallingConvention = CallingConvention.StdCall)]
        private extern static int GuestCard(byte fUSB, int dlsCoID, int CardNo, int dai, int llock, int pdoors, string BDate, string EDate, string RoomNo, [MarshalAs(UnmanagedType.LPStr)] StringBuilder CardHexStr);

        //Xóa thẻ
        [DllImport("proRFL.dll", CallingConvention = CallingConvention.StdCall)]
        private extern static int CardErase(byte fUSB, int dlsCoID, [MarshalAs(UnmanagedType.LPStr)] StringBuilder CardHexStr);

        //Lấy Lock No
        [DllImport("proRFL.dll", CallingConvention = CallingConvention.StdCall)]
        private extern static int GetGuestLockNoByCardDataStr(int dlsCoID, string CardDataStr, [MarshalAs(UnmanagedType.LPStr)] StringBuilder LockNo);

        //0:USB, 1:proUSB
        private static byte flagUSB = Convert.ToByte(1);

        private static IntPtr proModule = IntPtr.Zero;

        private static HotelLockResult initializeUSB()
        {
            proModule = LoadLibrary("proRFL.dll");
            if (proModule == IntPtr.Zero)
            {
                return new HotelLockResult(1998, "Không tìm thấy thư viện \"proRFL.dll\"");
            }
            int ret = initializeUSB(flagUSB);
            return new HotelLockResult(ret, ret == 0 ? "" : "Đầu đọc/ghi thẻ chưa kết nối, mã lỗi: " + ret);
        }

        public static BaseResult __GhiLaiThongTinThe(string LockNoNew, string LockNoOld, DateTime CheckIn, DateTime CheckOut, string startWith)
        {
            try
            {
                //Kiểm tra dữ liệu trên thẻ trước
                HotelLockResult rs = GetCardNumber(startWith);
                if (rs.ret)
                {
                    if (LockNoOld == rs.message)
                    {
                        rs = IssueCard(LockNoNew, CheckIn, CheckOut, startWith);
                        if (rs.ret)
                        {
                            return new OkResult();
                        }
                        else
                        {
                            return new ErrorResult(rs.message);
                        }
                    }
                    else
                    {
                        return new ErrorResult("Thẻ trên đầu đọc không đúng số yêu cầu!");
                    }
                }
                else
                {
                    return new ErrorResult(rs.message);
                }
            }
            catch (Exception ex)
            {
                return new OkResult(ex.Message);
            }
        }

        public static BaseResult __GhiThe(string LockNo, DateTime CheckIn, DateTime CheckOut, string startWith)
        {
            try
            {
                HotelLockResult rs = IssueCard(LockNo, CheckIn, CheckOut, startWith);
                if (rs.ret)
                {
                    return new OkResult();
                }
                else
                {
                    return new ErrorResult(rs.message);
                }
            }
            catch (Exception ex)
            {
                return new ErrorResult(ex.Message);
            }
        }

        public static BaseResult __XoaThe(string LockNo, string startWith)
        {
            try
            {
                HotelLockResult rs = EraseCard(LockNo, startWith);
                if (rs.ret)
                {
                    return new OkResult();
                }
                else
                {
                    return new ErrorResult(rs.message);
                }
            }
            catch (Exception ex)
            {
                return new ErrorResult(ex.Message);
            }
        }

        private static HotelLockResult GetCardNumber(string startWith)
        {
            HotelLockResult rs = initializeUSB();
            if (rs.ret)
            {
                //Đọc thẻ lấy chuỗi dữ liệu
                StringBuilder buffer = new StringBuilder(128);
                int ret = ReadCard(flagUSB, buffer);
                if (ret == 0) //success
                {
                    string dataHexStr = buffer.ToString();
                    if (dataHexStr.Length > 6 && dataHexStr.Substring(0, 6) == startWith)
                    {
                        int dlsCoID = 0;
                        ret = GetGuestLockNoByCardDataStr(dlsCoID, dataHexStr, buffer);
                        if (ret == 0) rs.message = buffer.ToString();
                    }
                }
                else
                {
                    rs.ret = false;
                    rs.message = "Lỗi đọc thẻ, mã lỗi: " + ret;
                }
            }
            return rs;
        }

        private static HotelLockResult ReadCard(string startWith)
        {
            HotelLockResult rs = initializeUSB();
            if (rs.ret)
            {
                StringBuilder buffer = new StringBuilder(128);
                int ret = ReadCard(flagUSB, buffer);
                if (ret != 0)
                {
                    rs.ret = false;
                    rs.message = "Lỗi đọc thẻ, mã lỗi " + ret;
                }
                else
                {
                    string dataHexStr = buffer.ToString();
                    if (dataHexStr.Length < 6 || dataHexStr.Substring(0, 6) != startWith)
                    {
                        rs.ret = false;
                        rs.message = "Không có thẻ, hoặc thẻ không đúng định dạng";
                    }
                }
            }
            return rs;
        }

        private static HotelLockResult IssueCard(string LockNo, DateTime CheckIn, DateTime CheckOut, string startWith)
        {
            int dlsCoID = 0;
            //Flag of Deadbolt
            byte llock = Convert.ToByte(1);
            //Flag of public door
            byte pdoors = 0;
            //Card No.(0-15)
            int CardNo = 10;
            //Guest Dai
            int dai = 0;
            //Time of Issue card, Just the current time,now
            string BDate = CheckIn.ToString("yyMMddHHmm");
            //Check Out
            string EDate = CheckOut.ToString("yyMMddHHmm");

            //Thực hiện ghi thẻ
            //1. Kiểm tra xem có đọc được thẻ không?
            HotelLockResult rs = ReadCard(startWith);
            if (rs.ret)
            {
                StringBuilder CardHexStr = new StringBuilder(128);

                //2. Ghi thông tin lên thẻ
                int ret = GuestCard(flagUSB, dlsCoID, CardNo, dai, llock, pdoors, BDate, EDate, LockNo, CardHexStr);

                //Thông báo kết quả
                if (ret != 0)
                {
                    rs.ret = false;
                    rs.message = "Không ghi được thẻ, mã lỗi : " + ret;
                }
                else
                {
                    rs.ret = true;
                    rs.message = "";
                }
            }
            return rs;
        }

        private static HotelLockResult EraseCard(string LockNo, string startWith)
        {
            int dlsCoID = 0;
            HotelLockResult rs;
            //Trường hợp này phải kiểm tra thẻ trên đầu ghi có đúng số không?
            if (!string.IsNullOrEmpty(LockNo))
            {
                rs = GetCardNumber(startWith);
                if (rs.ret)
                {
                    if (LockNo != rs.message) //Sai số thẻ không cho xóa
                    {
                        rs.ret = false;
                        rs.message = "Thẻ trên đầu đọc không đúng số yêu cầu " + LockNo;
                    }
                }
                else
                    return rs;
            }

            //Nếu trên đầu đọc có thẻ thì xóa
            rs = ReadCard(startWith);
            if (rs.ret)
            {
                StringBuilder CardHexStr = new StringBuilder(128);
                int ret = CardErase(flagUSB, dlsCoID, CardHexStr);
                if (ret != 0)
                {
                    rs.ret = false;
                    rs.message = "Lỗi xóa thẻ, mã lỗi: " + ret + Environment.NewLine + CardHexStr.ToString();
                }
                else
                {
                    return rs;
                }
            }
            return rs;
        }
    }

    class HotelLockResult
    {
        public HotelLockResult()
        {
        }

        public HotelLockResult(int _ret, string _msg)
        {
            this.ret = _ret == 0;
            this.message = _msg;
        }

        public bool ret { set; get; }

        public string message { set; get; }
    }
}
