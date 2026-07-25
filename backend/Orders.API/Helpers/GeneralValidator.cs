using System;
using Orders.API.Exceptions;

namespace Orders.API.Helpers
{
    public  class GeneralValidator
    {
        public static void ValidateDataExists<T>(T? data, string message)
        {
            if (data == null)
            {
                throw new CustomException(404, message);
            }
        }
    }
}