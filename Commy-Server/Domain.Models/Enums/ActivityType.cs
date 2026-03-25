using System.Diagnostics.CodeAnalysis;

namespace Domain.Models.Enums
{
	public class ActivityType : IEquatable<ActivityType?>, IParsable<ActivityType>
	{

		private readonly int _id;
		private readonly string _name;

		private static IEnumerable<ActivityType> All()
		{
			yield return LookingForService;
			yield return NewServiceProvider;
			yield return Selling;
			yield return StatsDeals;
		}

		public static readonly ActivityType LookingForService = new ActivityType(1, "LOOKING_FOR_SERVICE");
		public static readonly ActivityType NewServiceProvider = new ActivityType(2, "NEW_SERVICE_PROVIDER");
		public static readonly ActivityType Selling = new ActivityType(3, "SELLING");
		public static readonly ActivityType StatsDeals = new ActivityType(4, "STATS_DEALS");

		public int ID => _id;
		public string Name => _name;

		private ActivityType(int id, string name)
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
			return Equals(obj as ActivityType);
		}

		public bool Equals(ActivityType? other)
		{
			return other is not null &&
				   _id == other._id &&
				   _name == other._name;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_id, _name);
		}

		public static bool operator ==(ActivityType? left, ActivityType? right)
		{
			return EqualityComparer<ActivityType>.Default.Equals(left, right);
		}

		public static bool operator !=(ActivityType? left, ActivityType? right)
		{
			return !(left == right);
		}

		public static explicit operator ActivityType(int id)
		{
			var result = All().First(i => i._id == id);

			if (result == null)
				throw new InvalidCastException($"'{id}' is not a valid ID for {typeof(ActivityType).Name}");

			return result;
		}

		public static explicit operator ActivityType(string name) => Parse(name, null);
		public static implicit operator int(ActivityType duration) => duration._id;
		public static implicit operator string(ActivityType duration) => duration._name;

		public override string? ToString()
		{
			return _name;
		}

		public static ActivityType Parse(string s, IFormatProvider? provider = null)
		{
			ActivityType? value;

			if (TryParse(s, provider, out value) == false)
				throw new InvalidCastException($"'{s}' is not a valid Name for {typeof(ActivityType).Name}");

			return value;
		}

		public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out ActivityType result)
		{
			return TryParse(s, null, out result);
		}

		public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ActivityType result)
		{
			result = All().First(i => i._name == s);

			return result != null;
		}
	}
}