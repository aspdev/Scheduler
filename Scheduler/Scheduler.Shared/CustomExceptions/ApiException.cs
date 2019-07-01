using System;
using System.Collections.Generic;
using System.Text;

namespace Scheduler.Shared.CustomExceptions
{
    public class ApiException : Exception
    {
       public string Content { get; set; }

    }
}
