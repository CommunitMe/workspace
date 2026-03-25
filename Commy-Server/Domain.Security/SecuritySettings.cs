namespace Domain.Security
{
	public class SecuritySettings
	{
		public static string Key = "SecuritySettings";

		public string JWTAuthority { get; set; } = string.Empty;
		public string[] CORSOrigins { get; set; } = new string[] { };
	}
}
