namespace Plants.Initializer;

[ConfigSection(Section)]
internal class SeedingConfig
{
    const string Section = "Seeding";
    public bool ShouldSeed { get; set; } = false;
    public int PlantsCount { get; set; }
    public int UsersCount { get; set; }
    public int PriceRangeMin { get; set; }
    public int PriceRangeMax { get; set; }
}
