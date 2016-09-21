using System.Reflection;

namespace LightMapper.Infrastructure
{
    /// <summary>Bindle of target and source class members</summary>
    public class MappingProperty
    {
        /// <summary>SourceT accessor info</summary>
        public MemberInfo SourceAccessor { get; private set; }
        /// <summary>TargetT accessor info</summary>
        public MemberInfo TargetAccessor { get; private set; }
        /// <summary>Use this bundle in mapping</summary>
        public bool InMapping { get; set; }

        /// <summary>MappingProperty constructor</summary>
        /// <param name="source">Source class accessor</param>
        /// <param name="target">Target class accessor</param>
        /// <param name="inMapping">Use this bundle in mapping</param>
        public MappingProperty(MemberInfo source, MemberInfo target, bool inMapping)
        {
            SourceAccessor = source;
            TargetAccessor = target;
            InMapping = inMapping;
        }

        /// <summary>Helper method that sets source class accessor</summary>
        /// <param name="member">MemberInfo object that describes source class accessor</param>
        public void SetSourceAccessor(MemberInfo member)
        {
            SourceAccessor = member;
            InMapping = true;
        }
    }
}