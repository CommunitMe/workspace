using System.Diagnostics.CodeAnalysis;

namespace Domain.Models.Enums
{
	public class EntityType : IEquatable<EntityType?>, IParsable<EntityType>
	{

		private readonly int _id;
		private readonly string _name;

		private static IEnumerable<EntityType> All()
		{
			yield return AllCategories;
			yield return Categories;
			yield return Coupons;
            yield return MarketItems;
            yield return Businesses;
			yield return Members;
		}

		public static readonly EntityType AllCategories = new EntityType(1, "ALL_CATEGORIES");
		public static readonly EntityType Categories = new EntityType(2, "Categories");
		public static readonly EntityType Coupons = new EntityType(3, "COUPONS");
        public static readonly EntityType MarketItems = new EntityType(4, "MARKET_ITEMS");
        public static readonly EntityType Businesses = new EntityType(5, "BUSINESSES");
		public static readonly EntityType Members = new EntityType(6, "MEMBERS");

		public int ID => _id;
		public string Name => _name;

		private EntityType(int id, string name)
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
			return Equals(obj as EntityType);
		}

		public bool Equals(EntityType? other)
		{
			return other is not null &&
				   _id == other._id &&
				   _name == other._name;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_id, _name);
		}

		public static bool operator ==(EntityType? left, EntityType? right)
		{
			return EqualityComparer<EntityType>.Default.Equals(left, right);
		}

		public static bool operator !=(EntityType? left, EntityType? right)
		{
			return !(left == right);
		}

		public static explicit operator EntityType(int id)
		{
			var result = All().First(i => i._id == id);

			if (result == null)
				throw new InvalidCastException($"'{id}' is not a valid ID for {typeof(EntityType).Name}");

			return result;
		}

		public static explicit operator EntityType(string name) => Parse(name, null);
		public static implicit operator int(EntityType duration) => duration._id;
		public static implicit operator string(EntityType duration) => duration._name;

		public override string? ToString()
		{
			return _name;
		}

		public static EntityType Parse(string s, IFormatProvider? provider = null)
		{
			EntityType? value;

			if (TryParse(s, provider, out value) == false)
				throw new InvalidCastException($"'{s}' is not a valid Name for {typeof(EntityType).Name}");

			return value;
		}

		public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out EntityType result)
		{
			return TryParse(s, null, out result);
		}

		public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out EntityType result)
		{
			result = All().First(i => i._name == s);

			return result != null;
		}

		public static IEnumerable<EntityType> AsEnumerable()
		{
			return All();
		}
	}
}
