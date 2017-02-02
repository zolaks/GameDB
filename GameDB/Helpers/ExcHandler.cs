using System;

namespace GameDB.Helpers
{
    public static class ExcHandler
    {

        public static string Handle(string message, Exception exception)
        {

            return string.Format(string.Format("{0}: {1}", message, exception.Message));

        }

    }
}