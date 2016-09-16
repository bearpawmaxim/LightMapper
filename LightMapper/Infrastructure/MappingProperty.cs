using System.Reflection;

namespace LightMapper.Infrastructure
{
    public class MappingProperty
    {
        public MemberInfo SourceAccessor { get; private set; }
        public MemberInfo TargetAccessor { get; private set; }
        public bool IsProperty { get; private set; }
        public string Type { get; private set; }
        public bool IsNullable { get; private set; }
        public bool InMapping { get; set; }

        public MappingProperty(MemberInfo source, MemberInfo target, bool isProperty, string type, bool isNullable, bool inMapping)
        {
            SourceAccessor = source;
            TargetAccessor = target;
            IsProperty = isProperty;
            Type = type;
            IsNullable = isNullable;
            InMapping = inMapping;
        }

        public void SetSourceAccessor(MemberInfo member, bool isProperty)
        {
            SourceAccessor = member;
            IsProperty = isProperty;
            InMapping = true;
        }
    }
}