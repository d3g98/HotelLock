using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using HotelLock;

namespace HotelLock
{
    public class KOKLock
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("NewICdll.dll", SetLastError = true)]
        private static extern int OpenPort();

        [DllImport("NewICdll.dll", SetLastError = true)]
        private static extern int ClosePort();

        [DllImport("NewICdll.dll", SetLastError = true)]
        private static extern int ReadData(StringBuilder CardData, StringBuilder CNum, int nBeepFlag);

        [DllImport("NewICdll.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int IssueData([MarshalAs(UnmanagedType.LPStr)]string CData, [MarshalAs(UnmanagedType.LPStr)]string CNum, int nBeepFlag);

        [DllImport("NewICdll.dll", SetLastError = true)]
        private static extern int CancelCard([MarshalAs(UnmanagedType.LPStr)]string CNum, int nBeepFlag);

        private static IntPtr newModule = IntPtr.Zero;

        public static BaseResult __GhiLaiThongTinThe(string RoomAddressNew, string RoomAddressOld, DateTime cInTime, DateTime cOutTime)
        {
            try
            {
                newModule = LoadLibrary("NewICdll.dll");
                if (newModule == IntPtr.Zero)
                {
                    return new ErrorResult("Không tìm thấy thư viện \"NewICdll.dll\"");
                }
                int ret = OpenPort();
                Thread.Sleep(500);
                if (ret == 0)
                {
                    //Xóa thẻ
                    bool run = true;
                    string roomArrCheck = "";
                    if (RoomAddressOld != "")
                    {
                        StringBuilder CardData = new StringBuilder(255);
                        StringBuilder RoomNo = new StringBuilder(255);
                        ret = ReadData(CardData, RoomNo, 0);
                        if (ret == 0)
                        {
                            //string roomArrCheck = "";
                            string[] splitStrArr = CardData.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string val in splitStrArr)
                            {
                                if (val.Contains("R") && val.Contains("-"))
                                {
                                    roomArrCheck = val.Substring(1);
                                    break;
                                }
                            }
                            if (roomArrCheck == "")
                            {
                                return new ErrorResult("Lỗi trong quá trình kiểm tra thông tin: Mã phòng trống!");
                            }
                            else
                            {
                                if (roomArrCheck != RoomAddressOld) run = false;
                            }
                        }
                        else
                        {
                            return new ErrorResult("Lỗi trong quá trình kiểm tra thông tin: " + KOKMessage.GetMessage(ret));
                        }
                    }

                    if (run)
                    {
                        string roomNo = "";
                        ret = CancelCard(roomNo, 1);
                        if (ret == 0)
                        {
                            //Ghi thẻ
                            string data = "T0|R{0}|D{1}|O{2}|L0";
                            //T: Build.No
                            //R: RoomAddress
                            //D: CheckIn yyMMddHHmm
                            //O: CheckOut yyMMddHHmm
                            string RoomNo = "";
                            ret = IssueData(string.Format(data, RoomAddressNew, cInTime.ToString("yyMMddHHmm"), cOutTime.ToString("yyMMddHHmm")), RoomNo, 1);
                            if (ret == 0)
                            {
                                return new OkResult();
                            }
                            else
                            {
                                return new ErrorResult("Lỗi ghi thẻ mới: ;" + KOKMessage.GetMessage(ret));
                            }
                        }
                        else
                        {
                            //Lỗi xóa thẻ
                            return new ErrorResult("Lỗi ghi thẻ(Xóa thông tin): " + KOKMessage.GetMessage(ret));
                        }
                    }
                    else
                    {
                        return new ErrorResult("Lỗi ghi thẻ: Thẻ trên đầu đọc không đúng số yêu cầu!");
                    }
                }
                else
                {
                    return new ErrorResult("Lỗi ghi thẻ: Bạn không được mở phần mềm khóa khi sử dụng chức năng này!" + Environment.NewLine + KOKMessage.GetMessage(ret));
                }
            }
            catch (Exception ex)
            {
                return new ErrorResult(ex.Message);
            }
            finally
            {
                if (newModule != IntPtr.Zero) ClosePort();
            }
        }

        public static BaseResult __GhiThe(string RoomAddress, DateTime cInTime, DateTime cOutTime)
        {
            try
            {
                newModule = LoadLibrary("NewICdll.dll");
                if (newModule == IntPtr.Zero)
                {
                    return new ErrorResult("Không tìm thấy thư viện \"NewICdll.dll\"");
                }
                int ret = OpenPort();
                Thread.Sleep(500);
                if (ret == 0)
                {
                    //Ghi thẻ
                    string data = "T0|R{0}|D{1}|O{2}|L0";
                    //T: Build.No
                    //R: RoomAddress
                    //D: CheckIn yyMMddHHmm
                    //O: CheckOut yyMMddHHmm
                    string RoomNo = "";
                    ret = IssueData(string.Format(data, RoomAddress, cInTime.ToString("yyMMddHHmm"), cOutTime.ToString("yyMMddHHmm")), RoomNo, 1);
                    if (ret == 0)
                    {
                        return new OkResult();
                    }
                    else
                    {
                        return new ErrorResult("Lỗi ghi thẻ: ;" + KOKMessage.GetMessage(ret));
                    }
                }
                else
                {
                    return new ErrorResult("Lỗi ghi thẻ: Bạn không được mở phần mềm khóa khi sử dụng chức năng này!" + Environment.NewLine + KOKMessage.GetMessage(ret));
                }
            }
            catch (Exception ex)
            {
                return new ErrorResult(ex.Message);
            }
            finally
            {
                if (newModule != IntPtr.Zero) ClosePort();
            }
        }

        public static BaseResult __XoaThe(string info = "")
        {
            try
            {
                newModule = LoadLibrary("NewICdll.dll");
                if (newModule == IntPtr.Zero)
                {
                    return new ErrorResult("Không tìm thấy thư viện \"NewICdll.dll\"");
                }
                int ret = OpenPort();
                Thread.Sleep(500);
                if (ret == 0)
                {
                    //Xóa thẻ
                    bool run = true;
                    string roomArrCheck = "";
                    if (info != "")
                    {
                        StringBuilder CardData = new StringBuilder(255);
                        StringBuilder RoomNo = new StringBuilder(255);
                        ret = ReadData(CardData, RoomNo, 0);
                        if (ret == 0)
                        {
                            //string roomArrCheck = "";
                            string[] splitStrArr = CardData.ToString().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string val in splitStrArr)
                            {
                                if (val.Contains("R") && val.Contains("-"))
                                {
                                    roomArrCheck = val.Substring(1);
                                    break;
                                }
                            }
                            if (roomArrCheck == "")
                            {
                                return new ErrorResult("Lỗi trong quá trình kiểm tra thông tin: Mã phòng trống!");
                            }
                            else
                            {
                                if (roomArrCheck != info) run = false;
                            }
                        }
                        else
                        {
                            return new ErrorResult("Lỗi trong quá trình kiểm tra thông tin: " + KOKMessage.GetMessage(ret));
                        }
                    }

                    if (run)
                    {
                        string roomNo = "";
                        ret = CancelCard(roomNo, 1);
                        if (ret == 0)
                        {
                            return new OkResult();
                        }
                        else
                        {
                            return new ErrorResult("Lỗi ghi thẻ: " + KOKMessage.GetMessage(ret));
                        }
                    }
                    else
                    {
                        return new ErrorResult("Lỗi ghi thẻ: Thẻ trên đầu đọc không đúng số yêu cầu!");
                    }
                }
                else
                {
                    return new ErrorResult("Lỗi ghi thẻ: Bạn không được mở phần mềm khóa khi sử dụng chức năng này!" + Environment.NewLine + KOKMessage.GetMessage(ret));
                }
            }
            catch (Exception ex)
            {
                return new ErrorResult(ex.Message);
            }
            finally
            {
                if (newModule != IntPtr.Zero) ClosePort();
            }
        }
    }

    class KOKMessage
    {
        public static string GetMessage(int code)
        {
            switch (code)
            {
                case 0: return "Successful";
                case 1: return "No Card";
                case 2: return "Card Error";
                case 3: return "Password Error";
                case 4: return "Serial Port Communication Error";
                case 5: return "Authorization Error";
                case 7: return "New Card";
                case 10: return "Data Error";
                case 11: return "Configuration file Error";
                default: return "";
            }
        }
    }
}
