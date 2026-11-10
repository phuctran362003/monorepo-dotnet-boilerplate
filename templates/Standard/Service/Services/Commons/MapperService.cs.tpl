using System.Reflection;

public class MapperService : IMapperService
{
    public TDestination Map<TSource, TDestination>(TSource source)
        where TSource : class
        where TDestination : class, new()
    {
        var destination = new TDestination();
        var sourceProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destProps = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var sourceProp in sourceProps)
        {
            var destProp = destProps.FirstOrDefault(property =>
                property.Name == sourceProp.Name && property.PropertyType == sourceProp.PropertyType);
            if (destProp is not null && destProp.CanWrite)
            {
                destProp.SetValue(destination, sourceProp.GetValue(source));
            }
        }

        return destination;
    }
}
