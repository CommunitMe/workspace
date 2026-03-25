using System.Diagnostics.CodeAnalysis;

namespace Domain.Models.Enums
{
	public class ServiceDuration : IEquatable<ServiceDuration?>, IParsable<ServiceDuration>
	{

		private readonly int _id;
		private readonly string _name;

		private static IEnumerable<ServiceDuration> All()
		{
			yield return FullTime;
			yield return PartTime;
			yield return OneOff;
		}

		public static readonly ServiceDuration FullTime = new ServiceDuration(1, "FULL_TIME");
		public static readonly ServiceDuration PartTime = new ServiceDuration(2, "PART_TIME");
		public static readonly ServiceDuration OneOff = new ServiceDuration(3, "ONE_OFF");

		public int ID => _id;
		public string Name => _name;

		private ServiceDuration(int id, string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
			}

			_id = id;
			_name = name;
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as ServiceDuration);
		}

		public bool Equals(ServiceDuration? other)
		{
			return other is not null &&
				   _id == other._id &&
				   _name == other._name;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_id, _name);
		}

		public static bool operator ==(ServiceDuration? left, ServiceDuration? right)
		{
			return EqualityComparer<ServiceDuration>.Default.Equals(left, right);
		}

		public static bool operator !=(ServiceDuration? left, ServiceDuration? right)
		{
			return !(left == right);
		}

		public static explicit operator ServiceDuration(int id)
		{
			var result = All().First(i => i._id == id);

			if (result == null)
				throw new InvalidCastException($"'{id}' is not a valid ID for {typeof(ServiceDuration).Name}");

			return result;
		}

		public static explicit operator ServiceDuration(string name) => Parse(name, null);
		public static implicit operator int(ServiceDuration duration) => duration._id;
		public static implicit operator string(ServiceDuration duration) => duration._name;

		public override string? ToString()
		{
			return _name;
		}

		public static ServiceDuration Parse(string s, IFormatProvider? provider = null)
		{
			ServiceDuration? value;

			if (TryParse(s, provider, out value) == false)
				throw new InvalidCastException($"'{s}' is not a valid Name for {typeof(ServiceDuration).Name}");

			return value;
		}

		public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out ServiceDuration result)
		{
			return TryParse(s, null, out result);
		}

		public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ServiceDuration result)
		{
			result = All().First(i => i._name == s);

			return result != null;
		}
	}
}