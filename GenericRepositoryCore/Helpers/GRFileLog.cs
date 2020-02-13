
using System;
using System.IO;
using System.Web;

namespace GenericRepository.Helpers
{
    public class GRFileLog
    {
       

        public static void WriteToErrFileLog(string message, params object[] args)
        {
            
            //try
            //{
            //    if (HttpContext.Current == null) return;
            //    string fileName = HttpContext.Current.Server.MapPath(@"~/App_Data/GenericRepository.Errors.log");
            //    string line = string.Format("{0}: {1}{2}", DateTime.Now, string.Format(message, args), Environment.NewLine);
            //    File.AppendAllText(fileName, line);
            //}
            //catch { }
        }
    }
}
