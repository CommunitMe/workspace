namespace Domain.Infra.Globals
{
    public class MailResponse
    {
        public int StatusCode { get; set; }
        public Exception? Exception { get; set; }
    }
}