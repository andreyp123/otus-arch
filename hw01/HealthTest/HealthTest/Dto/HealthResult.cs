namespace HealthTest.Dto
{
    public class HealthResult
    {
        public string Status { get; set; }

        public static HealthResult OK
        {
            get { return new HealthResult { Status = "OK" }; }
        }
    }
}