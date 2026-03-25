namespace Domain.Services.Abstractions
{
    public interface ISmsManager
    {
        bool Send(string to, string from, string subject, string body);
        bool Send(string[] to, string from, string subject, string body);
        Task<bool> SendAsync(string to, string from, string subject, string body);
        Task<bool> SendAsync(string[] to, string from, string subject, string body);
    }
}