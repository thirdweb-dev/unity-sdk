namespace Thirdweb {
    public abstract class Routable {
        protected string baseRoute;

        public Routable(string baseRoute) {
            this.baseRoute = baseRoute;
        }

        protected string getRoute(string functionName) {
            return $"{baseRoute}.{functionName}";
        }
    }
}