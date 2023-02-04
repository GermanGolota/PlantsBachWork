namespace Plants.Presentation;

public record ListViewResult<T>(List<T> Items)
{
	public ListViewResult(IEnumerable<T> items) : this(items.ToList())
	{

	}
}