using Domain.Abstractions.Common;
using Domain.Models.Criteria;

namespace Domain.Models
{
	public class Community : IPersistantModel<Community>, IEquatable<Community?>
	{
		public long Id { get; set; }
		public string Name { get; set; } = null!;
		public string Description { get; set; } = null!;
		public string ImageUid { get; set; } = null!;

		public override bool Equals(object? obj)
		{
			return Equals(obj as Community);
		}

		public bool Equals(Community? other)
		{
			return other is not null &&
				   Id == other.Id;
		}

		public ICriterion<Community> GetCriterion()
		{
			return new CommunityCriterion { Id = Id };
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Id);
		}

		public static bool operator ==(Community? left, Community? right)
		{
			return EqualityComparer<Community>.Default.Equals(left, right);
		}

		public static bool operator !=(Community? left, Community? right)
		{
			return !(left == right);
		}
	}
}
