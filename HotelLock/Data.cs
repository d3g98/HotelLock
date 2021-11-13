using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelLock
{
    //class Data
    //{
    //}

    public class BaseResult
    {
        public bool success { set; get; }
        public string message { set; get; }
        public object tag { set; get; }
    }

    public class OkResult : BaseResult
    {
        public OkResult()
        {
            success = true;
        }

        public OkResult(object data)
        {
            success = true;
            tag = data;
        }
    }

    public class ErrorResult : BaseResult
    {

        public ErrorResult(string msg)
        {
            success = false;
            message = msg;
        }
    }
}
