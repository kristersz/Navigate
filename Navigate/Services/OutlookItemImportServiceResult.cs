using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Navigate.Services
{
    public enum OutlookItemImportServiceResult
    {
        None,
        Ok,
        OkWithWarnings,
        NotImported,
        Error
    }
}