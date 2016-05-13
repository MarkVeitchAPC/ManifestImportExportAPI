using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestImportExportAPI.Models
{
    public class ReturnedData<T>
    {


        public T Data { get; private set; }


        public ReturnedData(T data)
        {
            Data = data;
        }

        public static object ReturnData(T datum)
        {
            return new ReturnedData<T>(datum);
        }

        public static object ReturnData(IList<T> data)
        {
            return new ReturnedData<IList<T>>(data);
        }
    }
}