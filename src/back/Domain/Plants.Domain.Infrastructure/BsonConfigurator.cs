using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;

namespace Plants.Domain.Infrastructure;

public static class BsonConfigurator
{
    public static void Configure()
    {
        AddSerializers();

        var aggregateClassMap = GetAggregateClassMap();
        var helper = InfrastructureHelpers.Aggregate;

        foreach (var (_, type) in helper.Aggregates)
        {
            var baseMap = BuildBaseMap(aggregateClassMap, type);
            var map = new BsonClassMap(type, baseMap);
            var ctor = helper.AggregateCtors[type];
            map.MapConstructor(ctor, nameof(AggregateBase.Id));
            map.AutoMap();
            BsonClassMap.RegisterClassMap(map);
        }

    }

    private static BsonClassMap BuildBaseMap(BsonClassMap aggregateClassMap, Type type)
    {
        var baseClassMap = aggregateClassMap;
        var baseTypes = type.GetBaseTypes()
            .Where(_ => _.IsStrictlyAssignableTo(typeof(AggregateBase)))
            .Distinct()
            .Reverse();
        foreach (var baseType in baseTypes)
        {
            baseClassMap = new BsonClassMap(baseType, baseClassMap);
            baseClassMap.AutoMap();
        }
        return baseClassMap;
    }

    private static void AddSerializers()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        BsonSerializer.RegisterSerializer(typeof(Dictionary<long, string>), new PairDictionarySerializer<Dictionary<long, string>>());
    }

    private static BsonClassMap GetAggregateClassMap()
    {
        var aggregateClassMap = new BsonClassMap(typeof(AggregateBase));
        aggregateClassMap.MapProperty(nameof(AggregateBase.Version));
        aggregateClassMap.MapProperty(nameof(AggregateBase.CommandsProcessed));
        aggregateClassMap.MapProperty(nameof(AggregateBase.CommandsProcessedIds));
        aggregateClassMap.MapProperty(nameof(AggregateBase.Name));
        aggregateClassMap.MapProperty(nameof(AggregateBase.LastUpdateTime));
        aggregateClassMap.MapIdProperty(nameof(AggregateBase.Id));
        return aggregateClassMap;
    }

    private static List<Type> GetBaseTypes(this Type type)
    {
        List<Type> baseTypes = new();
        AddBaseTypes(type, baseTypes);
        return baseTypes;
    }

    private static void AddBaseTypes(Type type, List<Type> acc)
    {
        if (type.BaseType is not null)
        {
            acc.Add(type.BaseType);
            AddBaseTypes(type.BaseType, acc);
        }

        foreach (var @interface in type.GetInterfaces())
        {
            acc.Add(@interface);
            AddBaseTypes(@interface, acc);
        }
    }

    private class PairDictionarySerializer<TDictionary>
        : DictionarySerializerBase<TDictionary, long, string> where TDictionary : class, IEnumerable<KeyValuePair<long, string>>
    {
        public PairDictionarySerializer() : base(DictionaryRepresentation.Document, new Int64Serializer(BsonType.String), new StringSerializer())
        {

        }

        protected override ICollection<KeyValuePair<long, string>> CreateAccumulator()
        {
            return new Dictionary<long, string>();
        }
    }

}
