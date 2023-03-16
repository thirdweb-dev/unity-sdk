namespace Thirdweb
{
    public abstract class Routable
    {
        public static string separator = "/";
        public static string subSeparator = "#";

        public static string append(string route, string subRoute)
        {
            return $"{route}{separator}{subRoute}";
        }

        protected string baseRoute;

        public Routable(string baseRoute)
        {
            this.baseRoute = baseRoute;
        }

        protected string getRoute(string functionName)
        {
            return $"{baseRoute}{separator}{functionName}";
        }
    }
}
