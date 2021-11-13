using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using HotelLock;

namespace HotelLock
{
    public class HuneLock
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("hunerf.dll", SetLastError = true)]
        private static unsafe extern int ReadMessage(int Com, int nBlock, int Encrypt, int* DBCardNo, int* DBCardtype, int* DBPassLevel, string CardPass, string DBSystemcode, string DBAddress, string SDateTime);
        [DllImport("hunerf.dll", SetLastError = true)]
        private static extern int KeyCard(int Com, int CardNo, int nBlock, int Encrypt, string CardPass, string SystemCode, string HotelCode, string Pass, string Address, string SDIn,
           string STIn, string SDOut, string STOut, int LEVEL_Pass, int PassMode, int AddressMode, int AddressQty, int TimeMode, int V8, int V16, int V24, int AlwaysOpen, int OpenBolt,
           int TerminateOld, int ValidTimes);
        [DllImport("hunerf.dll", SetLastError = true)]
        private static extern int ReadCardSN(int Com, StringBuilder CardSN);
        private static IntPtr hModule = IntPtr.Zero;

        public static BaseResult __GhiThe(string HotelCode, string SystemCode, string roomAddress, DateTime cInTime, DateTime cOutTime)
        {
            try
            {
                hModule = LoadLibrary("HUNERF.dll");
                if (hModule == IntPtr.Zero)
                {
                    string abc = Directory.GetCurrentDirectory();
                    return new ErrorResult("Không tìm thấy thư viện \"hunerf.dll\"");
                }
                StringBuilder sb = _WriteCard(HotelCode, SystemCode, roomAddress, cInTime, cOutTime);
                if (sb.Length > 0)
                {
                    return new ErrorResult(sb.ToString());
                }
                return new OkResult();
            }
            catch (Exception ex)
            {
                return new ErrorResult(ex.Message);
            }
        }

        public static BaseResult __XoaThe(string HotelCode, string SystemCode, string roomAddress)
        {
            try
            {
                hModule = LoadLibrary("hunerf.dll");
                if (hModule == IntPtr.Zero)
                {
                    return new ErrorResult("Không tìm thấy thư viện \"hunerf.dll\"");
                }
                DateTime timeClear = new DateTime(2010, 1, 1);
                StringBuilder sb = _WriteCard(HotelCode, SystemCode, roomAddress, timeClear, timeClear);
                if (sb.Length > 0)
                {
                    return new ErrorResult(sb.ToString());
                }
                return new OkResult();
            }
            catch (Exception ex)
            {
                return new ErrorResult(ex.Message);
            }
        }

        private static StringBuilder _WriteCard(string HotelCode, string SystemCode, string roomAddress, DateTime cInTime, DateTime cOutTime)
        {
            StringBuilder sb = new StringBuilder();
            int Com = 7;
            int CardNo = 2;
            int nBlock = 4;
            int Encrypt = 1;
            string CardPass = "555";
            string RoomPass = "555";
            string Address = roomAddress;
            string DTPSDInVar = string.Format("{0:yy-MM-dd}", cInTime).ToString();
            string DTPSTInVar = cInTime.ToString("hh:nn:ss");
            string DTPSDOutVar = string.Format("{0:yy-MM-dd}", cOutTime).ToString();
            string DTPSTOutVar = cOutTime.ToString("hh:nn:ss");

            int LevelPass = 3;
            int PassMode = 1;
            int AddressMode = 0;
            int AddressQty = 1;
            int TimeMode = 0;
            int V8 = 255;
            int V16 = 255;
            int V24 = 255;
            int AlwaysOpen = 0;
            int OpenBolt = 1;
            int TerminateOld = 1;
            RoomPass = _GoDateTime(DateTime.Now);

            int ValidTimes = 255;
            try
            {
                int Ret = KeyCard(Com, CardNo, nBlock, Encrypt, CardPass, SystemCode, HotelCode, RoomPass, Address, DTPSDInVar, DTPSTInVar, DTPSDOutVar, DTPSTOutVar
                     , LevelPass, PassMode, AddressMode, AddressQty, TimeMode, V8, V16, V24, AlwaysOpen, OpenBolt, TerminateOld, ValidTimes);
                if (Ret == 0)
                {
                }
                else
                {
                    sb.AppendLine("Lỗi ghi thẻ, mã lỗi：" + Ret.ToString());
                }
            }
            catch (Exception err)
            {
                sb.AppendLine(err.Message);
            }
            return sb;
        }

        private static string _GoDateTime(DateTime DTVar)
        {
            long DW1 = 0;
            string DW2 = "";
            long Year = DTVar.Year;
            long Month = DTVar.Month;
            long Day = DTVar.Day;
            long Hour = DTVar.Hour;
            long Min = DTVar.Minute;
            long Sec = DTVar.Second;
            DW1 = (Min / 4) + (Hour << 4) + (Day << 9) + (Month << 14) + ((((Year - 8) % 1000) % 63) << 18);
            DW2 = Convert.ToString(DW1, 16);
            return DW2.ToUpper();
        }
    }
}
