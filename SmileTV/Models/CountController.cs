using Prometheus;

namespace SmileTV.Models
{
    public static class CountController
    {
        public static Dictionary<string, Counter> Counters = new Dictionary<string, Counter>();
    }
}
